using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Domain.Entities;
using Xunit;

namespace Medialuncita.Application.Tests;

/// <summary>
/// Casos guía pedidos explícitamente para validar la conversión entre la unidad de
/// COMPRA de un ingrediente y la unidad usada en la RECETA. Cada test usa una receta/variante
/// aislada (rendimiento 1, sin merma, sin mano de obra ni servicios) para que el costo total
/// sea exactamente el costo del ingrediente, sin ruido de otros componentes.
/// </summary>
public class ConversionPrecioCompraTests
{
    private readonly CosteoService _sut = new();

    [Fact]
    public void Harina_CompradaPorKilo_UsadaEnGramos()
    {
        // Harina a $2500/kg, receta usa 300 g -> costo esperado $750
        var resultado = CostearIngredienteUnico(
            unidadCompra: TestDataBuilder.Kilogramo,
            precioPorUnidadCompra: 2500m,
            cantidadEnReceta: 300m,
            unidadReceta: TestDataBuilder.Gramo);

        resultado.Should().Be(750m);
    }

    [Fact]
    public void Huevos_CompradosPorDocena_UsadosEnUnidades()
    {
        // Huevos a $6000/docena, receta usa 3 unidades -> costo esperado $1500
        var resultado = CostearIngredienteUnico(
            unidadCompra: TestDataBuilder.Docena,
            precioPorUnidadCompra: 6000m,
            cantidadEnReceta: 3m,
            unidadReceta: TestDataBuilder.Unidad);

        resultado.Should().Be(1500m);
    }

    [Fact]
    public void Leche_CompradaPorLitro_UsadaEnMililitros()
    {
        // Leche a $1500/litro, receta usa 250 ml -> costo esperado $375
        var resultado = CostearIngredienteUnico(
            unidadCompra: TestDataBuilder.Litro,
            precioPorUnidadCompra: 1500m,
            cantidadEnReceta: 250m,
            unidadReceta: TestDataBuilder.Mililitro);

        resultado.Should().Be(375m);
    }

    /// <summary>Arma una receta/variante mínima con un único ingrediente, sin merma,
    /// sin mano de obra ni servicios, y devuelve el CostoTotal (== costo de ese ingrediente).</summary>
    private decimal CostearIngredienteUnico(
        UnidadMedida unidadCompra,
        decimal precioPorUnidadCompra,
        decimal cantidadEnReceta,
        UnidadMedida unidadReceta)
    {
        var ingrediente = new Ingrediente
        {
            Id = 1,
            Nombre = "Ingrediente de prueba",
            UnidadCompraId = unidadCompra.Id,
            UnidadCompra = unidadCompra,
            MermaDefault = 0m
        };

        var unidadPorcion = TestDataBuilder.Porcion;

        var receta = new Receta
        {
            Id = 1,
            Nombre = "Receta de prueba",
            RendimientoBaseCantidad = 1m,
            RendimientoBaseUnidadId = unidadPorcion.Id,
            RendimientoBaseUnidad = unidadPorcion,
            TiempoPreparacionBaseMinutos = 0,
            Ingredientes = new List<RecetaIngrediente>
            {
                new() { Id = 1, RecetaId = 1, IngredienteId = ingrediente.Id, Ingrediente = ingrediente, Cantidad = cantidadEnReceta, UnidadId = unidadReceta.Id, Unidad = unidadReceta }
            },
            Servicios = new List<RecetaServicio>()
        };

        var variante = new ProductoVariante
        {
            Id = 1,
            Nombre = "Variante de prueba",
            RendimientoCantidad = 1m,
            RendimientoUnidadId = unidadPorcion.Id,
            RendimientoUnidad = unidadPorcion,
            IngredienteOverrides = new List<VarianteIngredienteOverride>(),
            Materiales = new List<VarianteMaterial>(),
            Servicios = new List<VarianteServicio>()
        };

        var precios = new Dictionary<int, decimal> { [ingrediente.Id] = precioPorUnidadCompra };

        var resultado = _sut.CalcularCosto(receta, variante, precios, new Dictionary<int, decimal>(), tarifaManoDeObraPorHora: 0m);

        return resultado.CostoTotal;
    }
}
