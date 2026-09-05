using Medialuncita.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medialuncita.Infrastructure.Data;

public class MedialuncitaDbContext : DbContext
{
    public MedialuncitaDbContext(DbContextOptions<MedialuncitaDbContext> options) : base(options) { }

    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();

    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
    public DbSet<HistorialPrecioIngrediente> HistorialPreciosIngredientes => Set<HistorialPrecioIngrediente>();

    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<HistorialPrecioMaterial> HistorialPreciosMateriales => Set<HistorialPrecioMaterial>();

    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<RecetaIngrediente> RecetaIngredientes => Set<RecetaIngrediente>();

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoVariante> ProductoVariantes => Set<ProductoVariante>();
    public DbSet<VarianteIngredienteOverride> VarianteIngredienteOverrides => Set<VarianteIngredienteOverride>();
    public DbSet<VarianteMaterial> VarianteMateriales => Set<VarianteMaterial>();

    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<RecetaServicio> RecetaServicios => Set<RecetaServicio>();
    public DbSet<VarianteServicio> VarianteServicios => Set<VarianteServicio>();

    public DbSet<ConfiguracionGlobal> ConfiguracionGlobal => Set<ConfiguracionGlobal>();

    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<PresupuestoItem> PresupuestoItems => Set<PresupuestoItem>();
    public DbSet<PresupuestoItemIngredienteDetalle> PresupuestoItemIngredienteDetalles => Set<PresupuestoItemIngredienteDetalle>();
    public DbSet<PresupuestoItemMaterialDetalle> PresupuestoItemMaterialDetalles => Set<PresupuestoItemMaterialDetalle>();
    public DbSet<PresupuestoItemServicioDetalle> PresupuestoItemServicioDetalles => Set<PresupuestoItemServicioDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Precision decimal explícita: SQLite no tiene tipo decimal nativo (EF lo mapea a
        // TEXT), pero fijamos la precisión igual para que el modelo sea explícito y
        // portable si el día de mañana se migra a otro motor.
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(6);
        }

        modelBuilder.Entity<UnidadMedida>(e =>
        {
            e.HasIndex(u => u.Nombre).IsUnique();
        });

        modelBuilder.Entity<Ingrediente>(e =>
        {
            e.HasOne(i => i.UnidadCompra).WithMany().HasForeignKey(i => i.UnidadCompraId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(i => i.Nombre);
        });

        modelBuilder.Entity<HistorialPrecioIngrediente>(e =>
        {
            e.HasOne(h => h.Ingrediente).WithMany(i => i.HistorialPrecios).HasForeignKey(h => h.IngredienteId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.IngredienteId, h.Fecha }); // resolución rápida de "precio vigente"
        });

        modelBuilder.Entity<Material>(e =>
        {
            e.HasOne(m => m.UnidadCompra).WithMany().HasForeignKey(m => m.UnidadCompraId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistorialPrecioMaterial>(e =>
        {
            e.HasOne(h => h.Material).WithMany(m => m.HistorialPrecios).HasForeignKey(h => h.MaterialId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.MaterialId, h.Fecha });
        });

        modelBuilder.Entity<Receta>(e =>
        {
            e.HasOne(r => r.RendimientoBaseUnidad).WithMany().HasForeignKey(r => r.RendimientoBaseUnidadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecetaIngrediente>(e =>
        {
            e.HasOne(ri => ri.Receta).WithMany(r => r.Ingredientes).HasForeignKey(ri => ri.RecetaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ri => ri.Ingrediente).WithMany().HasForeignKey(ri => ri.IngredienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ri => ri.Unidad).WithMany().HasForeignKey(ri => ri.UnidadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.HasOne(p => p.Receta).WithMany().HasForeignKey(p => p.RecetaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductoVariante>(e =>
        {
            e.HasOne(v => v.Producto).WithMany(p => p.Variantes).HasForeignKey(v => v.ProductoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.RendimientoUnidad).WithMany().HasForeignKey(v => v.RendimientoUnidadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VarianteIngredienteOverride>(e =>
        {
            e.HasOne(o => o.Variante).WithMany(v => v.IngredienteOverrides).HasForeignKey(o => o.VarianteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.Ingrediente).WithMany().HasForeignKey(o => o.IngredienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Unidad).WithMany().HasForeignKey(o => o.UnidadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VarianteMaterial>(e =>
        {
            e.HasOne(vm => vm.Variante).WithMany(v => v.Materiales).HasForeignKey(vm => vm.VarianteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(vm => vm.Material).WithMany().HasForeignKey(vm => vm.MaterialId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecetaServicio>(e =>
        {
            e.HasOne(rs => rs.Receta).WithMany(r => r.Servicios).HasForeignKey(rs => rs.RecetaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rs => rs.Servicio).WithMany().HasForeignKey(rs => rs.ServicioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VarianteServicio>(e =>
        {
            e.HasOne(vs => vs.Variante).WithMany(v => v.Servicios).HasForeignKey(vs => vs.VarianteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(vs => vs.Servicio).WithMany().HasForeignKey(vs => vs.ServicioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Presupuesto>(e =>
        {
            e.HasMany(p => p.Items).WithOne(i => i.Presupuesto).HasForeignKey(i => i.PresupuestoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PresupuestoItem>(e =>
        {
            // Sin FK real a ProductoVariante: es solo trazabilidad (puede quedar huérfana).
            e.Property(i => i.ProductoVarianteId).IsRequired(false);
            e.HasMany(i => i.DetalleIngredientes).WithOne(d => d.PresupuestoItem!).HasForeignKey(d => d.PresupuestoItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(i => i.DetalleMateriales).WithOne(d => d.PresupuestoItem!).HasForeignKey(d => d.PresupuestoItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(i => i.DetalleServicios).WithOne(d => d.PresupuestoItem!).HasForeignKey(d => d.PresupuestoItemId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed de configuración global por defecto (fila única Id=1).
        modelBuilder.Entity<ConfiguracionGlobal>().HasData(new ConfiguracionGlobal
        {
            Id = 1,
            TarifaManoDeObraPorHora = 0m,
            EstrategiaPrecioDefault = Domain.Enums.EstrategiaPrecio.Margen,
            MargenPorcentualDefault = 0m,
            MultiplicadorDefault = 1m,
            EstrategiaRedondeoDefault = Domain.Enums.EstrategiaRedondeo.SinRedondeo
        });
    }
}
