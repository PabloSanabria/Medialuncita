using Medialuncita.Domain.Enums;

namespace Medialuncita.Domain.Entities;

/// <summary>
/// Producto vendible, originado en una Receta madre. El Producto en sí no tiene
/// costo: el costo se calcula siempre a nivel ProductoVariante.
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<ProductoVariante> Variantes { get; set; } = new List<ProductoVariante>();
}

/// <summary>
/// Variante concreta y vendible de un Producto (ej: "Rogel individual", "Rogel 12 porciones").
/// NO depende de un único FactorAjuste genérico: escala la receta madre por
/// RendimientoCantidad/RendimientoBaseCantidad, pero permite overrides puntuales
/// por ingrediente (VarianteIngredienteOverride) y siempre declara su propio
/// packaging (VarianteMateriales), porque el packaging no escala linealmente.
/// </summary>
public class ProductoVariante
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Rendimiento de ESTA variante (ej: 1 para "individual", 12 para "12 porciones").</summary>
    public decimal RendimientoCantidad { get; set; }

    /// <summary>
    /// Debe ser del mismo Tipo (Peso/Volumen/Unidad) que Receta.RendimientoBaseUnidad
    /// para poder calcular el factor de escala. Se valida en Application, no acá.
    /// </summary>
    public int RendimientoUnidadId { get; set; }
    public UnidadMedida? RendimientoUnidad { get; set; }

    /// <summary>
    /// Tiempo de armado/decoración/empaquetado propio de ESTA variante, POR LOTE
    /// (no se multiplica por la cantidad de unidades vendidas dentro del lote).
    /// Ej: armar y bañar una tanda de rogeles individuales.
    /// </summary>
    public int TiempoAdicionalPorLoteMinutos { get; set; }

    /// <summary>
    /// Tiempo de armado/decoración propio de ESTA variante, POR UNIDAD/PORCIÓN vendida.
    /// Ej: decorar cada porción individualmente. Se multiplica por RendimientoCantidad
    /// al costear el lote completo.
    /// </summary>
    public int TiempoAdicionalPorUnidadMinutos { get; set; }

    // --- Configuración de precio de venta (puede heredar de ConfiguracionGlobal si es null) ---
    public EstrategiaPrecio? EstrategiaPrecioOverride { get; set; }
    public decimal? MargenPorcentualOverride { get; set; }
    public decimal? MultiplicadorOverride { get; set; }
    public decimal? PrecioManualOverride { get; set; }
    public EstrategiaRedondeo? EstrategiaRedondeoOverride { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<VarianteIngredienteOverride> IngredienteOverrides { get; set; } = new List<VarianteIngredienteOverride>();
    public ICollection<VarianteMaterial> Materiales { get; set; } = new List<VarianteMaterial>();
    public ICollection<VarianteServicio> Servicios { get; set; } = new List<VarianteServicio>();
}
