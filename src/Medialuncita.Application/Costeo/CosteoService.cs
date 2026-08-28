using Medialuncita.Application.Costeo.Dtos;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;

namespace Medialuncita.Application.Costeo;

public class CosteoService : ICosteoService
{
    public ResultadoCosteo CalcularCosto(
        Receta receta,
        ProductoVariante variante,
        IReadOnlyDictionary<int, PrecioVigente> precioVigentePorIngredienteId,
        IReadOnlyDictionary<int, PrecioVigente> precioVigentePorMaterialId,
        decimal tarifaManoDeObraPorHora)
    {
        ArgumentNullException.ThrowIfNull(receta);
        ArgumentNullException.ThrowIfNull(variante);

        if (receta.RendimientoBaseUnidad is null)
            throw new InvalidOperationException("La receta no tiene RendimientoBaseUnidad cargada.");
        if (variante.RendimientoUnidad is null)
            throw new InvalidOperationException("La variante no tiene RendimientoUnidad cargada.");
        if (receta.RendimientoBaseUnidad.Tipo != variante.RendimientoUnidad.Tipo)
        {
            throw new InvalidOperationException(
                $"El rendimiento de la variante ({variante.RendimientoUnidad.Tipo}) debe ser del mismo " +
                $"tipo que el de la receta madre ({receta.RendimientoBaseUnidad.Tipo}).");
        }
        if (receta.RendimientoBaseCantidad <= 0)
            throw new InvalidOperationException("RendimientoBaseCantidad de la receta debe ser mayor a cero.");
        if (variante.RendimientoCantidad <= 0)
            throw new InvalidOperationException("RendimientoCantidad de la variante debe ser mayor a cero.");

        var factorEscala = variante.RendimientoCantidad / receta.RendimientoBaseCantidad;

        var (detalleIngredientes, costoIngredientes) = CalcularIngredientes(
            receta, variante, factorEscala, precioVigentePorIngredienteId);

        var (detalleMateriales, costoPackaging) = CalcularMateriales(
            variante, precioVigentePorMaterialId);

        var tiempoTotalMinutos = CalcularTiempoTotalMinutos(receta, variante, factorEscala);
        var costoManoDeObra = tiempoTotalMinutos / 60m * tarifaManoDeObraPorHora;

        var (detalleServicios, costoServicios) = CalcularServicios(
            receta, variante, tiempoTotalMinutos);

        var costoTotal = costoIngredientes + costoPackaging + costoManoDeObra + costoServicios;
        var costoUnitario = costoTotal / variante.RendimientoCantidad;

        return new ResultadoCosteo(
            VarianteId: variante.Id,
            RendimientoCantidad: variante.RendimientoCantidad,
            Ingredientes: detalleIngredientes,
            Materiales: detalleMateriales,
            Servicios: detalleServicios,
            TiempoTotalMinutos: tiempoTotalMinutos,
            TarifaManoDeObraPorHora: tarifaManoDeObraPorHora,
            CostoIngredientes: costoIngredientes,
            CostoPackaging: costoPackaging,
            CostoManoDeObra: costoManoDeObra,
            CostoServicios: costoServicios,
            CostoTotal: costoTotal,
            CostoUnitario: costoUnitario);
    }

    private static (List<DetalleIngredienteCosteo>, decimal costoTotal) CalcularIngredientes(
        Receta receta,
        ProductoVariante variante,
        decimal factorEscala,
        IReadOnlyDictionary<int, PrecioVigente> precios)
    {
        var overridesPorIngrediente = variante.IngredienteOverrides
            .ToDictionary(o => o.IngredienteId);

        var detalle = new List<DetalleIngredienteCosteo>();
        decimal costoTotal = 0m;

        foreach (var recetaIngrediente in receta.Ingredientes)
        {
            if (recetaIngrediente.Ingrediente is null || recetaIngrediente.Unidad is null)
            {
                throw new InvalidOperationException(
                    $"RecetaIngrediente {recetaIngrediente.Id} no tiene Ingrediente/Unidad cargados.");
            }

            decimal cantidadRequerida;
            UnidadMedida unidadRequerida;

            if (overridesPorIngrediente.TryGetValue(recetaIngrediente.IngredienteId, out var over))
            {
                if (over.Unidad is null)
                    throw new InvalidOperationException($"Override {over.Id} no tiene Unidad cargada.");
                cantidadRequerida = over.CantidadOverride;
                unidadRequerida = over.Unidad;
            }
            else
            {
                cantidadRequerida = recetaIngrediente.Cantidad * factorEscala;
                unidadRequerida = recetaIngrediente.Unidad;
            }

            var merma = recetaIngrediente.MermaOverride ?? recetaIngrediente.Ingrediente.MermaDefault;
            if (merma is < 0 or >= 1)
            {
                throw new InvalidOperationException(
                    $"Merma inválida ({merma}) para el ingrediente '{recetaIngrediente.Ingrediente.Nombre}'. Debe estar en [0, 1).");
            }

            // Merma matemáticamente correcta: se necesita MÁS cantidad "cruda" para
            // obtener la cantidad neta requerida, no se resta al final.
            var cantidadEfectiva = cantidadRequerida / (1 - merma);

            if (!precios.TryGetValue(recetaIngrediente.IngredienteId, out var precioVigente))
            {
                throw new InvalidOperationException(
                    $"No hay precio vigente cargado para el ingrediente '{recetaIngrediente.Ingrediente.Nombre}'.");
            }

            var cantidadEfectivaBase = ConversionUnidades.ConvertirAUnidadBase(
                cantidadEfectiva, unidadRequerida, precioVigente.Unidad.Tipo,
                recetaIngrediente.Ingrediente.DensidadGramosPorMililitro);

            var precioPorUnidadBase = ConversionUnidades.PrecioPorUnidadBase(precioVigente.Precio, precioVigente.Unidad);
            var subtotal = cantidadEfectivaBase * precioPorUnidadBase;

            costoTotal += subtotal;

            detalle.Add(new DetalleIngredienteCosteo(
                NombreIngrediente: recetaIngrediente.Ingrediente.Nombre,
                CantidadRequerida: cantidadRequerida,
                UnidadRequerida: unidadRequerida.Abreviatura,
                MermaAplicada: merma,
                CantidadEfectivaEnUnidadBase: cantidadEfectivaBase,
                PrecioUnitarioUsado: precioPorUnidadBase,
                Subtotal: subtotal));
        }

        return (detalle, costoTotal);
    }

    private static (List<DetalleMaterialCosteo>, decimal costoTotal) CalcularMateriales(
        ProductoVariante variante,
        IReadOnlyDictionary<int, PrecioVigente> precios)
    {
        var detalle = new List<DetalleMaterialCosteo>();
        decimal costoTotal = 0m;

        foreach (var vm in variante.Materiales)
        {
            if (vm.Material is null)
                throw new InvalidOperationException($"VarianteMaterial {vm.Id} no tiene Material cargado.");

            var merma = vm.Material.MermaDefault;
            if (merma is < 0 or >= 1)
            {
                throw new InvalidOperationException(
                    $"Merma inválida ({merma}) para el material '{vm.Material.Nombre}'. Debe estar en [0, 1).");
            }

            var cantidadEfectiva = vm.Cantidad / (1 - merma);

            if (!precios.TryGetValue(vm.MaterialId, out var precioVigente))
            {
                throw new InvalidOperationException(
                    $"No hay precio vigente cargado para el material '{vm.Material.Nombre}'.");
            }

            // Los materiales no tienen densidad: solo se admite conversión dentro del mismo Tipo.
            if (vm.Material.UnidadCompra is not null && vm.Material.UnidadCompra.Tipo != precioVigente.Unidad.Tipo)
            {
                throw new InvalidOperationException(
                    $"El material '{vm.Material.Nombre}' tiene un precio cargado en una unidad de tipo distinto a su unidad de compra.");
            }

            var unidadCantidad = vm.Material.UnidadCompra ?? precioVigente.Unidad;
            var cantidadEfectivaBase = ConversionUnidades.ConvertirAUnidadBase(
                cantidadEfectiva, unidadCantidad, precioVigente.Unidad.Tipo);

            var precioPorUnidadBase = ConversionUnidades.PrecioPorUnidadBase(precioVigente.Precio, precioVigente.Unidad);
            var subtotal = cantidadEfectivaBase * precioPorUnidadBase;

            costoTotal += subtotal;

            detalle.Add(new DetalleMaterialCosteo(
                NombreMaterial: vm.Material.Nombre,
                CantidadRequerida: vm.Cantidad,
                MermaAplicada: merma,
                CantidadEfectivaEnUnidadBase: cantidadEfectivaBase,
                UnidadBase: precioVigente.Unidad.Abreviatura,
                PrecioUnitarioUsado: precioPorUnidadBase,
                Subtotal: subtotal));
        }

        return (detalle, costoTotal);
    }

    /// <summary>
    /// Distingue explícitamente 3 componentes de tiempo, tal como lo requiere el modelo:
    ///  1) Tiempo de preparación de la receta/lote (prorrateado por escala: una tanda más
    ///     grande necesita proporcionalmente más tiempo de mezclado/horneado).
    ///  2) Tiempo adicional POR LOTE de la variante (armado general, fijo, no escala por unidad).
    ///  3) Tiempo adicional POR UNIDAD de la variante (decoración individual, se multiplica
    ///     por la cantidad de unidades del rendimiento).
    /// Ninguno de los tres asume que "todo escala proporcionalmente": cada uno tiene su
    /// propia regla.
    /// </summary>
    private static int CalcularTiempoTotalMinutos(Receta receta, ProductoVariante variante, decimal factorEscala)
    {
        var tiempoBaseProrrateado = receta.TiempoPreparacionBaseMinutos * factorEscala;
        var tiempoPorLote = variante.TiempoAdicionalPorLoteMinutos;
        var tiempoPorUnidad = variante.TiempoAdicionalPorUnidadMinutos * variante.RendimientoCantidad;

        var total = tiempoBaseProrrateado + tiempoPorLote + tiempoPorUnidad;
        return (int)Math.Round(total, MidpointRounding.AwayFromZero);
    }

    private static (List<DetalleServicioCosteo>, decimal costoTotal) CalcularServicios(
        Receta receta,
        ProductoVariante variante,
        int tiempoTotalMinutos)
    {
        var detalle = new List<DetalleServicioCosteo>();
        decimal costoTotal = 0m;
        var horas = tiempoTotalMinutos / 60m;

        foreach (var rs in receta.Servicios)
        {
            var (linea, subtotal) = ResolverServicio(rs.Servicio, rs.ModoProrrateo, horas);
            detalle.Add(linea);
            costoTotal += subtotal;
        }

        foreach (var vs in variante.Servicios)
        {
            var (linea, subtotal) = ResolverServicio(vs.Servicio, vs.ModoProrrateo, horas);
            detalle.Add(linea);
            costoTotal += subtotal;
        }

        return (detalle, costoTotal);
    }

    private static (DetalleServicioCosteo, decimal) ResolverServicio(Servicio? servicio, ModoProrrateo modo, decimal horas)
    {
        if (servicio is null)
            throw new InvalidOperationException("Servicio no cargado en la relación Receta/VarianteServicio.");

        var subtotal = modo switch
        {
            ModoProrrateo.PorHora => horas * (servicio.CostoPorHora ?? 0m),
            ModoProrrateo.PorLote => servicio.CostoPorLote ?? 0m,
            _ => throw new InvalidOperationException($"ModoProrrateo no soportado: {modo}")
        };

        return (new DetalleServicioCosteo(servicio.Nombre, modo, subtotal), subtotal);
    }

    public ResultadoPrecioVenta CalcularPrecioVenta(
        decimal costoUnitario,
        EstrategiaPrecio estrategia,
        decimal? margenPorcentual,
        decimal? multiplicador,
        decimal? precioManual,
        EstrategiaRedondeo redondeo)
    {
        decimal precioSinRedondeo = estrategia switch
        {
            EstrategiaPrecio.Margen => costoUnitario * (1 + (margenPorcentual
                ?? throw new InvalidOperationException("Falta MargenPorcentual para la estrategia Margen."))),

            EstrategiaPrecio.Multiplicador => costoUnitario * (multiplicador
                ?? throw new InvalidOperationException("Falta Multiplicador para la estrategia Multiplicador.")),

            EstrategiaPrecio.Manual => precioManual
                ?? throw new InvalidOperationException("Falta PrecioManual para la estrategia Manual."),

            _ => throw new InvalidOperationException($"Estrategia no soportada: {estrategia}")
        };

        var precioFinal = AplicarRedondeo(precioSinRedondeo, redondeo);

        return new ResultadoPrecioVenta(
            estrategia, margenPorcentual, multiplicador, redondeo, precioSinRedondeo, precioFinal);
    }

    private static decimal AplicarRedondeo(decimal precio, EstrategiaRedondeo redondeo) => redondeo switch
    {
        EstrategiaRedondeo.SinRedondeo => precio,
        EstrategiaRedondeo.RedondeoEntero => Math.Ceiling(precio),
        EstrategiaRedondeo.RedondeoMultiploDe50 => Math.Ceiling(precio / 50m) * 50m,
        EstrategiaRedondeo.RedondeoMultiploDe100 => Math.Ceiling(precio / 100m) * 100m,
        _ => throw new InvalidOperationException($"EstrategiaRedondeo no soportada: {redondeo}")
    };
}
