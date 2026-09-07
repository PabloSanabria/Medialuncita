using Medialuncita.Application.Abstractions;
using Medialuncita.Domain.Entities;
using Medialuncita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Medialuncita.Infrastructure.Repositories;

public class UnidadMedidaRepository(MedialuncitaDbContext db) : IUnidadMedidaRepository
{
    public Task<UnidadMedida?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<UnidadMedida>> GetAllAsync(CancellationToken ct = default) =>
        db.UnidadesMedida.OrderBy(u => u.Nombre).ToListAsync(ct);

    public async Task AddAsync(UnidadMedida unidad, CancellationToken ct = default) =>
        await db.UnidadesMedida.AddAsync(unidad, ct);

    public async Task<bool> EstaEnUsoAsync(int id, CancellationToken ct = default)
    {
        // Se revisan todas las FK reales del esquema que apuntan a UnidadMedida,
        // aunque hoy no todas tengan pantalla propia (ej. ProductoVariante) — la
        // relación ya existe en el modelo desde la Fase 0 y hay que respetarla.
        if (await db.Ingredientes.AnyAsync(i => i.UnidadCompraId == id, ct)) return true;
        if (await db.Materiales.AnyAsync(m => m.UnidadCompraId == id, ct)) return true;
        if (await db.Recetas.AnyAsync(r => r.RendimientoBaseUnidadId == id, ct)) return true;
        if (await db.RecetaIngredientes.AnyAsync(ri => ri.UnidadId == id, ct)) return true;
        if (await db.ProductoVariantes.AnyAsync(v => v.RendimientoUnidadId == id, ct)) return true;
        if (await db.VarianteIngredienteOverrides.AnyAsync(o => o.UnidadId == id, ct)) return true;
        return false;
    }

    public Task DeleteAsync(UnidadMedida unidad, CancellationToken ct = default)
    {
        db.UnidadesMedida.Remove(unidad);
        return Task.CompletedTask;
    }
}

public class IngredienteRepository(MedialuncitaDbContext db) : IIngredienteRepository
{
    public Task<Ingrediente?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Ingredientes.Include(i => i.UnidadCompra).FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<Ingrediente?> GetByIdConHistorialAsync(int id, CancellationToken ct = default) =>
        db.Ingredientes
            .Include(i => i.UnidadCompra)
            .Include(i => i.HistorialPrecios)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<List<Ingrediente>> GetAllActivosAsync(CancellationToken ct = default) =>
        db.Ingredientes.Include(i => i.UnidadCompra).Where(i => i.Activo).OrderBy(i => i.Nombre).ToListAsync(ct);

    public async Task AddAsync(Ingrediente ingrediente, CancellationToken ct = default) =>
        await db.Ingredientes.AddAsync(ingrediente, ct);

    public async Task<int> AgregarPrecioAsync(HistorialPrecioIngrediente historial, CancellationToken ct = default)
    {
        await db.HistorialPreciosIngredientes.AddAsync(historial, ct);
        await db.SaveChangesAsync(ct); // necesario para que el Id devuelto sea el real (autoincremental de SQLite)
        return historial.Id;
    }

    public Task<HistorialPrecioIngrediente?> GetPrecioVigenteAsync(int ingredienteId, DateTime? aFecha = null, CancellationToken ct = default)
    {
        var query = db.HistorialPreciosIngredientes.Where(h => h.IngredienteId == ingredienteId);
        if (aFecha.HasValue) query = query.Where(h => h.Fecha <= aFecha.Value);
        return query.OrderByDescending(h => h.Fecha).ThenByDescending(h => h.Id).FirstOrDefaultAsync(ct);
    }

    public Task<List<HistorialPrecioIngrediente>> GetHistorialAsync(int ingredienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
    {
        var query = db.HistorialPreciosIngredientes.Where(h => h.IngredienteId == ingredienteId);
        if (desde.HasValue) query = query.Where(h => h.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(h => h.Fecha <= hasta.Value);
        return query.OrderBy(h => h.Fecha).ToListAsync(ct);
    }

    public Task<int> ContarRecetasQueLoUsanAsync(int ingredienteId, CancellationToken ct = default) =>
        db.RecetaIngredientes
            .Where(ri => ri.IngredienteId == ingredienteId)
            .Select(ri => ri.RecetaId)
            .Distinct()
            .CountAsync(ct);

    public Task DeleteAsync(Ingrediente ingrediente, CancellationToken ct = default)
    {
        db.Ingredientes.Remove(ingrediente);
        return Task.CompletedTask;
    }

    public async Task EliminarPrecioAsync(int historialId, CancellationToken ct = default)
    {
        var registro = await db.HistorialPreciosIngredientes.FindAsync([historialId], ct);
        if (registro is not null) db.HistorialPreciosIngredientes.Remove(registro);
    }
}

public class MaterialRepository(MedialuncitaDbContext db) : IMaterialRepository
{
    public Task<Material?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Materiales.Include(m => m.UnidadCompra).FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<List<Material>> GetAllActivosAsync(CancellationToken ct = default) =>
        db.Materiales.Include(m => m.UnidadCompra).Where(m => m.Activo).OrderBy(m => m.Nombre).ToListAsync(ct);

    public async Task AddAsync(Material material, CancellationToken ct = default) =>
        await db.Materiales.AddAsync(material, ct);

    public async Task<int> AgregarPrecioAsync(HistorialPrecioMaterial historial, CancellationToken ct = default)
    {
        await db.HistorialPreciosMateriales.AddAsync(historial, ct);
        await db.SaveChangesAsync(ct); // necesario para que el Id devuelto sea el real (autoincremental de SQLite)
        return historial.Id;
    }

    public Task<HistorialPrecioMaterial?> GetPrecioVigenteAsync(int materialId, DateTime? aFecha = null, CancellationToken ct = default)
    {
        var query = db.HistorialPreciosMateriales.Where(h => h.MaterialId == materialId);
        if (aFecha.HasValue) query = query.Where(h => h.Fecha <= aFecha.Value);
        return query.OrderByDescending(h => h.Fecha).ThenByDescending(h => h.Id).FirstOrDefaultAsync(ct);
    }

    public Task<List<HistorialPrecioMaterial>> GetHistorialAsync(int materialId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
    {
        var query = db.HistorialPreciosMateriales.Where(h => h.MaterialId == materialId);
        if (desde.HasValue) query = query.Where(h => h.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(h => h.Fecha <= hasta.Value);
        return query.OrderBy(h => h.Fecha).ToListAsync(ct);
    }

    public Task<int> ContarVariantesQueLoUsanAsync(int materialId, CancellationToken ct = default) =>
        db.VarianteMateriales
            .Where(vm => vm.MaterialId == materialId)
            .Select(vm => vm.VarianteId)
            .Distinct()
            .CountAsync(ct);

    public Task DeleteAsync(Material material, CancellationToken ct = default)
    {
        db.Materiales.Remove(material);
        return Task.CompletedTask;
    }

    public async Task EliminarPrecioAsync(int historialId, CancellationToken ct = default)
    {
        var registro = await db.HistorialPreciosMateriales.FindAsync([historialId], ct);
        if (registro is not null) db.HistorialPreciosMateriales.Remove(registro);
    }
}
