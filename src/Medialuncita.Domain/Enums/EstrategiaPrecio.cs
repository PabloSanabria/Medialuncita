namespace Medialuncita.Domain.Enums;

/// <summary>
/// Estrategia utilizada para calcular el precio de venta a partir del costo unitario.
/// El diseño contempla 4 estrategias desde el modelo, aunque el MVP de UI
/// solo exponga Margen y Multiplicador inicialmente.
/// </summary>
public enum EstrategiaPrecio
{
    /// <summary>PrecioVenta = CostoUnitario * (1 + Margen%).</summary>
    Margen = 1,

    /// <summary>PrecioVenta = CostoUnitario * Multiplicador.</summary>
    Multiplicador = 2,

    /// <summary>El usuario fija el precio manualmente, ignorando costo/margen.</summary>
    Manual = 3
}

/// <summary>
/// Estrategia de redondeo aplicada al precio de venta final, luego de calcular
/// por Margen, Multiplicador o Manual. Independiente de la estrategia de precio.
/// </summary>
public enum EstrategiaRedondeo
{
    SinRedondeo = 0,
    RedondeoEntero = 1,
    RedondeoMultiploDe50 = 2,
    RedondeoMultiploDe100 = 3
}
