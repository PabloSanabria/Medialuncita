using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;

namespace Medialuncita.Application.Tests;

/// <summary>
/// Construye un grafo de dominio mínimo y realista para testear el motor de costeo,
/// inspirado en el caso guía del proyecto: "Rogel" con variantes individual / 12 porciones.
/// Todo en memoria, sin EF Core, para que los tests del motor sean puros y rápidos.
/// </summary>
internal static class TestDataBuilder
{
    public static UnidadMedida Gramo => new() { Id = 1, Nombre = "Gramo", Abreviatura = "g", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1 };
    public static UnidadMedida Kilogramo => new() { Id = 2, Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
    public static UnidadMedida Mililitro => new() { Id = 3, Nombre = "Mililitro", Abreviatura = "ml", Tipo = TipoUnidad.Volumen, FactorAUnidadBase = 1 };
    public static UnidadMedida Litro => new() { Id = 4, Nombre = "Litro", Abreviatura = "l", Tipo = TipoUnidad.Volumen, FactorAUnidadBase = 1000 };
    public static UnidadMedida Taza => new() { Id = 5, Nombre = "Taza", Abreviatura = "taza", Tipo = TipoUnidad.Volumen, FactorAUnidadBase = 250 };
    public static UnidadMedida Unidad => new() { Id = 6, Nombre = "Unidad", Abreviatura = "u", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
    public static UnidadMedida Porcion => new() { Id = 7, Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
    public static UnidadMedida Docena => new() { Id = 8, Nombre = "Docena", Abreviatura = "docena", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 12 };

    public static Ingrediente Harina() => new()
    {
        Id = 1,
        Nombre = "Harina 0000",
        UnidadCompraId = Kilogramo.Id,
        UnidadCompra = Kilogramo,
        MermaDefault = 0.02m // 2%
    };

    public static Ingrediente DulceDeLeche() => new()
    {
        Id = 2,
        Nombre = "Dulce de leche",
        UnidadCompraId = Kilogramo.Id,
        UnidadCompra = Kilogramo,
        MermaDefault = 0m
    };

    public static Material CajaIndividual() => new()
    {
        Id = 1,
        Nombre = "Caja individual",
        UnidadCompraId = Unidad.Id,
        UnidadCompra = Unidad,
        MermaDefault = 0m
    };

    public static Servicio Gas() => new() { Id = 1, Nombre = "Gas", CostoPorHora = 60m, CostoPorLote = null };

    /// <summary>
    /// Receta madre "Rogel": rinde 20 porciones, usa 1kg de harina y 500g de dulce de leche,
    /// lleva 90 minutos de preparación de lote, y usa gas prorrateado por hora.
    /// </summary>
    public static Receta RecetaRogel()
    {
        var harina = Harina();
        var dulce = DulceDeLeche();

        return new Receta
        {
            Id = 1,
            Nombre = "Rogel",
            RendimientoBaseCantidad = 20m,
            RendimientoBaseUnidadId = Porcion.Id,
            RendimientoBaseUnidad = Porcion,
            TiempoPreparacionBaseMinutos = 90,
            Ingredientes = new List<RecetaIngrediente>
            {
                new() { Id = 1, RecetaId = 1, IngredienteId = harina.Id, Ingrediente = harina, Cantidad = 1, UnidadId = Kilogramo.Id, Unidad = Kilogramo },
                new() { Id = 2, RecetaId = 1, IngredienteId = dulce.Id, Ingrediente = dulce, Cantidad = 500, UnidadId = Gramo.Id, Unidad = Gramo }
            },
            Servicios = new List<RecetaServicio>
            {
                new() { Id = 1, RecetaId = 1, ServicioId = Gas().Id, Servicio = Gas(), ModoProrrateo = ModoProrrateo.PorHora }
            }
        };
    }

    /// <summary>Variante "Rogel 12 porciones": escala linealmente (factor 12/20 = 0.6), sin overrides.</summary>
    public static ProductoVariante Variante12Porciones() => new()
    {
        Id = 1,
        Nombre = "Rogel 12 porciones",
        RendimientoCantidad = 12m,
        RendimientoUnidadId = Porcion.Id,
        RendimientoUnidad = Porcion,
        TiempoAdicionalPorLoteMinutos = 15, // armado general de la torta
        TiempoAdicionalPorUnidadMinutos = 0,
        IngredienteOverrides = new List<VarianteIngredienteOverride>(),
        Materiales = new List<VarianteMaterial>(),
        Servicios = new List<VarianteServicio>()
    };

    /// <summary>
    /// Variante "Rogel individual": rinde 1, con override de dulce de leche (proporcionalmente
    /// lleva más relleno que la torta grande), packaging propio y tiempo por unidad.
    /// </summary>
    public static ProductoVariante VarianteIndividual()
    {
        var dulce = DulceDeLeche();
        var caja = CajaIndividual();

        return new ProductoVariante
        {
            Id = 2,
            Nombre = "Rogel individual",
            RendimientoCantidad = 1m,
            RendimientoUnidadId = Porcion.Id,
            RendimientoUnidad = Porcion,
            TiempoAdicionalPorLoteMinutos = 0,
            TiempoAdicionalPorUnidadMinutos = 3, // armado y decoración individual
            IngredienteOverrides = new List<VarianteIngredienteOverride>
            {
                new() { Id = 1, VarianteId = 2, IngredienteId = dulce.Id, Ingrediente = dulce, CantidadOverride = 40, UnidadId = Gramo.Id, Unidad = Gramo }
            },
            Materiales = new List<VarianteMaterial>
            {
                new() { Id = 1, VarianteId = 2, MaterialId = caja.Id, Material = caja, Cantidad = 1 }
            },
            Servicios = new List<VarianteServicio>()
        };
    }

    /// <summary>Precio vigente por ingrediente, expresado en su UnidadCompra (ya no lleva unidad propia).</summary>
    public static IReadOnlyDictionary<int, decimal> PreciosIngredientesDefault() => new Dictionary<int, decimal>
    {
        [Harina().Id] = 1500m, // $1500 / kg
        [DulceDeLeche().Id] = 3000m // $3000 / kg
    };

    public static IReadOnlyDictionary<int, decimal> PreciosMaterialesDefault() => new Dictionary<int, decimal>
    {
        [CajaIndividual().Id] = 200m // $200 / unidad
    };
}
