using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Medialuncita.Infrastructure.Data;

/// <summary>
/// Permite ejecutar `dotnet ef migrations add/update` apuntando directamente a este
/// proyecto (Infrastructure) sin necesitar levantar MAUI o Web como "startup project".
/// Usa un archivo .db de diseño separado del de runtime; no se usa en producción.
/// </summary>
public class MedialuncitaDbContextFactory : IDesignTimeDbContextFactory<MedialuncitaDbContext>
{
    public MedialuncitaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MedialuncitaDbContext>();
        optionsBuilder.UseSqlite("Data Source=medialuncita.design.db");
        return new MedialuncitaDbContext(optionsBuilder.Options);
    }
}
