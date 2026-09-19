using Medialuncita.Application;
using Medialuncita.UI.Services;
using Medialuncita.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Medialuncita.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Solo se registra Application acá. Infrastructure (EF Core + SQLite) NO se registra
// todavía para este host: ver nota en Medialuncita.Web.csproj y en el README sobre
// la estrategia de persistencia pendiente para Web/PWA (Fase 3 del roadmap).
builder.Services.AddApplication();

// Entrega de archivos generados (ej. PDF de presupuestos) vía descarga de navegador.
builder.Services.AddScoped<IArchivoDescargaService, ArchivoDescargaServiceWeb>();

await builder.Build().RunAsync();
