namespace Medialuncita.Domain.Entities;

/// <summary>
/// Registro histórico de precio de un ingrediente. Es la ÚNICA fuente de verdad
/// de precios: "precio vigente" = registro con Fecha máxima para ese IngredienteId.
/// Cargar un precio nuevo siempre es un INSERT, nunca un UPDATE sobre un campo suelto.
/// </summary>
public class HistorialPrecioIngrediente
{
    public int Id { get; set; }

    public int IngredienteId { get; set; }
    public Ingrediente? Ingrediente { get; set; }

    public DateTime Fecha { get; set; }

    /// <summary>Precio expresado en la unidad indicada por UnidadId.</summary>
    public decimal Precio { get; set; }

    public int UnidadId { get; set; }
    public UnidadMedida? Unidad { get; set; }

    /// <summary>
    /// Origen del precio. En el MVP siempre "Manual". Reservado para cuando existan
    /// fuentes externas (IA, mercado, INDEC): esas fuentes solo pueden GENERAR una
    /// sugerencia; si el usuario la acepta, se inserta un registro acá con Fuente
    /// distinta a "Manual", pero por el mismo camino de código.
    /// </summary>
    public string Fuente { get; set; } = "Manual";
}
