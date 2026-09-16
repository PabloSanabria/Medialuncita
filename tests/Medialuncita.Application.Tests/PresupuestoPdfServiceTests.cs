using System.Text;
using FluentAssertions;
using Medialuncita.Application.Presupuestos.Pdf;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;
using Xunit;

namespace Medialuncita.Application.Tests;

/// <summary>
/// PresupuestoPdfService no tiene dependencias (ni repos, ni CosteoService, ni precios):
/// se construye directo, sin mocks, y se lo alimenta con un Presupuesto armado a mano
/// (como si viniera de PresupuestoRepository.GetByIdAsync). Esto por sí solo prueba que
/// el servicio no puede estar recalculando nada: no tiene forma de acceder a otra fuente
/// de datos que no sea el objeto que se le pasa.
/// </summary>
public class PresupuestoPdfServiceTests
{
    private readonly PresupuestoPdfService _sut = new();

    private static Presupuesto CrearPresupuesto(int cantidadItems = 2, string? clienteNombre = "Juan Pérez", string? notas = null)
    {
        var presupuesto = new Presupuesto
        {
            Id = 42,
            Fecha = new DateTime(2026, 9, 10),
            ClienteNombre = clienteNombre,
            Notas = notas
        };

        for (var i = 0; i < cantidadItems; i++)
        {
            var precioUnitario = 100m + i;
            var cantidad = 3m;
            presupuesto.Items.Add(new PresupuestoItem
            {
                NombreProductoSnapshot = $"Medialuna {i}",
                NombreVarianteSnapshot = $"Docena {i}",
                Cantidad = cantidad,
                PrecioUnitarioAlMomento = precioUnitario,
                Subtotal = precioUnitario * cantidad,
                CostoTotalSnapshot = 50m,
                CostoUnitarioSnapshot = 16.66m,
                EstrategiaPrecioSnapshot = EstrategiaPrecio.Margen,
                EstrategiaRedondeoSnapshot = EstrategiaRedondeo.SinRedondeo
            });
        }

        presupuesto.Total = presupuesto.Items.Sum(it => it.Subtotal);
        return presupuesto;
    }

    [Fact]
    public void GenerarPdf_ProduceUnDocumentoConEncabezadoYCierrePdfValidos()
    {
        var bytes = _sut.GenerarPdf(CrearPresupuesto());

        var texto = Encoding.Latin1.GetString(bytes);
        texto.Should().StartWith("%PDF-1.4");
        texto.TrimEnd().Should().EndWith("%%EOF");
        texto.Should().Contain("/Type /Catalog");
        texto.Should().Contain("/Type /Page");
    }

    [Fact]
    public void GenerarPdf_NoRecalculaNada_UsaSoloLosValoresDelSnapshot()
    {
        // Los valores de costeo (CostoTotalSnapshot, etc.) son intencionalmente "raros"
        // e inconsistentes con Precio/Subtotal para dejar en evidencia que, si el servicio
        // recalculara algo, el número que aparecería en el PDF no sería el que pusimos acá.
        var presupuesto = CrearPresupuesto(cantidadItems: 1);
        presupuesto.Items.First().PrecioUnitarioAlMomento = 123.45m;
        presupuesto.Items.First().Subtotal = 370.35m;
        presupuesto.Total = 370.35m;

        var texto = Encoding.Latin1.GetString(_sut.GenerarPdf(presupuesto));

        texto.Should().Contain("123.45");
        texto.Should().Contain("370.35");
        texto.Should().Contain("Presupuesto");
    }

    [Fact]
    public void GenerarPdf_SinClienteNiNotas_NoFalla()
    {
        var presupuesto = CrearPresupuesto(clienteNombre: null, notas: null);

        var accion = () => _sut.GenerarPdf(presupuesto);

        accion.Should().NotThrow();
    }

    [Fact]
    public void GenerarPdf_ConMuchosItems_GeneraMasDeUnaPagina()
    {
        var presupuesto = CrearPresupuesto(cantidadItems: 60);

        var texto = Encoding.Latin1.GetString(_sut.GenerarPdf(presupuesto));

        // Cada objeto de página tiene "/Type /Page" (distinto de "/Type /Pages", el nodo raíz).
        var cantidadPaginas = texto.Split("/Type /Page ").Length - 1;
        cantidadPaginas.Should().BeGreaterThan(1);
        texto.Should().Contain("continuaci\u00f3n"); // encabezado de páginas siguientes a la primera
    }

    [Fact]
    public void GenerarPdf_ConNombresConParentesisYBarras_NoRompeElDocumento()
    {
        var presupuesto = CrearPresupuesto(cantidadItems: 1);
        presupuesto.Items.First().NombreProductoSnapshot = "Medialuna (grande) \\ especial";

        var bytes = _sut.GenerarPdf(presupuesto);
        var texto = Encoding.Latin1.GetString(bytes);

        texto.Should().Contain("%%EOF");
        // Los paréntesis del nombre deben aparecer escapados con \ dentro del stream.
        texto.Should().Contain("\\(grande\\)");
    }
}
