using System.Globalization;
using System.Text;

namespace Medialuncita.Application.Presupuestos.Pdf;

/// <summary>
/// Escritor de PDF (1.4) mínimo, sin dependencias de NuGet. Genera páginas A4 con texto
/// posicionado absolutamente y líneas simples, usando las fuentes estándar Helvetica /
/// Helvetica-Bold (no requieren embeber datos de fuente). Los streams de contenido se
/// escriben sin comprimir: el documento resultante es más pesado que uno con FlateDecode,
/// pero para un presupuesto de una imprenta casera el tamaño es irrelevante y se gana
/// simplicidad total (nada de zlib, nada de librerías externas).
///
/// No es específico de Presupuesto: sabe dibujar texto y líneas en páginas A4, nada más.
/// La composición del contenido (qué texto, en qué posición) vive en <see cref="PresupuestoPdfService"/>.
/// </summary>
public sealed class MinimalPdfDocument
{
    public const double PageWidth = 595.28;  // A4 en puntos (72 dpi)
    public const double PageHeight = 841.89;

    private readonly List<List<string>> _pageOperators = new();

    /// <summary>Empieza una página nueva. Debe llamarse al menos una vez antes de dibujar.</summary>
    public void NuevaPagina() => _pageOperators.Add(new List<string>());

    public int PaginaActualIndice => _pageOperators.Count - 1;

    public int CantidadPaginas => _pageOperators.Count;

    /// <summary>Dibuja texto con posición absoluta (origen: esquina inferior izquierda de la página).</summary>
    public void DibujarTexto(double x, double y, PdfFont font, double size, string texto)
    {
        var ops = _pageOperators[^1];
        var escapado = EscaparTexto(texto);
        ops.Add("BT");
        ops.Add($"/{(font == PdfFont.Bold ? "F2" : "F1")} {Fmt(size)} Tf");
        ops.Add($"1 0 0 1 {Fmt(x)} {Fmt(y)} Tm");
        ops.Add($"({escapado}) Tj");
        ops.Add("ET");
    }

    /// <summary>Dibuja texto alineado a la derecha del punto (xDerecha, y), usando el ancho
    /// real de la fuente Helvetica estándar para calcular dónde empezar.</summary>
    public void DibujarTextoDerecha(double xDerecha, double y, PdfFont font, double size, string texto)
    {
        var ancho = HelveticaMetrics.AnchoTexto(texto, size);
        DibujarTexto(xDerecha - ancho, y, font, size, texto);
    }

    public void DibujarLinea(double x1, double y1, double x2, double y2, double grosor = 0.5)
    {
        var ops = _pageOperators[^1];
        ops.Add($"{Fmt(grosor)} w");
        ops.Add($"{Fmt(x1)} {Fmt(y1)} m");
        ops.Add($"{Fmt(x2)} {Fmt(y2)} l");
        ops.Add("S");
    }

    /// <summary>Serializa el documento completo a bytes PDF válidos.</summary>
    public byte[] Render()
    {
        if (_pageOperators.Count == 0)
            throw new InvalidOperationException("El documento no tiene páginas.");

        using var stream = new MemoryStream();
        var offsets = new List<long>();

        void Write(string s)
        {
            var bytes = Encoding.Latin1.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        void BeginObj(int num)
        {
            offsets.Add(stream.Position);
            Write($"{num} 0 obj\n");
        }

        Write("%PDF-1.4\n");
        // Línea binaria de rigor para que herramientas que detectan PDFs binarios no lo traten como texto puro.
        stream.Write(new byte[] { 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A }, 0, 6);

        var cantidadPaginas = _pageOperators.Count;
        // Numeración de objetos: 1=Catalog, 2=Pages, 3=Font Helvetica, 4=Font Helvetica-Bold,
        // luego para cada página: (5+2i)=Page, (6+2i)=Contents.
        const int catalogNum = 1;
        const int pagesNum = 2;
        const int fontRegularNum = 3;
        const int fontBoldNum = 4;
        var pageObjNums = new int[cantidadPaginas];
        var contentObjNums = new int[cantidadPaginas];
        for (var i = 0; i < cantidadPaginas; i++)
        {
            pageObjNums[i] = 5 + i * 2;
            contentObjNums[i] = 6 + i * 2;
        }
        var totalObjetos = 4 + cantidadPaginas * 2;

        // 1) Catalog
        BeginObj(catalogNum);
        Write($"<< /Type /Catalog /Pages {pagesNum} 0 R >>\nendobj\n");

        // 2) Pages
        BeginObj(pagesNum);
        var kids = string.Join(" ", pageObjNums.Select(n => $"{n} 0 R"));
        Write($"<< /Type /Pages /Kids [{kids}] /Count {cantidadPaginas} >>\nendobj\n");

        // 3) Fuente Helvetica
        BeginObj(fontRegularNum);
        Write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

        // 4) Fuente Helvetica-Bold
        BeginObj(fontBoldNum);
        Write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        // 5..) Páginas + contenidos
        for (var i = 0; i < cantidadPaginas; i++)
        {
            var contenido = string.Join('\n', _pageOperators[i]);
            var contenidoBytes = Encoding.Latin1.GetBytes(contenido);

            BeginObj(pageObjNums[i]);
            Write(
                $"<< /Type /Page /Parent {pagesNum} 0 R "
                + $"/MediaBox [0 0 {Fmt(PageWidth)} {Fmt(PageHeight)}] "
                + $"/Resources << /Font << /F1 {fontRegularNum} 0 R /F2 {fontBoldNum} 0 R >> >> "
                + $"/Contents {contentObjNums[i]} 0 R >>\nendobj\n");

            BeginObj(contentObjNums[i]);
            Write($"<< /Length {contenidoBytes.Length} >>\nstream\n");
            stream.Write(contenidoBytes, 0, contenidoBytes.Length);
            Write("\nendstream\nendobj\n");
        }

        // xref
        var xrefStart = stream.Position;
        Write($"xref\n0 {totalObjetos + 1}\n");
        Write("0000000000 65535 f \n");
        foreach (var offset in offsets)
            Write($"{offset:D10} 00000 n \n");

        Write("trailer\n");
        Write($"<< /Size {totalObjetos + 1} /Root {catalogNum} 0 R >>\n");
        Write("startxref\n");
        Write($"{xrefStart}\n");
        Write("%%EOF");

        return stream.ToArray();
    }

    private static string Fmt(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string EscaparTexto(string texto)
    {
        // Las cadenas PDF van entre paréntesis: hay que escapar \, ( y ).
        // Se codifica en Latin-1/WinAnsi más adelante; los caracteres fuera de ese rango
        // (emojis, etc.) se reemplazan por '?' para no romper el documento.
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
        {
            if (c is '\\' or '(' or ')') sb.Append('\\');
            sb.Append(c <= 0xFF ? c : '?');
        }
        return sb.ToString();
    }
}

public enum PdfFont
{
    Regular,
    Bold
}

/// <summary>
/// Anchos de carácter de Helvetica regular (en milésimas de em, valores AFM estándar).
/// Se usan solo para calcular alineación a la derecha de columnas numéricas; no afecta
/// el renderizado real (eso lo hace el visor de PDF con la fuente real).
/// </summary>
internal static class HelveticaMetrics
{
    private const int AnchoPorDefecto = 556;

    private static readonly Dictionary<char, int> Anchos = new()
    {
        [' '] = 278,
        ['!'] = 278,
        ['"'] = 355,
        ['#'] = 556,
        ['$'] = 556,
        ['%'] = 889,
        ['&'] = 667,
        ['\''] = 191,
        ['('] = 333,
        [')'] = 333,
        ['*'] = 389,
        ['+'] = 584,
        [','] = 278,
        ['-'] = 333,
        ['.'] = 278,
        ['/'] = 278,
        ['0'] = 556, ['1'] = 556, ['2'] = 556, ['3'] = 556, ['4'] = 556,
        ['5'] = 556, ['6'] = 556, ['7'] = 556, ['8'] = 556, ['9'] = 556,
        [':'] = 278,
        [';'] = 278,
        ['<'] = 584,
        ['='] = 584,
        ['>'] = 584,
        ['?'] = 556,
        ['@'] = 1015,
        ['A'] = 667, ['B'] = 667, ['C'] = 722, ['D'] = 722, ['E'] = 667,
        ['F'] = 611, ['G'] = 778, ['H'] = 722, ['I'] = 278, ['J'] = 500,
        ['K'] = 667, ['L'] = 556, ['M'] = 833, ['N'] = 722, ['O'] = 778,
        ['P'] = 667, ['Q'] = 778, ['R'] = 722, ['S'] = 667, ['T'] = 611,
        ['U'] = 722, ['V'] = 667, ['W'] = 944, ['X'] = 667, ['Y'] = 667, ['Z'] = 611,
        ['a'] = 556, ['b'] = 556, ['c'] = 500, ['d'] = 556, ['e'] = 556,
        ['f'] = 278, ['g'] = 556, ['h'] = 556, ['i'] = 222, ['j'] = 222,
        ['k'] = 500, ['l'] = 222, ['m'] = 833, ['n'] = 556, ['o'] = 556,
        ['p'] = 556, ['q'] = 556, ['r'] = 333, ['s'] = 500, ['t'] = 278,
        ['u'] = 556, ['v'] = 500, ['w'] = 722, ['x'] = 500, ['y'] = 500, ['z'] = 500,
    };

    public static double AnchoTexto(string texto, double size)
    {
        var milesimas = 0;
        foreach (var c in texto)
            milesimas += Anchos.GetValueOrDefault(c, AnchoPorDefecto);
        return milesimas / 1000.0 * size;
    }
}
