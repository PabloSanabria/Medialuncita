using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Presupuestos.Pdf;

/// <summary>
/// Compone el layout del PDF de un presupuesto. Solo lee campos ya persistidos de
/// <see cref="Presupuesto"/>/<see cref="PresupuestoItem"/> (snapshot congelado): nunca
/// consulta repositorios, servicios de precios ni CosteoService. Si el presupuesto tiene
/// más ítems de los que entran en una página, pagina automáticamente.
/// </summary>
public sealed class PresupuestoPdfService : IPresupuestoPdfService
{
    // Layout en puntos (A4 — ver MinimalPdfDocument.PageWidth/PageHeight).
    private const double MarginX = 50;
    private const double MarginTop = 50;
    private const double MarginBottom = 50;
    private const double RowHeight = 16;

    // Espacio reservado al pie en TODAS las páginas (para que el total y la nota siempre
    // entren en la última, sin tener que recalcular en dos pasadas).
    private const double ReservedFooter = 85;

    // Columnas de la tabla (coordenadas X absolutas; ColSubtotalRight = borde derecho de página).
    private const double ColProductoX = MarginX;
    private const double ColVarianteX = MarginX + 150;
    private const double ColCantidadRight = MarginX + 330;
    private const double ColPrecioRight = MarginX + 420;
    private const double ColSubtotalRight = MarginX + 495;

    public byte[] GenerarPdf(Presupuesto presupuesto)
    {
        ArgumentNullException.ThrowIfNull(presupuesto);

        var doc = new MinimalPdfDocument();
        var topY = MinimalPdfDocument.PageHeight - MarginTop;
        const double bottomY = MarginBottom;

        var tieneCliente = !string.IsNullOrWhiteSpace(presupuesto.ClienteNombre);
        var tieneNotas = !string.IsNullOrWhiteSpace(presupuesto.Notas);

        // Debe reflejar EXACTAMENTE la suma de los "y -= N" que se ejecutan más abajo antes
        // de la primera fila de datos (incluyendo los 18pt que consume DibujarEncabezadoTabla:
        // 6 hasta la línea + 12 hasta la fila). Si se cambia el layout de dibujo, actualizar acá.
        var alturaHeaderPagina1 = 26 + 18 + 15 + (tieneCliente ? 15 : 0) + (tieneNotas ? 13 : 0) + 10 + 18 + 18;
        const double alturaHeaderContinuacion = 24 + 18;

        var espacioPagina1 = topY - alturaHeaderPagina1 - bottomY - ReservedFooter;
        var espacioOtras = topY - alturaHeaderContinuacion - bottomY - ReservedFooter;

        var rowsPrimeraPagina = Math.Max(1, (int)Math.Floor(espacioPagina1 / RowHeight));
        var rowsOtrasPaginas = Math.Max(1, (int)Math.Floor(espacioOtras / RowHeight));

        var items = presupuesto.Items.ToList();
        var totalPaginas = 1;
        if (items.Count > rowsPrimeraPagina)
        {
            var restantes = items.Count - rowsPrimeraPagina;
            totalPaginas += (int)Math.Ceiling(restantes / (double)rowsOtrasPaginas);
        }

        var indiceItem = 0;
        for (var pagina = 1; pagina <= totalPaginas; pagina++)
        {
            doc.NuevaPagina();
            double y;

            if (pagina == 1)
            {
                y = topY;
                doc.DibujarTexto(MarginX, y, PdfFont.Bold, 20, "Medialuncita");
                y -= 26;
                doc.DibujarTexto(MarginX, y, PdfFont.Bold, 13, $"Presupuesto Nº {presupuesto.Id}");
                y -= 18;
                doc.DibujarTexto(MarginX, y, PdfFont.Regular, 10, $"Fecha: {presupuesto.Fecha:dd/MM/yyyy}");
                y -= 15;
                if (tieneCliente)
                {
                    doc.DibujarTexto(MarginX, y, PdfFont.Regular, 10, $"Cliente: {presupuesto.ClienteNombre}");
                    y -= 15;
                }
                if (tieneNotas)
                {
                    doc.DibujarTexto(MarginX, y, PdfFont.Regular, 9, $"Notas: {Truncar(presupuesto.Notas!, 90)}");
                    y -= 13;
                }
                y -= 10;
                doc.DibujarLinea(MarginX, y, ColSubtotalRight, y);
                y -= 18;
            }
            else
            {
                y = topY;
                doc.DibujarTexto(MarginX, y, PdfFont.Bold, 12, $"Presupuesto Nº {presupuesto.Id} (continuación)");
                y -= 24;
            }

            y = DibujarEncabezadoTabla(doc, y);

            var rowsEstaPagina = pagina == 1 ? rowsPrimeraPagina : rowsOtrasPaginas;
            var hastaIndice = Math.Min(items.Count, indiceItem + rowsEstaPagina);
            for (; indiceItem < hastaIndice; indiceItem++)
            {
                DibujarFila(doc, y, items[indiceItem]);
                y -= RowHeight;
            }

            doc.DibujarLinea(MarginX, y + 6, ColSubtotalRight, y + 6);

            if (pagina == totalPaginas)
            {
                y -= 16;
                doc.DibujarTextoDerecha(ColSubtotalRight, y, PdfFont.Bold, 12,
                    $"Total: {FormatearMoneda(presupuesto.Total)}");
                y -= 22;
                doc.DibujarTexto(MarginX, y, PdfFont.Regular, 8,
                    "Los valores de este presupuesto quedaron congelados al momento de su generacion");
                y -= 10;
                doc.DibujarTexto(MarginX, y, PdfFont.Regular, 8,
                    "y no cambian aunque luego se actualicen precios, recetas o variantes.");
            }

            if (totalPaginas > 1)
                doc.DibujarTextoDerecha(ColSubtotalRight, 30, PdfFont.Regular, 8, $"Pagina {pagina} de {totalPaginas}");
        }

        return doc.Render();
    }

    private static double DibujarEncabezadoTabla(MinimalPdfDocument doc, double y)
    {
        doc.DibujarTexto(ColProductoX, y, PdfFont.Bold, 9, "Producto");
        doc.DibujarTexto(ColVarianteX, y, PdfFont.Bold, 9, "Variante");
        doc.DibujarTextoDerecha(ColCantidadRight, y, PdfFont.Bold, 9, "Cantidad");
        doc.DibujarTextoDerecha(ColPrecioRight, y, PdfFont.Bold, 9, "P. Unitario");
        doc.DibujarTextoDerecha(ColSubtotalRight, y, PdfFont.Bold, 9, "Subtotal");
        y -= 6;
        doc.DibujarLinea(MarginX, y, ColSubtotalRight, y);
        y -= 12;
        return y;
    }

    private static void DibujarFila(MinimalPdfDocument doc, double y, PresupuestoItem item)
    {
        doc.DibujarTexto(ColProductoX, y, PdfFont.Regular, 9, Truncar(item.NombreProductoSnapshot, 24));
        doc.DibujarTexto(ColVarianteX, y, PdfFont.Regular, 9, Truncar(item.NombreVarianteSnapshot, 20));
        doc.DibujarTextoDerecha(ColCantidadRight, y, PdfFont.Regular, 9, item.Cantidad.ToString());
        doc.DibujarTextoDerecha(ColPrecioRight, y, PdfFont.Regular, 9, FormatearMoneda(item.PrecioUnitarioAlMomento));
        doc.DibujarTextoDerecha(ColSubtotalRight, y, PdfFont.Regular, 9, FormatearMoneda(item.Subtotal));
    }

    /// <summary>Mismo formato que usa la pantalla PresupuestoDetalle.razor ($ + "0.##"),
    /// para que el PDF muestre exactamente lo mismo que ya ve Pablo en la UI.</summary>
    private static string FormatearMoneda(decimal valor) => $"${valor.ToString("0.##")}";

    private static string Truncar(string texto, int maxLen) =>
        texto.Length <= maxLen ? texto : texto[..(maxLen - 3)] + "...";
}
