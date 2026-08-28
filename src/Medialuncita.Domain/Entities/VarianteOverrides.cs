namespace Medialuncita.Domain.Entities;

/// <summary>
/// Sobreescribe, para una variante puntual, la cantidad de un ingrediente que
/// de otro modo se calcularía escalando automáticamente la receta madre.
/// Uso: cuando la proporción de un ingrediente NO es lineal respecto al
/// rendimiento (ej: más merengue proporcional en la porción individual que en la de 20).
/// Si NO existe override para un ingrediente de la receta, se usa Cantidad * FactorEscala.
/// </summary>
public class VarianteIngredienteOverride
{
    public int Id { get; set; }

    public int VarianteId { get; set; }
    public ProductoVariante? Variante { get; set; }

    public int IngredienteId { get; set; }
    public Ingrediente? Ingrediente { get; set; }

    public decimal CantidadOverride { get; set; }

    public int UnidadId { get; set; }
    public UnidadMedida? Unidad { get; set; }
}

/// <summary>
/// Packaging propio de una variante. NUNCA se escala automáticamente desde la
/// receta madre (una caja individual no es "1/20 de una caja"): cada variante
/// declara explícitamente qué materiales usa y en qué cantidad.
/// </summary>
public class VarianteMaterial
{
    public int Id { get; set; }

    public int VarianteId { get; set; }
    public ProductoVariante? Variante { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public decimal Cantidad { get; set; }
}
