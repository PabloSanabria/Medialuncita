namespace Medialuncita.Domain.Entities;

/// <summary>
/// Ingrediente de repostería. NO tiene un campo de precio propio:
/// el precio vigente se resuelve consultando HistorialPrecioIngrediente
/// (el registro con la Fecha más reciente). Esto evita tener dos fuentes
/// de verdad para el precio.
/// </summary>
public class Ingrediente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }

    /// <summary>Unidad en la que habitualmente se compra (ej: kg, l, unidad).</summary>
    public int UnidadCompraId { get; set; }
    public UnidadMedida? UnidadCompra { get; set; }

    /// <summary>Merma por defecto (0 a 1, ej 0.05 = 5%). Puede sobreescribirse por receta.</summary>
    public decimal MermaDefault { get; set; }

    /// <summary>
    /// Densidad en gramos por mililitro (g/ml). Opcional: solo necesaria si el
    /// ingrediente se va a usar en recetas con una unidad de un Tipo distinto
    /// al de su UnidadCompra (ej: comprar harina en kg pero recetar en tazas/ml).
    /// </summary>
    public decimal? DensidadGramosPorMililitro { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<HistorialPrecioIngrediente> HistorialPrecios { get; set; } = new List<HistorialPrecioIngrediente>();
}
