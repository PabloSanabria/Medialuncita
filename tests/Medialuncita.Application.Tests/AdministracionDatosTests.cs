using FluentAssertions;
using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;
using Medialuncita.Infrastructure.Data;
using Medialuncita.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Medialuncita.Application.Tests;

/// <summary>
/// Tests de integración real (SQLite en memoria, sin mocks) para las operaciones de
/// administración de datos: editar, eliminar de forma segura (bloqueando o
/// desactivando según corresponda), y administrar el historial de precios.
/// </summary>
public class AdministracionDatosTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MedialuncitaDbContext _db;
    private readonly UnidadMedidaRepository _unidades;
    private readonly IngredienteRepository _ingredientes;
    private readonly MaterialRepository _materiales;
    private readonly RecetaRepository _recetas;
    private readonly ProductoRepository _productos;
    private readonly EfUnitOfWork _uow;

    public AdministracionDatosTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MedialuncitaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new MedialuncitaDbContext(options);
        _db.Database.EnsureCreated();

        _unidades = new UnidadMedidaRepository(_db);
        _ingredientes = new IngredienteRepository(_db);
        _materiales = new MaterialRepository(_db);
        _recetas = new RecetaRepository(_db);
        _productos = new ProductoRepository(_db);
        _uow = new EfUnitOfWork(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ---------------- Unidades ----------------

    [Fact]
    public async Task ModificarUnidad_PersisteLosNuevosValores()
    {
        var unidad = new UnidadMedida { Nombre = "Kilo", Abreviatura = "k", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(unidad);
        await _db.SaveChangesAsync();

        var cargada = await _unidades.GetByIdAsync(unidad.Id);
        cargada!.Nombre = "Kilogramo";
        cargada.Abreviatura = "kg";
        await _uow.SaveChangesAsync();

        var releida = await _unidades.GetByIdAsync(unidad.Id);
        releida!.Nombre.Should().Be("Kilogramo");
        releida.Abreviatura.Should().Be("kg");
    }

    [Fact]
    public async Task EliminarUnidad_SinDependencias_SeElimina()
    {
        var unidad = new UnidadMedida { Nombre = "Docena", Abreviatura = "docena", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 12 };
        _db.UnidadesMedida.Add(unidad);
        await _db.SaveChangesAsync();

        (await _unidades.EstaEnUsoAsync(unidad.Id)).Should().BeFalse();

        await _unidades.DeleteAsync(unidad);
        await _uow.SaveChangesAsync();

        (await _unidades.GetByIdAsync(unidad.Id)).Should().BeNull();
    }

    [Fact]
    public async Task EstaEnUsoUnidad_ConIngredienteQueLaUsa_DevuelveTrue()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(kg);
        await _db.SaveChangesAsync();

        _db.Ingredientes.Add(new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id });
        await _db.SaveChangesAsync();

        (await _unidades.EstaEnUsoAsync(kg.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task EstaEnUsoUnidad_SinUsos_DevuelveFalse()
    {
        var unidad = new UnidadMedida { Nombre = "Litro", Abreviatura = "l", Tipo = TipoUnidad.Volumen, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(unidad);
        await _db.SaveChangesAsync();

        (await _unidades.EstaEnUsoAsync(unidad.Id)).Should().BeFalse();
    }

    // ---------------- Ingredientes ----------------

    [Fact]
    public async Task ModificarIngrediente_PersisteLosNuevosValores()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var lt = new UnidadMedida { Nombre = "Litro", Abreviatura = "l", Tipo = TipoUnidad.Volumen, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.AddRange(kg, lt);
        await _db.SaveChangesAsync();

        var ingrediente = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id, MermaDefault = 0.02m };
        _db.Ingredientes.Add(ingrediente);
        await _db.SaveChangesAsync();

        var cargado = await _ingredientes.GetByIdAsync(ingrediente.Id);
        cargado!.Nombre = "Harina 0000";
        cargado.UnidadCompraId = lt.Id; // cambio de unidad de compra, permitido
        cargado.MermaDefault = 0.05m;
        cargado.DensidadGramosPorMililitro = 0.53m;
        await _uow.SaveChangesAsync();

        var releido = await _ingredientes.GetByIdAsync(ingrediente.Id);
        releido!.Nombre.Should().Be("Harina 0000");
        releido.UnidadCompraId.Should().Be(lt.Id);
        releido.MermaDefault.Should().Be(0.05m);
        releido.DensidadGramosPorMililitro.Should().Be(0.53m);
    }

    [Fact]
    public async Task EliminarIngrediente_SinDependencias_SeEliminaFisicamenteYCascadeaHistorial()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(kg);
        await _db.SaveChangesAsync();

        var ingrediente = new Ingrediente { Nombre = "Azúcar", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(ingrediente);
        await _db.SaveChangesAsync();

        _db.HistorialPreciosIngredientes.Add(new HistorialPrecioIngrediente { IngredienteId = ingrediente.Id, Precio = 1000, Fecha = DateTime.Today });
        await _db.SaveChangesAsync();

        (await _ingredientes.ContarRecetasQueLoUsanAsync(ingrediente.Id)).Should().Be(0);

        await _ingredientes.DeleteAsync(ingrediente);
        await _uow.SaveChangesAsync();

        (await _ingredientes.GetByIdAsync(ingrediente.Id)).Should().BeNull();
        (await _db.HistorialPreciosIngredientes.AnyAsync(h => h.IngredienteId == ingrediente.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ContarRecetasQueUsanIngrediente_CuentaRecetasDistintas()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.AddRange(kg, porcion);
        await _db.SaveChangesAsync();

        var harina = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(harina);
        await _db.SaveChangesAsync();

        var receta1 = new Receta { Nombre = "Receta 1", RendimientoBaseCantidad = 1, RendimientoBaseUnidadId = porcion.Id };
        receta1.Ingredientes.Add(new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 1, UnidadId = kg.Id });
        var receta2 = new Receta { Nombre = "Receta 2", RendimientoBaseCantidad = 1, RendimientoBaseUnidadId = porcion.Id };
        receta2.Ingredientes.Add(new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 2, UnidadId = kg.Id });
        _db.Recetas.AddRange(receta1, receta2);
        await _db.SaveChangesAsync();

        (await _ingredientes.ContarRecetasQueLoUsanAsync(harina.Id)).Should().Be(2);
    }

    [Fact]
    public async Task Ingrediente_MarcadoInactivo_QuedaExcluidoDeGetAllActivos()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(kg);
        await _db.SaveChangesAsync();

        var ingrediente = new Ingrediente { Nombre = "Manteca", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(ingrediente);
        await _db.SaveChangesAsync();

        (await _ingredientes.GetAllActivosAsync()).Should().Contain(i => i.Id == ingrediente.Id);

        ingrediente.Activo = false;
        await _uow.SaveChangesAsync();

        (await _ingredientes.GetAllActivosAsync()).Should().NotContain(i => i.Id == ingrediente.Id);
    }

    // ---------------- Historial de precios ----------------

    [Fact]
    public async Task AgregarYEliminarPrecioHistorico_Ingrediente()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(kg);
        await _db.SaveChangesAsync();

        var ingrediente = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(ingrediente);
        await _db.SaveChangesAsync();

        var id1 = await _ingredientes.AgregarPrecioAsync(new HistorialPrecioIngrediente { IngredienteId = ingrediente.Id, Precio = 2100, Fecha = new DateTime(2026, 8, 1) });
        var id2 = await _ingredientes.AgregarPrecioAsync(new HistorialPrecioIngrediente { IngredienteId = ingrediente.Id, Precio = 2300, Fecha = new DateTime(2026, 8, 15) });
        await _uow.SaveChangesAsync();

        (await _ingredientes.GetHistorialAsync(ingrediente.Id)).Should().HaveCount(2);

        await _ingredientes.EliminarPrecioAsync(id1);
        await _uow.SaveChangesAsync();

        var historial = await _ingredientes.GetHistorialAsync(ingrediente.Id);
        historial.Should().HaveCount(1);
        historial.Single().Id.Should().Be(id2);
    }

    [Fact]
    public async Task EliminarPrecioMasReciente_PrecioVigenteVuelveAlAnterior()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        _db.UnidadesMedida.Add(kg);
        await _db.SaveChangesAsync();

        var ingrediente = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(ingrediente);
        await _db.SaveChangesAsync();

        await _ingredientes.AgregarPrecioAsync(new HistorialPrecioIngrediente { IngredienteId = ingrediente.Id, Precio = 2100, Fecha = new DateTime(2026, 8, 1) });
        var idMasReciente = await _ingredientes.AgregarPrecioAsync(new HistorialPrecioIngrediente { IngredienteId = ingrediente.Id, Precio = 2500, Fecha = new DateTime(2026, 8, 29) });
        await _uow.SaveChangesAsync();

        (await _ingredientes.GetPrecioVigenteAsync(ingrediente.Id))!.Precio.Should().Be(2500);

        await _ingredientes.EliminarPrecioAsync(idMasReciente);
        await _uow.SaveChangesAsync();

        (await _ingredientes.GetPrecioVigenteAsync(ingrediente.Id))!.Precio.Should().Be(2100);
    }

    // ---------------- Materiales (un caso espejo, misma lógica que Ingredientes) ----------------

    [Fact]
    public async Task EliminarMaterial_SinDependencias_SeEliminaFisicamenteYCascadeaHistorial()
    {
        var unidad = new UnidadMedida { Nombre = "Unidad", Abreviatura = "u", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.Add(unidad);
        await _db.SaveChangesAsync();

        var material = new Material { Nombre = "Caja", UnidadCompraId = unidad.Id };
        _db.Materiales.Add(material);
        await _db.SaveChangesAsync();

        _db.HistorialPreciosMateriales.Add(new HistorialPrecioMaterial { MaterialId = material.Id, Precio = 200, Fecha = DateTime.Today });
        await _db.SaveChangesAsync();

        (await _materiales.ContarVariantesQueLoUsanAsync(material.Id)).Should().Be(0);

        await _materiales.DeleteAsync(material);
        await _uow.SaveChangesAsync();

        (await _materiales.GetByIdAsync(material.Id)).Should().BeNull();
        (await _db.HistorialPreciosMateriales.AnyAsync(h => h.MaterialId == material.Id)).Should().BeFalse();
    }

    // ---------------- Recetas ----------------

    [Fact]
    public async Task ModificarReceta_PersisteCambiosDeEncabezadoYLineas()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var g = new UnidadMedida { Nombre = "Gramo", Abreviatura = "g", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1 };
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.AddRange(kg, g, porcion);
        await _db.SaveChangesAsync();

        var harina = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id };
        var azucar = new Ingrediente { Nombre = "Azúcar", UnidadCompraId = kg.Id };
        _db.Ingredientes.AddRange(harina, azucar);
        await _db.SaveChangesAsync();

        var receta = new Receta { Nombre = "Bizcochuelo", RendimientoBaseCantidad = 10, RendimientoBaseUnidadId = porcion.Id, TiempoPreparacionBaseMinutos = 30 };
        var lineaHarina = new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 300, UnidadId = g.Id };
        receta.Ingredientes.Add(lineaHarina);
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        // Simula la edición: cambia el encabezado, modifica una línea existente, quita
        // ninguna y agrega una nueva.
        var cargada = await _recetas.GetByIdConIngredientesAsync(receta.Id);
        cargada!.Nombre = "Bizcochuelo básico";
        cargada.TiempoPreparacionBaseMinutos = 35;
        cargada.Ingredientes.Single().Cantidad = 350; // modifica cantidad de la línea existente
        cargada.Ingredientes.Add(new RecetaIngrediente { IngredienteId = azucar.Id, Cantidad = 150, UnidadId = g.Id }); // agrega línea nueva
        await _uow.SaveChangesAsync();

        var releida = await _recetas.GetByIdConIngredientesAsync(receta.Id);
        releida!.Nombre.Should().Be("Bizcochuelo básico");
        releida.TiempoPreparacionBaseMinutos.Should().Be(35);
        releida.Ingredientes.Should().HaveCount(2);
        releida.Ingredientes.Single(i => i.IngredienteId == harina.Id).Cantidad.Should().Be(350);
        releida.Ingredientes.Should().Contain(i => i.IngredienteId == azucar.Id && i.Cantidad == 150);
    }

    [Fact]
    public async Task EliminarReceta_SinProductosAsociados_SeEliminaYCascadeaIngredientes()
    {
        var kg = new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg", Tipo = TipoUnidad.Peso, FactorAUnidadBase = 1000 };
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.AddRange(kg, porcion);
        await _db.SaveChangesAsync();

        var harina = new Ingrediente { Nombre = "Harina", UnidadCompraId = kg.Id };
        _db.Ingredientes.Add(harina);
        await _db.SaveChangesAsync();

        var receta = new Receta { Nombre = "Receta descartable", RendimientoBaseCantidad = 1, RendimientoBaseUnidadId = porcion.Id };
        receta.Ingredientes.Add(new RecetaIngrediente { IngredienteId = harina.Id, Cantidad = 1, UnidadId = kg.Id });
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        (await _recetas.ContarProductosQueLaUsanAsync(receta.Id)).Should().Be(0);

        await _recetas.DeleteAsync(receta);
        await _uow.SaveChangesAsync();

        (await _db.Recetas.AnyAsync(r => r.Id == receta.Id)).Should().BeFalse();
        (await _db.RecetaIngredientes.AnyAsync(ri => ri.RecetaId == receta.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ContarProductosQueUsanReceta_SinProductos_DevuelveCero()
    {
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.Add(porcion);
        await _db.SaveChangesAsync();

        var receta = new Receta { Nombre = "Receta sin productos", RendimientoBaseCantidad = 1, RendimientoBaseUnidadId = porcion.Id };
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        (await _recetas.ContarProductosQueLaUsanAsync(receta.Id)).Should().Be(0);
    }

    // ---------------- Productos y variantes ----------------

    [Fact]
    public async Task ModificarProductoYVariante_PersisteLosCambios()
    {
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.Add(porcion);
        await _db.SaveChangesAsync();

        var receta = new Receta { Nombre = "Brownie", RendimientoBaseCantidad = 12, RendimientoBaseUnidadId = porcion.Id };
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();

        var producto = new Producto { Nombre = "Brownie clásico", RecetaId = receta.Id };
        producto.Variantes.Add(new ProductoVariante { Nombre = "Caja de 12", RendimientoCantidad = 12, RendimientoUnidadId = porcion.Id });
        await _productos.AddAsync(producto);
        await _uow.SaveChangesAsync();

        var cargado = await _productos.GetByIdAsync(producto.Id);
        cargado!.Nombre = "Brownie con nuez";
        cargado.Variantes.Single().TiempoAdicionalPorLoteMinutos = 15;
        await _uow.SaveChangesAsync();

        var releido = await _productos.GetByIdAsync(producto.Id);
        releido!.Nombre.Should().Be("Brownie con nuez");
        releido.Receta!.Nombre.Should().Be("Brownie");
        releido.Variantes.Should().ContainSingle();
        releido.Variantes.Single().TiempoAdicionalPorLoteMinutos.Should().Be(15);
        releido.Variantes.Single().RendimientoUnidad!.Tipo.Should().Be(TipoUnidad.Unidad);
    }

    [Fact]
    public async Task EliminarProducto_CascadeaSusVariantes()
    {
        var porcion = new UnidadMedida { Nombre = "Porción", Abreviatura = "porción", Tipo = TipoUnidad.Unidad, FactorAUnidadBase = 1 };
        _db.UnidadesMedida.Add(porcion);
        await _db.SaveChangesAsync();

        var receta = new Receta { Nombre = "Alfajor", RendimientoBaseCantidad = 6, RendimientoBaseUnidadId = porcion.Id };
        var producto = new Producto { Nombre = "Alfajor de maicena", Receta = receta };
        producto.Variantes.Add(new ProductoVariante { Nombre = "Media docena", RendimientoCantidad = 6, RendimientoUnidad = porcion });
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        await _productos.DeleteAsync(producto);
        await _uow.SaveChangesAsync();

        (await _productos.GetByIdAsync(producto.Id)).Should().BeNull();
        (await _db.ProductoVariantes.AnyAsync(v => v.ProductoId == producto.Id)).Should().BeFalse();
    }
}
