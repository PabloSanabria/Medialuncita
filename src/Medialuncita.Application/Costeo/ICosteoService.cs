using Medialuncita.Application.Costeo.Dtos;
using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Costeo;

/// <summary>
/// Motor de costeo. 100% determinístico: misma entrada -> misma salida, siempre.
/// No tiene ninguna dependencia de IA, red, ubicación ni fuentes externas.
/// Cualquier sugerencia externa de precio debe pasar antes por el flujo normal
/// de HistorialPrecio antes de llegar acá; este servicio nunca "estima" nada.
/// </summary>
public interface ICosteoService
{
    /// <summary>
    /// Calcula el costo completo de UNA variante de producto (ingredientes, packaging,
    /// mano de obra y servicios), incluyendo el efecto de la merma.
    /// </summary>
    /// <param name="receta">Receta madre, con Ingredientes y Servicios cargados.</param>
    /// <param name="variante">Variante a costear, con IngredienteOverrides, Materiales y Servicios cargados.</param>
    /// <param name="precioVigentePorIngredienteId">Precio vigente de cada IngredienteId usado,
    /// expresado en la UnidadCompra de ese ingrediente (que ya viene cargada en la entidad).</param>
    /// <param name="precioVigentePorMaterialId">Ídem para materiales, en su UnidadCompra.</param>
    /// <param name="tarifaManoDeObraPorHora">Tarifa efectiva (override de config global si no hay uno propio).</param>
    ResultadoCosteo CalcularCosto(
        Receta receta,
        ProductoVariante variante,
        IReadOnlyDictionary<int, decimal> precioVigentePorIngredienteId,
        IReadOnlyDictionary<int, decimal> precioVigentePorMaterialId,
        decimal tarifaManoDeObraPorHora);

    /// <summary>Calcula el precio de venta a partir de un costo unitario y una estrategia.</summary>
    ResultadoPrecioVenta CalcularPrecioVenta(
        decimal costoUnitario,
        Domain.Enums.EstrategiaPrecio estrategia,
        decimal? margenPorcentual,
        decimal? multiplicador,
        decimal? precioManual,
        Domain.Enums.EstrategiaRedondeo redondeo);
}
