namespace Medialuncita.Domain.Entities;

/// <summary>
/// Receta "madre": define las proporciones de ingredientes para un rendimiento
/// de referencia (RendimientoBaseCantidad). Las variantes de producto escalan
/// o sobreescriben estas proporciones (ver ProductoVariante).
/// </summary>
public class Receta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>Cantidad de rendimiento de referencia (ej: 20 en "rinde 20 porciones").</summary>
    public decimal RendimientoBaseCantidad { get; set; }

    public int RendimientoBaseUnidadId { get; set; }
    public UnidadMedida? RendimientoBaseUnidad { get; set; }

    /// <summary>
    /// Tiempo de preparación del LOTE completo de la receta base (mezclar, hornear, etc.),
    /// NO por unidad. Se prorratea entre las porciones/unidades del rendimiento al costear
    /// (ver ManoDeObraCalculo en Application). Distinto del tiempo adicional por variante.
    /// </summary>
    public int TiempoPreparacionBaseMinutos { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<RecetaIngrediente> Ingredientes { get; set; } = new List<RecetaIngrediente>();
    public ICollection<RecetaServicio> Servicios { get; set; } = new List<RecetaServicio>();
}

/// <summary>
/// Cantidad de un ingrediente necesaria para el RendimientoBaseCantidad de la receta.
/// </summary>
public class RecetaIngrediente
{
    public int Id { get; set; }

    public int RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public int IngredienteId { get; set; }
    public Ingrediente? Ingrediente { get; set; }

    public decimal Cantidad { get; set; }

    public int UnidadId { get; set; }
    public UnidadMedida? Unidad { get; set; }

    /// <summary>Si tiene valor, reemplaza a Ingrediente.MermaDefault solo para esta receta.</summary>
    public decimal? MermaOverride { get; set; }
}
