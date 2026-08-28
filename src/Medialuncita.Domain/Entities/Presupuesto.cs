using Medialuncita.Domain.Enums;

namespace Medialuncita.Domain.Entities;

public class Presupuesto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? ClienteNombre { get; set; }
    public string? Notas { get; set; }

    /// <summary>Suma de PresupuestoItem.Subtotal. Se recalcula al agregar/quitar ítems, pero
    /// una vez guardado el presupuesto, cada ítem ya tiene su precio congelado (snapshot).</summary>
    public decimal Total { get; set; }

    public ICollection<PresupuestoItem> Items { get; set; } = new List<PresupuestoItem>();
}

/// <summary>
/// Ítem de un presupuesto. Contiene el resultado COMPLETO y CONGELADO del cálculo
/// de costeo en el momento de generación: no depende de que la receta, la variante
/// o los precios actuales sigan siendo los mismos. Por eso casi todos los campos
/// tienen sufijo "Snapshot": son una copia de datos, no una referencia viva.
/// Las FK a ProductoVarianteId/ProductoId se conservan solo a fines de trazabilidad
/// (poder decir "esto se generó a partir de tal variante"), nunca se usan para recalcular.
/// </summary>
public class PresupuestoItem
{
    public int Id { get; set; }

    public int PresupuestoId { get; set; }
    public Presupuesto? Presupuesto { get; set; }

    /// <summary>Trazabilidad únicamente. Puede quedar huérfana si la variante se borra después.</summary>
    public int? ProductoVarianteId { get; set; }

    public string NombreProductoSnapshot { get; set; } = string.Empty;
    public string NombreVarianteSnapshot { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    // ---- Totales congelados del costeo (salida de CosteoService, ya multiplicado x Cantidad) ----
    public decimal CostoIngredientesSnapshot { get; set; }
    public decimal CostoPackagingSnapshot { get; set; }
    public decimal CostoManoDeObraSnapshot { get; set; }
    public decimal CostoServiciosSnapshot { get; set; }
    public decimal CostoTotalSnapshot { get; set; }
    public decimal CostoUnitarioSnapshot { get; set; }

    // ---- Congelado de mano de obra (para poder auditar cómo se llegó al costo) ----
    public int TiempoTotalMinutosSnapshot { get; set; }
    public decimal TarifaManoDeObraPorHoraSnapshot { get; set; }

    // ---- Congelado de la estrategia de precio usada ----
    public EstrategiaPrecio EstrategiaPrecioSnapshot { get; set; }
    public decimal? MargenPorcentualSnapshot { get; set; }
    public decimal? MultiplicadorSnapshot { get; set; }
    public EstrategiaRedondeo EstrategiaRedondeoSnapshot { get; set; }

    /// <summary>Precio de venta unitario congelado al momento de generar el presupuesto.</summary>
    public decimal PrecioUnitarioAlMomento { get; set; }

    /// <summary>PrecioUnitarioAlMomento * Cantidad.</summary>
    public decimal Subtotal { get; set; }

    public ICollection<PresupuestoItemIngredienteDetalle> DetalleIngredientes { get; set; } = new List<PresupuestoItemIngredienteDetalle>();
    public ICollection<PresupuestoItemMaterialDetalle> DetalleMateriales { get; set; } = new List<PresupuestoItemMaterialDetalle>();
    public ICollection<PresupuestoItemServicioDetalle> DetalleServicios { get; set; } = new List<PresupuestoItemServicioDetalle>();
}

/// <summary>Línea congelada de un ingrediente usado en el cálculo (post-escala, post-merma).</summary>
public class PresupuestoItemIngredienteDetalle
{
    public int Id { get; set; }

    public int PresupuestoItemId { get; set; }
    public PresupuestoItem? PresupuestoItem { get; set; }

    public string NombreIngredienteSnapshot { get; set; } = string.Empty;
    public decimal CantidadRequeridaSnapshot { get; set; }
    public decimal MermaAplicadaSnapshot { get; set; }
    public decimal CantidadEfectivaSnapshot { get; set; }
    public string UnidadSnapshot { get; set; } = string.Empty;
    public decimal PrecioUnitarioUsadoSnapshot { get; set; }
    public decimal SubtotalSnapshot { get; set; }
}

/// <summary>Línea congelada de un material de packaging usado en el cálculo.</summary>
public class PresupuestoItemMaterialDetalle
{
    public int Id { get; set; }

    public int PresupuestoItemId { get; set; }
    public PresupuestoItem? PresupuestoItem { get; set; }

    public string NombreMaterialSnapshot { get; set; } = string.Empty;
    public decimal CantidadRequeridaSnapshot { get; set; }
    public decimal MermaAplicadaSnapshot { get; set; }
    public decimal CantidadEfectivaSnapshot { get; set; }
    public string UnidadSnapshot { get; set; } = string.Empty;
    public decimal PrecioUnitarioUsadoSnapshot { get; set; }
    public decimal SubtotalSnapshot { get; set; }
}

/// <summary>Línea congelada de un servicio prorrateado en el cálculo.</summary>
public class PresupuestoItemServicioDetalle
{
    public int Id { get; set; }

    public int PresupuestoItemId { get; set; }
    public PresupuestoItem? PresupuestoItem { get; set; }

    public string NombreServicioSnapshot { get; set; } = string.Empty;
    public string ModoProrrateoSnapshot { get; set; } = string.Empty;
    public decimal SubtotalSnapshot { get; set; }
}
