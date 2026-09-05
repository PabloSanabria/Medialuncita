namespace Medialuncita.Domain.Entities;

/// <summary>
/// Registro histórico de precio de un ingrediente. Es la ÚNICA fuente de verdad
/// de precios: "precio vigente" = registro con Fecha máxima para ese IngredienteId.
/// Cargar un precio nuevo siempre es un INSERT, nunca un UPDATE sobre un campo suelto.
///
/// Deliberadamente simple: solo Fecha y Precio. La unidad del precio es siempre la
/// UnidadCompra del ingrediente (no se repite acá, evita redundancia e inconsistencias).
/// Si en el futuro hace falta registrar de dónde salió un precio (IA, mercado, INDEC),
/// alcanza con agregar una columna nueva (ej. una "Fuente" nullable) en una migración
/// posterior, sin romper nada de lo existente. No se agrega ahora para no sumar
/// complejidad que todavía no se usa.
/// </summary>
public class HistorialPrecioIngrediente
{
    public int Id { get; set; }

    public int IngredienteId { get; set; }
    public Ingrediente? Ingrediente { get; set; }

    public DateTime Fecha { get; set; }

    /// <summary>Precio expresado en la UnidadCompra del ingrediente.</summary>
    public decimal Precio { get; set; }
}
