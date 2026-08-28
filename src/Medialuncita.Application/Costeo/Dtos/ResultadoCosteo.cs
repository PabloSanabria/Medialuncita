using Medialuncita.Domain.Enums;

namespace Medialuncita.Application.Costeo.Dtos;

/// <summary>
/// Línea de detalle de un ingrediente ya resuelto (post-escala, post-override, post-merma,
/// post-conversión de unidades y con precio vigente aplicado).
/// </summary>
public sealed record DetalleIngredienteCosteo(
    string NombreIngrediente,
    decimal CantidadRequerida,
    string UnidadRequerida,
    decimal MermaAplicada,
    decimal CantidadEfectivaEnUnidadBase,
    decimal PrecioUnitarioUsado,
    decimal Subtotal);

public sealed record DetalleMaterialCosteo(
    string NombreMaterial,
    decimal CantidadRequerida,
    decimal MermaAplicada,
    decimal CantidadEfectivaEnUnidadBase,
    string UnidadBase,
    decimal PrecioUnitarioUsado,
    decimal Subtotal);

public sealed record DetalleServicioCosteo(
    string NombreServicio,
    ModoProrrateo ModoProrrateo,
    decimal Subtotal);

/// <summary>
/// Resultado completo, determinístico, del costeo de UNA variante de producto.
/// Es la salida de CosteoService.CalcularCosto(...) y la entrada para:
///   - calcular el precio de venta (CalculadorPrecioVenta)
///   - generar el snapshot de un PresupuestoItem
/// No tiene ninguna dependencia de infraestructura, UI ni IA: es una función pura
/// sobre los datos que se le pasan.
/// </summary>
public sealed record ResultadoCosteo(
    int VarianteId,
    decimal RendimientoCantidad,
    IReadOnlyList<DetalleIngredienteCosteo> Ingredientes,
    IReadOnlyList<DetalleMaterialCosteo> Materiales,
    IReadOnlyList<DetalleServicioCosteo> Servicios,
    int TiempoTotalMinutos,
    decimal TarifaManoDeObraPorHora,
    decimal CostoIngredientes,
    decimal CostoPackaging,
    decimal CostoManoDeObra,
    decimal CostoServicios,
    decimal CostoTotal,
    decimal CostoUnitario);

public sealed record ResultadoPrecioVenta(
    EstrategiaPrecio Estrategia,
    decimal? MargenPorcentual,
    decimal? Multiplicador,
    EstrategiaRedondeo Redondeo,
    decimal PrecioUnitarioSinRedondeo,
    decimal PrecioUnitarioFinal);
