using Medialuncita.UI.Services;
using Microsoft.JSInterop;

namespace Medialuncita.Web.Services;

/// <summary>
/// Implementación para el host Web/WASM: reutiliza el JS ya existente
/// (Medialuncita.UI/wwwroot/js/archivos.js) para disparar la descarga del archivo vía
/// Blob + &lt;a download&gt; del navegador, que en un navegador real sí funciona.
/// </summary>
public class ArchivoDescargaServiceWeb : IArchivoDescargaService
{
    private readonly IJSRuntime _js;

    public ArchivoDescargaServiceWeb(IJSRuntime js) => _js = js;

    public async Task DescargarAsync(string nombreArchivo, byte[] contenido, string tipoMime)
    {
        var base64 = Convert.ToBase64String(contenido);
        await _js.InvokeVoidAsync("medialuncita.descargarArchivo", nombreArchivo, base64, tipoMime);
    }
}
