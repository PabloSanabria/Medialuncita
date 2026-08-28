using Medialuncita.Application.Abstractions;
using Medialuncita.Domain.Entities;
using Medialuncita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Medialuncita.Infrastructure.Repositories;

public class RecetaRepository(MedialuncitaDbContext db) : IRecetaRepository
{
    public Task<Receta?> GetByIdConIngredientesAsync(int id, CancellationToken ct = default) =>
        db.Recetas
            .Include(r => r.RendimientoBaseUnidad)
            .Include(r => r.Ingredientes).ThenInclude(ri => ri.Ingrediente)
            .Include(r => r.Ingredientes).ThenInclude(ri => ri.Unidad)
            .Include(r => r.Servicios).ThenInclude(rs => rs.Servicio)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Receta>> GetAllActivasAsync(CancellationToken ct = default) =>
        db.Recetas.Where(r => r.Activa).OrderBy(r => r.Nombre).ToListAsync(ct);

    public async Task AddAsync(Receta receta, CancellationToken ct = default) =>
        await db.Recetas.AddAsync(receta, ct);
}

public class ProductoRepository(MedialuncitaDbContext db) : IProductoRepository
{
    public Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Productos.Include(p => p.Receta).Include(p => p.Variantes).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Producto>> GetAllActivosAsync(CancellationToken ct = default) =>
        db.Productos.Include(p => p.Receta).Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task AddAsync(Producto producto, CancellationToken ct = default) =>
        await db.Productos.AddAsync(producto, ct);

    /// <summary>
    /// Carga completa necesaria para costear: variante -> producto -> receta madre
    /// (con sus ingredientes y servicios), y de la propia variante sus overrides,
    /// materiales y servicios. Esta es la única query "pesada" de todo el repositorio,
    /// deliberadamente: el motor de costeo necesita el grafo completo en memoria.
    /// </summary>
    public Task<ProductoVariante?> GetVarianteParaCosteoAsync(int varianteId, CancellationToken ct = default) =>
        db.ProductoVariantes
            .Include(v => v.RendimientoUnidad)
            .Include(v => v.Producto!).ThenInclude(p => p.Receta!).ThenInclude(r => r.RendimientoBaseUnidad)
            .Include(v => v.Producto!).ThenInclude(p => p.Receta!).ThenInclude(r => r.Ingredientes).ThenInclude(ri => ri.Ingrediente).ThenInclude(i => i!.UnidadCompra)
            .Include(v => v.Producto!).ThenInclude(p => p.Receta!).ThenInclude(r => r.Ingredientes).ThenInclude(ri => ri.Unidad)
            .Include(v => v.Producto!).ThenInclude(p => p.Receta!).ThenInclude(r => r.Servicios).ThenInclude(rs => rs.Servicio)
            .Include(v => v.IngredienteOverrides).ThenInclude(o => o.Unidad)
            .Include(v => v.Materiales).ThenInclude(m => m.Material).ThenInclude(mat => mat!.UnidadCompra)
            .Include(v => v.Servicios).ThenInclude(vs => vs.Servicio)
            .FirstOrDefaultAsync(v => v.Id == varianteId, ct);
}
