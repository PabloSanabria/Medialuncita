using Medialuncita.Application.Abstractions;
using Medialuncita.Domain.Entities;
using Medialuncita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Medialuncita.Infrastructure.Repositories;

public class ServicioRepository(MedialuncitaDbContext db) : IServicioRepository
{
    public Task<Servicio?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Servicios.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Servicio>> GetAllActivosAsync(CancellationToken ct = default) =>
        db.Servicios.Where(s => s.Activo).OrderBy(s => s.Nombre).ToListAsync(ct);

    public async Task AddAsync(Servicio servicio, CancellationToken ct = default) =>
        await db.Servicios.AddAsync(servicio, ct);

    public async Task<int> ContarUsosAsync(int servicioId, CancellationToken ct = default)
    {
        var enRecetas = await db.RecetaServicios.Select(rs => new { rs.ServicioId, rs.RecetaId })
            .Where(x => x.ServicioId == servicioId).Select(x => x.RecetaId).Distinct().CountAsync(ct);
        var enVariantes = await db.VarianteServicios.Select(vs => new { vs.ServicioId, vs.VarianteId })
            .Where(x => x.ServicioId == servicioId).Select(x => x.VarianteId).Distinct().CountAsync(ct);
        return enRecetas + enVariantes;
    }

    public Task DeleteAsync(Servicio servicio, CancellationToken ct = default)
    {
        db.Servicios.Remove(servicio);
        return Task.CompletedTask;
    }
}

public class ConfiguracionGlobalRepository(MedialuncitaDbContext db) : IConfiguracionGlobalRepository
{
    public async Task<ConfiguracionGlobal> GetAsync(CancellationToken ct = default)
    {
        // Fila única (seedeada con Id=1 en las migraciones). Si por algún motivo no
        // existe, se crea una por defecto para no romper el flujo de costeo.
        var config = await db.ConfiguracionGlobal.FirstOrDefaultAsync(c => c.Id == 1, ct);
        if (config is not null) return config;

        config = new ConfiguracionGlobal { Id = 1 };
        await db.ConfiguracionGlobal.AddAsync(config, ct);
        await db.SaveChangesAsync(ct);
        return config;
    }

    public async Task SaveAsync(ConfiguracionGlobal configuracion, CancellationToken ct = default)
    {
        db.ConfiguracionGlobal.Update(configuracion);
        await db.SaveChangesAsync(ct);
    }
}

public class PresupuestoRepository(MedialuncitaDbContext db) : IPresupuestoRepository
{
    public async Task AddAsync(Presupuesto presupuesto, CancellationToken ct = default) =>
        await db.Presupuestos.AddAsync(presupuesto, ct);

    public Task<Presupuesto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Presupuestos
            .Include(p => p.Items).ThenInclude(i => i.DetalleIngredientes)
            .Include(p => p.Items).ThenInclude(i => i.DetalleMateriales)
            .Include(p => p.Items).ThenInclude(i => i.DetalleServicios)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Presupuesto>> GetAllAsync(CancellationToken ct = default) =>
        db.Presupuestos.OrderByDescending(p => p.Fecha).ToListAsync(ct);
}

public class EfUnitOfWork(MedialuncitaDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}