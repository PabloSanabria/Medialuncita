namespace Medialuncita.Domain.Entities;

/// <summary>
/// Registro histórico de precio de un material de packaging. Mismo mecanismo
/// que HistorialPrecioIngrediente (ver esa clase para la justificación de diseño).
/// </summary>
public class HistorialPrecioMaterial
{
    public int Id { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public DateTime Fecha { get; set; }
    public decimal Precio { get; set; }

    public int UnidadId { get; set; }
    public UnidadMedida? Unidad { get; set; }

    public string Fuente { get; set; } = "Manual";
}
