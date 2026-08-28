using Medialuncita.Application.Abstractions;
using Medialuncita.Infrastructure.Data;
using Medialuncita.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Medialuncita.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra el DbContext apuntando a un archivo SQLite local (offline-first: no hay
    /// ninguna otra fuente de datos posible en esta capa) y todos los repositorios.
    /// </summary>
    /// <param name="sqliteDbPath">Ruta absoluta al archivo .db. Cada host (MAUI/Web) decide
    /// dónde vive ese archivo según las convenciones de su plataforma.</param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqliteDbPath)
    {
        services.AddDbContext<MedialuncitaDbContext>(options =>
            options.UseSqlite($"Data Source={sqliteDbPath}"));

        services.AddScoped<IUnidadMedidaRepository, UnidadMedidaRepository>();
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IRecetaRepository, RecetaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();
        services.AddScoped<IConfiguracionGlobalRepository, ConfiguracionGlobalRepository>();
        services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
