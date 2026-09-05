using FluentAssertions;
using Medialuncita.Application.Costeo;
using Medialuncita.Application.Precios;
using Medialuncita.Application.Presupuestos;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;
using Medialuncita.Infrastructure.Data;
using Medialuncita.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Medialuncita.Application.Tests;

/// <summary>
/// Test de integración real (sin mocks) sobre SQLite en memoria: valida que
///   1) el modelo de EF Core (el mismo que generan las migraciones) crea el esquema sin errores,
///   2) PresupuestoService arma correctamente el snapshot completo,
///   3) el presupuesto ya generado NO cambia si después se actualiza el precio de un ingrediente
///      (requisito de "snapshot congelado" / consulta histórica).
/// </summary>
public class PresupuestoServiceIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MedialuncitaDbContext _db;

    public PresupuestoServiceIntegrationTests()
    {
        // Conexión SQLite en memoria: debe permanecer abierta durante todo el test,
        // porque ":memory:" se destruye al cerrar la última conexión.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MedialuncitaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new MedialuncitaDbContext(options);
        _db.Database.EnsureCreated(); // usa el mismo modelo que las migraciones reales
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GenerarPresupuesto_CreaSnapshotCompleto_YNoCambiaSiElPrecioSeActualizaDespues()
    {
        // ---- Arrange: catálogo mínimo ----
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var g = new UnidadMedida { Nombre = "Gramo", Abreviatura = "g", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1 };
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.AddRange(kg, g, porcion);
        await _db.SaveChangesAsync();

        var harina = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id, MermaDefault = 0m };
        _db.Ingredientes.Add(harina);
        await _db.SaveChangesAsync();

        _db.HistorialPreciosIngredientes.Add(new HistorialPrecioIngrediente
        {
            IngredienteId = harina.Id,
            Precio = 1000m, // $1000/kg = $1/g
            Fecha = new DateTime(2026, 1, 1)
        });
        await _db.SaveChangesAsync();

        var receta = new Receta
        {
            Nombre = "Bizcochuelo",
            RendimientoBaseCantidad = 10m,
            RendimientoBaseUnidadId = porcion.Id,
            TiempoPreparacionBaseMinutos = 30
        };
        receta.Ingredientes.Add(new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 1, UnidadId = kg.Id });
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        var producto = new Producto { Nombre = "Bizcochuelo simple", RecetaId = receta.Id };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        var variante = new ProductoVariante
        {
            ProductoId = producto.Id,
            Nombre = "10 porciones",
            RendimientoCantidad = 10m,
            RendimientoUnidadId = porcion.Id
        };
        _db.ProductoVariantes.Add(variante);
        await _db.SaveChangesAsync();

        // Se usa el repositorio (no una consulta cruda) porque ya contempla el caso de que
        // el seed de HasData no se haya aplicado (p. ej. al crear el esquema vía
        // EnsureCreated en lugar de una migración real): si no encuentra la fila, la crea.
        var configRepo = new ConfiguracionGlobalRepository(_db);
        var config = await configRepo.GetAsync();
        config.TarifaManoDeObraPorHora = 1000m;
        config.EstrategiaPrecioDefault = EstrategiaPrecio.Margen;
        config.MargenPorcentualDefault = 0.5m; // 50%
        await configRepo.SaveAsync(config);

        var service = ConstruirPresupuestoService();

        // ---- Act: generar presupuesto ----
        var presupuestoId = await service.GenerarPresupuestoAsync(
            new[] { new ItemPresupuestoRequest(variante.Id, Cantidad: 2) },
            clienteNombre: "Cliente de prueba",
            notas: null,
            fecha: new DateTime(2026, 6, 1));

        // ---- Assert: snapshot correcto ----
        var presupuestoGuardado = await _db.Presupuestos
            .Include(p => p.Items).ThenInclude(i => i.DetalleIngredientes)
            .FirstAsync(p => p.Id == presupuestoId);

        var item = presupuestoGuardado.Items.Single();
        item.NombreProductoSnapshot.Should().Be("Bizcochuelo simple");
        item.NombreVarianteSnapshot.Should().Be("10 porciones");
        item.DetalleIngredientes.Should().ContainSingle(d => d.NombreIngredienteSnapshot == "Harina");

        // Costo ingredientes: 1kg * $1/g convertido = 1000g * $1/g = $1000. Costo unitario = 100.
        // Mano de obra: 30 min -> 0.5h * $1000 = $500. CostoTotal=1500, CostoUnitario=150.
        // Precio con margen 50%: 150*1.5=225. Subtotal para cantidad=2: 450.
        item.CostoUnitarioSnapshot.Should().Be(150m);
        item.PrecioUnitarioAlMomento.Should().Be(225m);
        item.Subtotal.Should().Be(450m);
        presupuestoGuardado.Total.Should().Be(450m);

        // ---- Act: se actualiza el precio del ingrediente DESPUÉS de generar el presupuesto ----
        _db.HistorialPreciosIngredientes.Add(new HistorialPrecioIngrediente
        {
            IngredienteId = harina.Id,
            Precio = 5000m, // el precio se disparó
            Fecha = new DateTime(2026, 7, 1)
        });
        await _db.SaveChangesAsync();

        // ---- Assert: el presupuesto histórico NO cambia ----
        var presupuestoReleido = await _db.Presupuestos
            .Include(p => p.Items)
            .FirstAsync(p => p.Id == presupuestoId);

        presupuestoReleido.Items.Single().PrecioUnitarioAlMomento.Should().Be(225m);
        presupuestoReleido.Total.Should().Be(450m);
    }

    private PresupuestoService ConstruirPresupuestoService()
    {
        var productoRepo = new ProductoRepository(_db);
        var presupuestoRepo = new PresupuestoRepository(_db);
        var configRepo = new ConfiguracionGlobalRepository(_db);
        var ingredienteRepo = new IngredienteRepository(_db);
        var materialRepo = new MaterialRepository(_db);
        var uow = new EfUnitOfWork(_db);
        var precioService = new PrecioConsultaService(ingredienteRepo, materialRepo, uow);
        var costeoService = new CosteoService();

        return new PresupuestoService(productoRepo, presupuestoRepo, configRepo, precioService, costeoService, uow);
    }
}
