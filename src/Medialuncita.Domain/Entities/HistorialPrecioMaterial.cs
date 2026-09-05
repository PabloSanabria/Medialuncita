namespace Medialuncita.Domain.Entities;

/// <summary>
/// Registro histórico de precio de un material de packaging. Mismo mecanismo y misma
/// justificación de diseño que HistorialPrecioIngrediente (ver esa clase).
/// </summary>
public class HistorialPrecioMaterial
{
    public int Id { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public DateTime Fecha { get; set; }

    /// <summary>Precio expresado en la UnidadCompra del material.</summary>
    public decimal Precio { get; set; }
}
