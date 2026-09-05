using Medialuncita.Application.Abstractions;
using Medialuncita.Application.Costeo;
using Medialuncita.Application.Precios;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;

namespace Medialuncita.Application.Presupuestos;

public sealed record ItemPresupuestoRequest(int ProductoVarianteId, decimal Cantidad);

public interface IPresupuestoService
{
    /// <summary>
    /// Arma y persiste un presupuesto nuevo. Para cada ítem: costea la variante con
    /// CosteoService (precios vigentes AL MOMENTO), calcula el precio de venta, y
    /// congela absolutamente todo el detalle en el PresupuestoItem/snapshot. Una vez
    /// guardado, el presupuesto no vuelve a leer precios ni recetas actuales.
    /// </summary>
    Task<int> GenerarPresupuestoAsync(
        IReadOnlyList<ItemPresupuestoRequest> items,
        string? clienteNombre,
        string? notas,
        DateTime fecha,
        CancellationToken ct = default);
}

public class PresupuestoService : IPresupuestoService
{
    private readonly IProductoRepository _productos;
    private readonly IPresupuestoRepository _presupuestos;
    private readonly IConfiguracionGlobalRepository _config;
    private readonly IPrecioConsultaService _precios;
    private readonly ICosteoService _costeo;
    private readonly IUnitOfWork _uow;

    public PresupuestoService(
        IProductoRepository productos,
        IPresupuestoRepository presupuestos,
        IConfiguracionGlobalRepository config,
        IPrecioConsultaService precios,
        ICosteoService costeo,
        IUnitOfWork uow)
    {
        _productos = productos;
        _presupuestos = presupuestos;
        _config = config;
        _precios = precios;
        _costeo = costeo;
        _uow = uow;
    }

    public async Task<int> GenerarPresupuestoAsync(
        IReadOnlyList<ItemPresupuestoRequest> items,
        string? clienteNombre,
        string? notas,
        DateTime fecha,
        CancellationToken ct = default)
    {
        if (items.Count == 0)
            throw new ArgumentException("El presupuesto debe tener al menos un ítem.", nameof(items));

        var configuracion = await _config.GetAsync(ct);
        var presupuesto = new Presupuesto { Fecha = fecha, ClienteNombre = clienteNombre, Notas = notas };

        foreach (var request in items)
        {
            var variante = await _productos.GetVarianteParaCosteoAsync(request.ProductoVarianteId, ct)
                ?? throw new InvalidOperationException($"No se encontró la variante {request.ProductoVarianteId}.");

            var producto = variante.Producto
                ?? throw new InvalidOperationException($"La variante {request.ProductoVarianteId} no tiene Producto cargado.");
            var receta = producto.Receta
                ?? throw new InvalidOperationException($"El producto {producto.Id} no tiene Receta cargada.");

            var precioIngredientes = new Dictionary<int, decimal>();
            foreach (var ri in receta.Ingredientes)
            {
                precioIngredientes[ri.IngredienteId] = await _precios.GetPrecioVigenteIngredienteAsync(ri.IngredienteId, ct);
            }

            var precioMateriales = new Dictionary<int, decimal>();
            foreach (var vm in variante.Materiales)
            {
                precioMateriales[vm.MaterialId] = await _precios.GetPrecioVigenteMaterialAsync(vm.MaterialId, ct);
            }

            var resultadoCosteo = _costeo.CalcularCosto(
                receta, variante, precioIngredientes, precioMateriales, configuracion.TarifaManoDeObraPorHora);

            var estrategia = variante.EstrategiaPrecioOverride ?? configuracion.EstrategiaPrecioDefault;
            var redondeo = variante.EstrategiaRedondeoOverride ?? configuracion.EstrategiaRedondeoDefault;
            var margen = variante.MargenPorcentualOverride ?? configuracion.MargenPorcentualDefault;
            var multiplicador = variante.MultiplicadorOverride ?? configuracion.MultiplicadorDefault;

            var resultadoPrecio = _costeo.CalcularPrecioVenta(
                resultadoCosteo.CostoUnitario,
                estrategia,
                margenPorcentual: estrategia == EstrategiaPrecio.Margen ? margen : null,
                multiplicador: estrategia == EstrategiaPrecio.Multiplicador ? multiplicador : null,
                precioManual: estrategia == EstrategiaPrecio.Manual ? variante.PrecioManualOverride : null,
                redondeo: redondeo);

            var item = ConstruirItemConSnapshot(producto, variante, request.Cantidad, resultadoCosteo, resultadoPrecio);
            presupuesto.Items.Add(item);
        }

        presupuesto.Total = presupuesto.Items.Sum(i => i.Subtotal);

        await _presupuestos.AddAsync(presupuesto, ct);
        await _uow.SaveChangesAsync(ct); // recién acá SQLite asigna el Id autoincremental
        return presupuesto.Id;
    }

    private static PresupuestoItem ConstruirItemConSnapshot(
        Producto producto,
        ProductoVariante variante,
        decimal cantidad,
        Costeo.Dtos.ResultadoCosteo costeo,
        Costeo.Dtos.ResultadoPrecioVenta precio)
    {
        var item = new PresupuestoItem
        {
            ProductoVarianteId = variante.Id,
            NombreProductoSnapshot = producto.Nombre,
            NombreVarianteSnapshot = variante.Nombre,
            Cantidad = cantidad,

            CostoIngredientesSnapshot = costeo.CostoIngredientes,
            CostoPackagingSnapshot = costeo.CostoPackaging,
            CostoManoDeObraSnapshot = costeo.CostoManoDeObra,
            CostoServiciosSnapshot = costeo.CostoServicios,
            CostoTotalSnapshot = costeo.CostoTotal,
            CostoUnitarioSnapshot = costeo.CostoUnitario,

            TiempoTotalMinutosSnapshot = costeo.TiempoTotalMinutos,
            TarifaManoDeObraPorHoraSnapshot = costeo.TarifaManoDeObraPorHora,

            EstrategiaPrecioSnapshot = precio.Estrategia,
            MargenPorcentualSnapshot = precio.MargenPorcentual,
            MultiplicadorSnapshot = precio.Multiplicador,
            EstrategiaRedondeoSnapshot = precio.Redondeo,

            PrecioUnitarioAlMomento = precio.PrecioUnitarioFinal,
            Subtotal = precio.PrecioUnitarioFinal * cantidad
        };

        foreach (var di in costeo.Ingredientes)
        {
            item.DetalleIngredientes.Add(new PresupuestoItemIngredienteDetalle
            {
                NombreIngredienteSnapshot = di.NombreIngrediente,
                CantidadRequeridaSnapshot = di.CantidadRequerida,
                MermaAplicadaSnapshot = di.MermaAplicada,
                CantidadEfectivaSnapshot = di.CantidadEfectivaEnUnidadBase,
                UnidadSnapshot = di.UnidadRequerida,
                PrecioUnitarioUsadoSnapshot = di.PrecioUnitarioUsado,
                SubtotalSnapshot = di.Subtotal
            });
        }

        foreach (var dm in costeo.Materiales)
        {
            item.DetalleMateriales.Add(new PresupuestoItemMaterialDetalle
            {
                NombreMaterialSnapshot = dm.NombreMaterial,
                CantidadRequeridaSnapshot = dm.CantidadRequerida,
                MermaAplicadaSnapshot = dm.MermaAplicada,
                CantidadEfectivaSnapshot = dm.CantidadEfectivaEnUnidadBase,
                UnidadSnapshot = dm.UnidadBase,
                PrecioUnitarioUsadoSnapshot = dm.PrecioUnitarioUsado,
                SubtotalSnapshot = dm.Subtotal
            });
        }

        foreach (var ds in costeo.Servicios)
        {
            item.DetalleServicios.Add(new PresupuestoItemServicioDetalle
            {
                NombreServicioSnapshot = ds.NombreServicio,
                ModoProrrateoSnapshot = ds.ModoProrrateo.ToString(),
                SubtotalSnapshot = ds.Subtotal
            });
        }

        return item;
    }
}
