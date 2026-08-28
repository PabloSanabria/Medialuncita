using Medialuncita.Application.Costeo.Dtos;
using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Costeo;

/// <summary>Precio vigente de un ingrediente o material, ya resuelto desde el historial.</summary>
public readonly record struct PrecioVigente(decimal Precio, UnidadMedida Unidad);

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
    /// <param name="precioVigentePorIngredienteId">Precio vigente resuelto para cada IngredienteId usado.</param>
    /// <param name="precioVigentePorMaterialId">Precio vigente resuelto para cada MaterialId usado.</param>
    /// <param name="tarifaManoDeObraPorHora">Tarifa efectiva (override de config global si no hay uno propio).</param>
    ResultadoCosteo CalcularCosto(
        Receta receta,
        ProductoVariante variante,
        IReadOnlyDictionary<int, PrecioVigente> precioVigentePorIngredienteId,
        IReadOnlyDictionary<int, PrecioVigente> precioVigentePorMaterialId,
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
