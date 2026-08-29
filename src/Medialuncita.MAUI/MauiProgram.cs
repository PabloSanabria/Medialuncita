// Reemplazar el contenido de MauiProgram.cs (generado por `dotnet new maui-blazor`)
// por esto, ajustando el namespace si hace falta.

using Medialuncita.Application;
using Medialuncita.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Medialuncita.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Base de datos SQLite local, offline-first. FileSystem.AppDataDirectory
        // resuelve a una carpeta privada de la app tanto en Android como en Windows,
        // sin código específico de plataforma.
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "medialuncita.db");

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(dbPath);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Aplica migraciones pendientes al arrancar (crea el archivo .db si no existe).
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Medialuncita.Infrastructure.Data.MedialuncitaDbContext>();
            db.Database.Migrate();
        }

        return app;
    }
}
