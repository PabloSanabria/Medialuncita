using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Domain.Enums;
using Xunit;

namespace Medialuncita.Application.Tests;

public class ConversionUnidadesTests
{
    [Fact]
    public void ConvertirAUnidadBase_MismoTipo_AplicaFactorDirecto()
    {
        var resultado = ConversionUnidades.ConvertirAUnidadBase(2m, TestDataBuilder.Kilogramo, TipoUnidad.Peso);
        resultado.Should().Be(2000m); // 2 kg -> 2000 g
    }

    [Fact]
    public void ConvertirAUnidadBase_Volumen_AplicaFactorDeLaTaza()
    {
        var resultado = ConversionUnidades.ConvertirAUnidadBase(1m, TestDataBuilder.Taza, TipoUnidad.Volumen);
        resultado.Should().Be(250m); // 1 taza -> 250 ml
    }

    [Fact]
    public void ConvertirAUnidadBase_PesoAVolumen_UsaDensidad()
    {
        // 100 g de un ingrediente con densidad 0.53 g/ml -> ~188.68 ml
        var resultado = ConversionUnidades.ConvertirAUnidadBase(100m, TestDataBuilder.Gramo, TipoUnidad.Volumen, densidadGramosPorMililitro: 0.53m);
        resultado.Should().BeApproximately(188.68m, 0.01m);
    }

    [Fact]
    public void ConvertirAUnidadBase_VolumenAPeso_UsaDensidad()
    {
        // 1 taza (250 ml) con densidad 0.53 g/ml -> 132.5 g
        var resultado = ConversionUnidades.ConvertirAUnidadBase(1m, TestDataBuilder.Taza, TipoUnidad.Peso, densidadGramosPorMililitro: 0.53m);
        resultado.Should().Be(132.5m);
    }

    [Fact]
    public void ConvertirAUnidadBase_PesoAVolumen_SinDensidad_Lanza()
    {
        var act = () => ConversionUnidades.ConvertirAUnidadBase(100m, TestDataBuilder.Gramo, TipoUnidad.Volumen);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConvertirAUnidadBase_ContraUnidadContable_Lanza()
    {
        var act = () => ConversionUnidades.ConvertirAUnidadBase(1m, TestDataBuilder.Kilogramo, TipoUnidad.Unidad);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PrecioPorUnidadBase_CalculaPrecioPorGramo()
    {
        var resultado = ConversionUnidades.PrecioPorUnidadBase(1500m, TestDataBuilder.Kilogramo);
        resultado.Should().Be(1.5m); // $1500/kg -> $1.5/g
    }

    // ---- Tests de EsConversionValida (usados por la pantalla de Recetas para validar
    // la unidad elegida contra la unidad de compra del ingrediente, antes de guardar) ----

    [Fact]
    public void EsConversionValida_MismoTipo_EsValida()
    {
        // Harina comprada en kg, receta la usa en gramos -> mismo tipo (Peso), siempre válido.
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Gramo, TipoUnidad.Peso);
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsConversionValida_Docena_ContraUnidad_EsValida()
    {
        // Huevos comprados por docena, receta los usa en unidades -> mismo tipo (Unidad).
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Unidad, TipoUnidad.Unidad);
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsConversionValida_Litro_ContraMililitro_EsValida()
    {
        // Leche comprada por litro, receta la usa en mililitros -> mismo tipo (Volumen).
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Mililitro, TipoUnidad.Volumen);
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsConversionValida_PesoAVolumen_ConDensidad_EsValida()
    {
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Gramo, TipoUnidad.Volumen, densidadGramosPorMililitro: 0.53m);
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsConversionValida_PesoAVolumen_SinDensidad_NoEsValida()
    {
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Gramo, TipoUnidad.Volumen);
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EsConversionValida_ContraUnidadContable_NuncaEsValida_AunqueHayaDensidad()
    {
        // Un ingrediente comprado por "Unidad" (ej: docena) no admite cruzarse con Peso/Volumen,
        // ni siquiera si el ingrediente tuviera densidad cargada.
        var resultado = ConversionUnidades.EsConversionValida(TestDataBuilder.Kilogramo, TipoUnidad.Unidad, densidadGramosPorMililitro: 1m);
        resultado.Should().BeFalse();
    }
}
