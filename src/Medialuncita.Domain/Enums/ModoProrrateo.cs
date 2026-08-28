namespace Medialuncita.Domain.Enums;

/// <summary>
/// Define cómo se prorratea el costo de un servicio (gas, luz, alquiler, etc.)
/// sobre una receta o variante.
/// </summary>
public enum ModoProrrateo
{
    /// <summary>Se cobra en función del tiempo de uso (horas) de la receta/variante.</summary>
    PorHora = 1,

    /// <summary>Se cobra un costo fijo por lote/tanda, sin importar el tiempo.</summary>
    PorLote = 2
}
