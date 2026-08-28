using Medialuncita.Application.Costeo;
using Medialuncita.Application.Precios;
using Medialuncita.Application.Presupuestos;
using Microsoft.Extensions.DependencyInjection;

namespace Medialuncita.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICosteoService, CosteoService>();
        services.AddScoped<IPrecioConsultaService, PrecioConsultaService>();
        services.AddScoped<IPresupuestoService, PresupuestoService>();
        return services;
    }
}
