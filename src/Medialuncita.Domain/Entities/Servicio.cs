using Medialuncita.Domain.Enums;

namespace Medialuncita.Domain.Entities;

/// <summary>
/// Servicio prorrateable (gas, electricidad, agua, alquiler, etc.).
/// Puede configurarse con costo por hora, costo por lote, o ambos
/// (el ModoProrrateo de cada relación Receta/Variante decide cuál se usa).
/// </summary>
public class Servicio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public decimal? CostoPorHora { get; set; }
    public decimal? CostoPorLote { get; set; }

    public bool Activo { get; set; } = true;
}

/// <summary>Servicio aplicable a la receta madre (ej: gas del horno para todo el lote).</summary>
public class RecetaServicio
{
    public int Id { get; set; }

    public int RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public ModoProrrateo ModoProrrateo { get; set; }
}

/// <summary>Servicio adicional aplicable a una variante puntual (ej: horno extra para decoración).</summary>
public class VarianteServicio
{
    public int Id { get; set; }

    public int VarianteId { get; set; }
    public ProductoVariante? Variante { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public ModoProrrateo ModoProrrateo { get; set; }
}
