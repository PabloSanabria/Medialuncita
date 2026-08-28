namespace Medialuncita.Domain.Entities;

/// <summary>
/// Material de packaging (cajas, etiquetas, film, bandejas, etc.).
/// Mismo patrón que Ingrediente: sin precio propio, se resuelve por historial.
/// </summary>
public class Material
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }

    public int UnidadCompraId { get; set; }
    public UnidadMedida? UnidadCompra { get; set; }

    /// <summary>Merma por defecto (0 a 1). Ej: etiquetas que se arruinan al pegar.</summary>
    public decimal MermaDefault { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<HistorialPrecioMaterial> HistorialPrecios { get; set; } = new List<HistorialPrecioMaterial>();
}
