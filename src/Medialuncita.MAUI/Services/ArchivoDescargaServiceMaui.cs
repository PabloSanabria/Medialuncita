using Medialuncita.UI.Services;

namespace Medialuncita.MAUI.Services;

/// <summary>
/// Implementación para el host MAUI (Windows y Android).
///
/// El WebView embebido de Blazor Hybrid no tiene un "chrome" de navegador que sepa
/// resolver la descarga de un Blob JS: en Windows (WebView2) el click en un
/// &lt;a download&gt; termina funcionando porque WebView2 delega en el shell de Windows,
/// pero en Android el WebView del sistema simplemente lo ignora en silencio (no hay
/// gestor de descargas nativo enganchado) — el botón "Generar PDF" parecía funcionar
/// pero no producía ningún archivo visible para el usuario.
///
/// En vez de depender de eso, se escribe el archivo a una carpeta temporal propia de la
/// app y se dispara el share sheet nativo de MAUI Essentials (Share.Default), que en
/// Android permite guardar/abrir/compartir el PDF y en Windows abre el diálogo
/// equivalente. Mismo código para ambas plataformas, sin `#if` ni carpetas
/// Platforms/*.
/// </summary>
public class ArchivoDescargaServiceMaui : IArchivoDescargaService
{
    public async Task DescargarAsync(string nombreArchivo, byte[] contenido, string tipoMime)
    {
        var ruta = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);
        await File.WriteAllBytesAsync(ruta, contenido);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Guardar o compartir presupuesto",
            File = new ShareFile(ruta)
        });
    }
}
