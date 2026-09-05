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
        var entry = await db.HistorialPreciosIngredientes.AddAsync(historial, ct);
        return entry.Entity.Id;
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
        var entry = await db.HistorialPreciosMateriales.AddAsync(historial, ct);
        return entry.Entity.Id;
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
}
