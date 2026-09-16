using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Presupuestos.Pdf;

/// <summary>
/// Genera el PDF de un presupuesto YA GUARDADO, a partir exclusivamente del snapshot
/// congelado en <see cref="Presupuesto"/>/<see cref="PresupuestoItem"/>. No consulta
/// precios vigentes, no recalcula costos ni vuelve a correr CosteoService: solo
/// formatea a PDF los valores que ya están persistidos.
/// </summary>
public interface IPresupuestoPdfService
{
    /// <summary>Devuelve los bytes del PDF. No hace I/O de archivo: el caller (UI) decide
    /// qué hacer con los bytes (descargar, guardar, etc.).</summary>
    byte[] GenerarPdf(Presupuesto presupuesto);
}
