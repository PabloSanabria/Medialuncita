using Medialuncita.Domain.Enums;

namespace Medialuncita.Domain.Entities;

/// <summary>
/// Unidad de medida (kg, g, l, ml, unidad, docena, etc.).
/// Cada unidad tiene un factor de conversión a la unidad BASE de su Tipo:
///   Peso   -> base = gramo (g)
///   Volumen-> base = mililitro (ml)
///   Unidad -> base = unidad (u)
/// Ejemplo: Kilogramo.FactorAUnidadBase = 1000 (1 kg = 1000 g).
/// </summary>
public class UnidadMedida
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public TipoUnidad Tipo { get; set; }

    /// <summary>Factor multiplicativo para convertir 1 unidad de esta medida a la unidad base de su Tipo.</summary>
    public decimal FactorAUnidadBase { get; set; }
}
