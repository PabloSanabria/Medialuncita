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
}
