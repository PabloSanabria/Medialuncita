namespace Medialuncita.Domain.Enums;

/// <summary>
/// Magnitud física que representa una unidad de medida.
/// Solo se pueden convertir/comparar unidades del mismo tipo,
/// salvo el caso Peso &lt;-&gt; Volumen mediado por la densidad del ingrediente.
/// </summary>
public enum TipoUnidad
{
    Peso = 1,
    Volumen = 2,
    Unidad = 3
}
