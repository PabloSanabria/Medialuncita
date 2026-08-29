using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Abstractions;

// Repositorios simples, sin genéricos "mágicos" ni Specification pattern:
// cada uno expone lo que el caso de uso necesita. Se mantienen finos a propósito.

public interface IUnidadMedidaRepository
{
    Task<UnidadMedida?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<UnidadMedida>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UnidadMedida unidad, CancellationToken ct = default);
}

public interface IIngredienteRepository
{
    Task<Ingrediente?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Ingrediente?> GetByIdConHistorialAsync(int id, CancellationToken ct = default);
    Task<List<Ingrediente>> GetAllActivosAsync(CancellationToken ct = default);
    Task AddAsync(Ingrediente ingrediente, CancellationToken ct = default);
    Task<int> AgregarPrecioAsync(HistorialPrecioIngrediente historial, CancellationToken ct = default);
    Task<HistorialPrecioIngrediente?> GetPrecioVigenteAsync(int ingredienteId, DateTime? aFecha = null, CancellationToken ct = default);
    Task<List<HistorialPrecioIngrediente>> GetHistorialAsync(int ingredienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default);
}

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Material>> GetAllActivosAsync(CancellationToken ct = default);
    Task AddAsync(Material material, CancellationToken ct = default);
    Task<int> AgregarPrecioAsync(HistorialPrecioMaterial historial, CancellationToken ct = default);
    Task<HistorialPrecioMaterial?> GetPrecioVigenteAsync(int materialId, DateTime? aFecha = null, CancellationToken ct = default);
    Task<List<HistorialPrecioMaterial>> GetHistorialAsync(int materialId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default);
}

public interface IRecetaRepository
{
    Task<Receta?> GetByIdConIngredientesAsync(int id, CancellationToken ct = default);
    Task<List<Receta>> GetAllActivasAsync(CancellationToken ct = default);
    Task AddAsync(Receta receta, CancellationToken ct = default);
}

public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Producto>> GetAllActivosAsync(CancellationToken ct = default);
    Task AddAsync(Producto producto, CancellationToken ct = default);

    /// <summary>Trae la variante con todo lo necesario para costear: receta madre,
    /// ingredientes de la receta, overrides, materiales y servicios de la variante.</summary>
    Task<ProductoVariante?> GetVarianteParaCosteoAsync(int varianteId, CancellationToken ct = default);
}

public interface IServicioRepository
{
    Task<Servicio?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Servicio>> GetAllActivosAsync(CancellationToken ct = default);
    Task AddAsync(Servicio servicio, CancellationToken ct = default);
}

public interface IConfiguracionGlobalRepository
{
    Task<ConfiguracionGlobal> GetAsync(CancellationToken ct = default);
    Task SaveAsync(ConfiguracionGlobal configuracion, CancellationToken ct = default);
}

public interface IPresupuestoRepository
{
    /// <summary>Agrega el presupuesto al contexto. NO devuelve el Id: recién se asigna
    /// después de llamar a IUnitOfWork.SaveChangesAsync (autoincremental de SQLite).
    /// Leer presupuesto.Id después de guardar.</summary>
    Task AddAsync(Presupuesto presupuesto, CancellationToken ct = default);
    Task<Presupuesto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Presupuesto>> GetAllAsync(CancellationToken ct = default);
}

/// <summary>
/// Unidad de trabajo mínima para confirmar cambios. Se evita el patrón
/// Repository+UnitOfWork "completo" tipo Clean Architecture de manual;
/// esto es solo un SaveChangesAsync expuesto de forma abstracta.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
