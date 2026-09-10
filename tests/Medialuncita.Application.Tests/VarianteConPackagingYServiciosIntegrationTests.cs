using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Application.Precios;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;
using Medialuncita.Infrastructure.Data;
using Medialuncita.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Medialuncita.Application.Tests;

/// <summary>
/// Test de integración real (sin mocks) que ejercita exactamente el mismo camino que
/// usa la pantalla "Detalle de variante": IProductoRepository.GetVarianteParaCosteoAsync
/// trayendo packaging (VarianteMaterial), servicios de la receta madre (RecetaServicio) Y
/// servicios propios de la variante (VarianteServicio) al mismo tiempo, más un override de
/// estrategia de precio a nivel variante. Antes de este entregable, este camino solo se
/// ejercía parcialmente desde PresupuestoServiceIntegrationTests (sin packaging/servicios).
/// </summary>
public class VarianteConPackagingYServiciosIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MedialuncitaDbContext _db;

    public VarianteConPackagingYServiciosIntegrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MedialuncitaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new MedialuncitaDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetVarianteParaCosteoAsync_TraeElGrafoCompleto_YCosteoServiceLoCalculaCorrectamente()
    {
        // ---- Arrange: catálogo mínimo ----
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var g = new UnidadMedida { Nombre = "Gramo", Abreviatura = "g", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1 };
        var unidad = new UnidadMedida { Nombre = "Unidad", Abreviatura = "u", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.AddRange(kg, g, unidad);
        await _db.SaveChangesAsync();

        var harina = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id, MermaDefault = 0m };
        _db.Ingredientes.Add(harina);
        await _db.SaveChangesAsync();
        _db.HistorialPreciosIngredientes.Add(new HistorialPrecioIngrediente { IngredienteId = harina.Id, Precio = 1000m, Fecha = new DateTime(2026, 1, 1) }); // $1/g
        await _db.SaveChangesAsync();

        var caja = new Material { Nombre = "Caja individual", UnidadCompraId = unidad.Id, MermaDefault = 0m };
        _db.Materiales.Add(caja);
        await _db.SaveChangesAsync();
        _db.HistorialPreciosMateriales.Add(new HistorialPrecioMaterial { MaterialId = caja.Id, Precio = 50m, Fecha = new DateTime(2026, 1, 1) }); // $50/u
        await _db.SaveChangesAsync();

        var gas = new Servicio { Nombre = "Gas del horno", CostoPorHora = 120m };
        var decoracion = new Servicio { Nombre = "Alquiler de soplete", CostoPorLote = 30m };
        _db.Servicios.AddRange(gas, decoracion);
        await _db.SaveChangesAsync();

        var receta = new Receta
        {
            Nombre = "Masa base",
            RendimientoBaseCantidad = 10m,
            RendimientoBaseUnidadId = unidad.Id,
            TiempoPreparacionBaseMinutos = 60 // 1 hora, escalado por factorEscala
        };
        receta.Ingredientes.Add(new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 1, UnidadId = kg.Id }); // 1kg = 1000g cada 10 unidades
        receta.Servicios.Add(new RecetaServicio { ServicioId = gas.Id, ModoProrrateo = ModoProrrateo.PorHora });
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        var producto = new Producto { Nombre = "Medialuna rellena", RecetaId = receta.Id };
        var variante = new ProductoVariante
        {
            ProductoId = producto.Id,
            Nombre = "Individual",
            RendimientoCantidad = 5m, // factorEscala = 5/10 = 0.5
            RendimientoUnidadId = unidad.Id,
            EstrategiaPrecioOverride = EstrategiaPrecio.Multiplicador,
            MultiplicadorOverride = 3m,
            EstrategiaRedondeoOverride = EstrategiaRedondeo.RedondeoEntero
        };
        variante.Materiales.Add(new VarianteMaterial { MaterialId = caja.Id, Cantidad = 5 }); // 5 cajas, packaging no escala
        variante.Servicios.Add(new VarianteServicio { ServicioId = decoracion.Id, ModoProrrateo = ModoProrrateo.PorLote });
        producto.Variantes.Add(variante);
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        var productoRepo = new ProductoRepository(_db);
        var ingredienteRepo = new IngredienteRepository(_db);
        var materialRepo = new MaterialRepository(_db);
        var uow = new EfUnitOfWork(_db);
        var precioService = new PrecioConsultaService(ingredienteRepo, materialRepo, uow);
        var costeoService = new CosteoService();

        // ---- Act: exactamente el camino que usa VarianteDetalle.razor ----
        var varianteCargada = await productoRepo.GetVarianteParaCosteoAsync(variante.Id);
        varianteCargada.Should().NotBeNull();

        var recetaCargada = varianteCargada!.Producto!.Receta!;

        var preciosIngredientes = new Dictionary<int, decimal>
        {
            [harina.Id] = await precioService.GetPrecioVigenteIngredienteAsync(harina.Id)
        };
        var preciosMateriales = new Dictionary<int, decimal>
        {
            [caja.Id] = await precioService.GetPrecioVigenteMaterialAsync(caja.Id)
        };

        var resultadoCosteo = costeoService.CalcularCosto(
            recetaCargada, varianteCargada, preciosIngredientes, preciosMateriales, tarifaManoDeObraPorHora: 600m);

        // ---- Assert: cada componente del costeo ----
        // Ingredientes: 1kg * 0.5 (factorEscala) = 500g, sin merma, a $1/g = $500.
        resultadoCosteo.CostoIngredientes.Should().Be(500m);

        // Packaging: 5 cajas a $50/u = $250 (no escala con factorEscala, se declara directo).
        resultadoCosteo.CostoPackaging.Should().Be(250m);

        // Tiempo: 60min * 0.5 = 30min = 0.5h. Mano de obra: 0.5h * $600 = $300.
        resultadoCosteo.TiempoTotalMinutos.Should().Be(30);
        resultadoCosteo.CostoManoDeObra.Should().Be(300m);

        // Servicios: gas por hora = 0.5h * $120 = $60 (receta) + soplete por lote = $30 (variante) = $90.
        resultadoCosteo.CostoServicios.Should().Be(90m);
        resultadoCosteo.Servicios.Should().HaveCount(2);
        resultadoCosteo.Servicios.Should().Contain(s => s.NombreServicio == "Gas del horno" && s.Subtotal == 60m);
        resultadoCosteo.Servicios.Should().Contain(s => s.NombreServicio == "Alquiler de soplete" && s.Subtotal == 30m);

        // Total: 500 + 250 + 300 + 90 = 1140. Unitario: 1140 / 5 = 228.
        resultadoCosteo.CostoTotal.Should().Be(1140m);
        resultadoCosteo.CostoUnitario.Should().Be(228m);

        // ---- Precio de venta: usa el override de la variante (Multiplicador x3, redondeo entero) ----
        var resultadoPrecio = costeoService.CalcularPrecioVenta(
            resultadoCosteo.CostoUnitario,
            varianteCargada.EstrategiaPrecioOverride!.Value,
            varianteCargada.MargenPorcentualOverride,
            varianteCargada.MultiplicadorOverride,
            varianteCargada.PrecioManualOverride,
            varianteCargada.EstrategiaRedondeoOverride!.Value);

        resultadoPrecio.PrecioUnitarioSinRedondeo.Should().Be(684m); // 228 * 3
        resultadoPrecio.PrecioUnitarioFinal.Should().Be(684m); // ya es entero, RedondeoEntero no cambia nada
    }
}
