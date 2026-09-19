namespace Medialuncita.UI.Services;

/// <summary>
/// Abstrae cómo se le entrega al usuario un archivo generado en memoria (hoy, el PDF de
/// un presupuesto). Cada host lo implementa según sus posibilidades:
/// - MAUI (Windows y Android): escribe el archivo a una carpeta temporal y dispara el
///   share sheet nativo (ver Medialuncita.MAUI/Services/ArchivoDescargaServiceMaui.cs).
/// - Web: dispara una descarga de navegador vía JS interop (ver
///   Medialuncita.Web/Services/ArchivoDescargaServiceWeb.cs).
/// La UI compartida (Razor) no sabe ni le importa cuál implementación está activa.
/// </summary>
public interface IArchivoDescargaService
{
    Task DescargarAsync(string nombreArchivo, byte[] contenido, string tipoMime);
}
