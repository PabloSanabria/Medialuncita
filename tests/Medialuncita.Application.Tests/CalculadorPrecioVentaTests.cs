using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Domain.Enums;
using Xunit;

namespace Medialuncita.Application.Tests;

public class CalculadorPrecioVentaTests
{
    private readonly CosteoService _sut = new();

    [Fact]
    public void CalcularPrecioVenta_PorMargen_AplicaPorcentajeSobreCosto()
    {
        var resultado = _sut.CalcularPrecioVenta(100m, EstrategiaPrecio.Margen, margenPorcentual: 0.30m, null, null, EstrategiaRedondeo.SinRedondeo);
        resultado.PrecioUnitarioFinal.Should().Be(130m);
    }

    [Fact]
    public void CalcularPrecioVenta_PorMultiplicador_MultiplicaCosto()
    {
        var resultado = _sut.CalcularPrecioVenta(100m, EstrategiaPrecio.Multiplicador, null, multiplicador: 2.5m, null, EstrategiaRedondeo.SinRedondeo);
        resultado.PrecioUnitarioFinal.Should().Be(250m);
    }

    [Fact]
    public void CalcularPrecioVenta_Manual_IgnoraElCostoYUsaElPrecioFijado()
    {
        var resultado = _sut.CalcularPrecioVenta(100m, EstrategiaPrecio.Manual, null, null, precioManual: 999m, EstrategiaRedondeo.SinRedondeo);
        resultado.PrecioUnitarioFinal.Should().Be(999m);
    }

    [Fact]
    public void CalcularPrecioVenta_RedondeoEntero_RedondeaHaciaArriba()
    {
        var resultado = _sut.CalcularPrecioVenta(100.2m, EstrategiaPrecio.Margen, margenPorcentual: 0m, null, null, EstrategiaRedondeo.RedondeoEntero);
        resultado.PrecioUnitarioFinal.Should().Be(101m);
    }

    [Fact]
    public void CalcularPrecioVenta_RedondeoMultiploDe50_RedondeaAlProximoMultiplo()
    {
        var resultado = _sut.CalcularPrecioVenta(101m, EstrategiaPrecio.Margen, margenPorcentual: 0m, null, null, EstrategiaRedondeo.RedondeoMultiploDe50);
        resultado.PrecioUnitarioFinal.Should().Be(150m);
    }

    [Fact]
    public void CalcularPrecioVenta_Margen_SinMargenProvisto_Lanza()
    {
        var act = () => _sut.CalcularPrecioVenta(100m, EstrategiaPrecio.Margen, margenPorcentual: null, null, null, EstrategiaRedondeo.SinRedondeo);
        act.Should().Throw<InvalidOperationException>();
    }
}
