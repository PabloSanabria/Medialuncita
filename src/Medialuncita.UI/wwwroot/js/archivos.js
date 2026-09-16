// Descarga un archivo generado en el servidor/UI (bytes en base64) desde Blazor.
// Usado por PresupuestoDetalle.razor para el botón "Generar PDF". No depende de
// ninguna librería externa: crea un Blob y simula el click de un <a download>.
window.medialuncita = window.medialuncita || {};

window.medialuncita.descargarArchivo = (nombreArchivo, contenidoBase64, tipoMime) => {
    const binario = atob(contenidoBase64);
    const bytes = new Uint8Array(binario.length);
    for (let i = 0; i < binario.length; i++) {
        bytes[i] = binario.charCodeAt(i);
    }

    const blob = new Blob([bytes], { type: tipoMime || "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = url;
    link.download = nombreArchivo;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
};
