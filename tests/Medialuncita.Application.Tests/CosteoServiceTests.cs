using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;
using Xunit;

namespace Medialuncita.Application.Tests;

public class CosteoServiceTests
{
    private readonly CosteoService _sut = new();

    [Fact]
    public void CalcularCosto_Variante12Porciones_EscalaProporcionalmenteYSumaTodosLosComponentes()
    {
        var receta = TestDataBuilder.RecetaRogel();
        var variante = TestDataBuilder.Variante12Porciones();
        var precios = TestDataBuilder.PreciosIngredientesDefault();
        var preciosMateriales = TestDataBuilder.PreciosMaterialesDefault();

        var resultado = _sut.CalcularCosto(receta, variante, precios, preciosMateriales, tarifaManoDeObraPorHora: 1200m);

        // factorEscala = 12/20 = 0.6
        // Harina: 1kg*0.6 = 0.6kg, merma 2% -> 0.6/0.98 kg efectivos -> *1.5 $/g convertido a g
        // Dulce:  500g*0.6 = 300g, sin merma -> 300g * 3 $/g
        resultado.CostoIngredientes.Should().BeApproximately(1818.37m, 0.01m);
        resultado.CostoPackaging.Should().Be(0m); // esta variante no declara materiales propios

        // Mano de obra: tiempo base 90*0.6=54 + 15 (lote) + 0 (por unidad) = 69 min
        resultado.TiempoTotalMinutos.Should().Be(69);
        resultado.CostoManoDeObra.Should().Be(1380m); // 69/60 * 1200

        // Servicio Gas por hora: 69/60 h * $60/h
        resultado.CostoServicios.Should().Be(69m);

        resultado.CostoTotal.Should().BeApproximately(3267.37m, 0.01m);
        resultado.CostoUnitario.Should().BeApproximately(272.28m, 0.01m); // CostoTotal / 12
    }

    [Fact]
    public void CalcularCosto_VarianteIndividual_UsaOverrideDePackagingYTiempoPorUnidad()
    {
        var receta = TestDataBuilder.RecetaRogel();
        var variante = TestDataBuilder.VarianteIndividual();
        var precios = TestDataBuilder.PreciosIngredientesDefault();
        var preciosMateriales = TestDataBuilder.PreciosMaterialesDefault();

        var resultado = _sut.CalcularCosto(receta, variante, precios, preciosMateriales, tarifaManoDeObraPorHora: 1200m);

        // El override de dulce de leche (40g fijos) reemplaza el cálculo proporcional (500g*0.05=25g)
        var detalleDulce = resultado.Ingredientes.Single(i => i.NombreIngrediente == "Dulce de leche");
        detalleDulce.CantidadRequerida.Should().Be(40m);

        // El packaging nunca escala: siempre 1 caja, sin importar el rendimiento
        resultado.CostoPackaging.Should().Be(200m);

        // Tiempo: base 90*0.05=4.5 + 0 (lote) + 3*1 (por unidad) = 7.5 -> redondeo a 8
        resultado.TiempoTotalMinutos.Should().Be(8);

        resultado.CostoTotal.Should().BeApproximately(564.53m, 0.01m);
        resultado.CostoUnitario.Should().BeApproximately(564.53m, 0.01m); // rinde 1 -> costo unitario = costo total
    }

    [Fact]
    public void CalcularCosto_AplicaMermaMatematicamenteCorrecta()
    {
        // Receta aislada: 1 ingrediente, rendimiento 1, sin mano de obra ni servicios,
        // para verificar la fórmula de merma sin ruido de otros componentes.
        var unidadKg = TestDataBuilder.Kilogramo;
        var unidadPorcion = TestDataBuilder.Porcion;
        var ingrediente = new Ingrediente { Id = 10, Nombre = "Chocolate", UnidadCompraId = unidadKg.Id, UnidadCompra = unidadKg, MermaDefault = 0.20m };

        var receta = new Receta
        {
            Id = 10,
            Nombre = "Receta aislada",
            RendimientoBaseCantidad = 1m,
            RendimientoBaseUnidadId = unidadPorcion.Id,
            RendimientoBaseUnidad = unidadPorcion,
            TiempoPreparacionBaseMinutos = 0,
            Ingredientes = new List<RecetaIngrediente>
            {
                new() { Id = 10, RecetaId = 10, IngredienteId = ingrediente.Id, Ingrediente = ingrediente, Cantidad = 1, UnidadId = unidadKg.Id, Unidad = unidadKg }
            },
            Servicios = new List<RecetaServicio>()
        };

        var variante = new ProductoVariante
        {
            Id = 10,
            Nombre = "Variante aislada",
            RendimientoCantidad = 1m,
            RendimientoUnidadId = unidadPorcion.Id,
            RendimientoUnidad = unidadPorcion,
            IngredienteOverrides = new List<VarianteIngredienteOverride>(),
            Materiales = new List<VarianteMaterial>(),
            Servicios = new List<VarianteServicio>()
        };

        var precios = new Dictionary<int, PrecioVigente> { [ingrediente.Id] = new PrecioVigente(100m, unidadKg) }; // $100/kg = $0.1/g

        var resultado = _sut.CalcularCosto(receta, variante, precios, new Dictionary<int, PrecioVigente>(), tarifaManoDeObraPorHora: 0m);

        // CantidadEfectiva = 1kg / (1 - 0.20) = 1.25 kg = 1250 g
        // Subtotal = 1250 * 0.1 = 125
        resultado.CostoIngredientes.Should().Be(125m);
        resultado.Ingredientes.Single().CantidadEfectivaEnUnidadBase.Should().Be(1250m);
    }

    [Fact]
    public void CalcularCosto_SinPrecioVigente_LanzaExcepcionClara()
    {
        var receta = TestDataBuilder.RecetaRogel();
        var variante = TestDataBuilder.Variante12Porciones();
        var preciosIncompletos = new Dictionary<int, PrecioVigente>(); // vacío a propósito

        var act = () => _sut.CalcularCosto(receta, variante, preciosIncompletos, new Dictionary<int, PrecioVigente>(), 0m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*precio vigente*");
    }

    [Fact]
    public void CalcularCosto_RendimientosDeTipoDistinto_Lanza()
    {
        var receta = TestDataBuilder.RecetaRogel(); // rendimiento en Porción (Unidad)
        var variante = TestDataBuilder.Variante12Porciones();
        variante.RendimientoUnidad = TestDataBuilder.Kilogramo; // Peso: incompatible

        var act = () => _sut.CalcularCosto(receta, variante, TestDataBuilder.PreciosIngredientesDefault(), new Dictionary<int, PrecioVigente>(), 0m);

        act.Should().Throw<InvalidOperationException>();
    }
}
