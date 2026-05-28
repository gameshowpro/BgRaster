namespace GameshowPro.BgRaster.Rendering;

using System.Globalization;
using System.Xml;

static class SvgRenderer
{
    internal static bool TryRender(Stream svgStream, SKCanvas canvas, SKRect fitRect, byte alpha)
    {
        SvgDocument? doc;
        try
        {
            doc = SvgParser.Parse(svgStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SvgRenderer: status=svg-parse-failed reason=\"{ex.Message}\"");
            return false;
        }
        if (doc is null) return false;

        float scale = MathF.Min(fitRect.Width / doc.ViewBoxWidth, fitRect.Height / doc.ViewBoxHeight);
        float dw = doc.ViewBoxWidth * scale;
        float dh = doc.ViewBoxHeight * scale;
        float dx = fitRect.Left + (fitRect.Width - dw) / 2f;
        float dy = fitRect.Top + (fitRect.Height - dh) / 2f;

        canvas.Save();
        canvas.Translate(dx, dy);
        canvas.Scale(scale);
        canvas.Translate(-doc.ViewBoxX, -doc.ViewBoxY);

        foreach (SvgShape shape in doc.Shapes)
            shape.Draw(canvas, alpha);

        canvas.Restore();
        return true;
    }
}

sealed record SvgDocument(float ViewBoxX, float ViewBoxY, float ViewBoxWidth, float ViewBoxHeight, ImmutableArray<SvgShape> Shapes);

abstract record SvgShape(SKColor? Fill, SKColor? Stroke, float StrokeWidth, float Opacity)
{
    internal void Draw(SKCanvas canvas, byte alpha)
    {
        byte combinedAlpha = (byte)(Opacity * alpha);
        if (Fill is SKColor fillColor)
        {
            using SKPaint fillPaint = new()
            {
                Style = SKPaintStyle.Fill,
                Color = fillColor.WithAlpha((byte)(fillColor.Alpha * combinedAlpha / 255)),
                IsAntialias = true,
            };
            DrawFill(canvas, fillPaint);
        }
        if (Stroke is SKColor strokeColor && StrokeWidth > 0f)
        {
            using SKPaint strokePaint = new()
            {
                Style = SKPaintStyle.Stroke,
                Color = strokeColor.WithAlpha((byte)(strokeColor.Alpha * combinedAlpha / 255)),
                StrokeWidth = StrokeWidth,
                IsAntialias = true,
            };
            DrawStroke(canvas, strokePaint);
        }
    }

    protected abstract void DrawFill(SKCanvas canvas, SKPaint paint);
    protected abstract void DrawStroke(SKCanvas canvas, SKPaint paint);
}

sealed record SvgRect(float X, float Y, float Width, float Height, SKColor? Fill, SKColor? Stroke, float StrokeWidth, float Opacity)
    : SvgShape(Fill, Stroke, StrokeWidth, Opacity)
{
    protected override void DrawFill(SKCanvas canvas, SKPaint paint)
        => canvas.DrawRect(SKRect.Create(X, Y, Width, Height), paint);
    protected override void DrawStroke(SKCanvas canvas, SKPaint paint)
        => canvas.DrawRect(SKRect.Create(X, Y, Width, Height), paint);
}

sealed record SvgLine(float X1, float Y1, float X2, float Y2, SKColor? Stroke, float StrokeWidth, float Opacity)
    : SvgShape(null, Stroke, StrokeWidth, Opacity)
{
    protected override void DrawFill(SKCanvas canvas, SKPaint paint) { }
    protected override void DrawStroke(SKCanvas canvas, SKPaint paint)
        => canvas.DrawLine(X1, Y1, X2, Y2, paint);
}

sealed record SvgPath(SKPath Path, SKColor? Fill, SKColor? Stroke, float StrokeWidth, float Opacity)
    : SvgShape(Fill, Stroke, StrokeWidth, Opacity)
{
    protected override void DrawFill(SKCanvas canvas, SKPaint paint) => canvas.DrawPath(Path, paint);
    protected override void DrawStroke(SKCanvas canvas, SKPaint paint) => canvas.DrawPath(Path, paint);
}

static class SvgParser
{
    internal static SvgDocument? Parse(Stream stream)
    {
        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
        };

        float vbX = 0f, vbY = 0f, vbW = 100f, vbH = 100f;
        List<SvgShape> shapes = [];

        using XmlReader reader = XmlReader.Create(stream, settings);
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            switch (reader.LocalName)
            {
                case "svg":
                    string? viewBox = reader.GetAttribute("viewBox");
                    if (viewBox is not null && TryParseViewBox(viewBox, out float x, out float y, out float w, out float h))
                    {
                        vbX = x; vbY = y; vbW = w; vbH = h;
                    }
                    else
                    {
                        if (TryParseFloat(reader.GetAttribute("width"), out float wv)) vbW = wv;
                        if (TryParseFloat(reader.GetAttribute("height"), out float hv)) vbH = hv;
                    }
                    break;

                case "rect":
                    shapes.Add(new SvgRect(
                        ParseFloat(reader.GetAttribute("x"), 0f),
                        ParseFloat(reader.GetAttribute("y"), 0f),
                        ParseFloat(reader.GetAttribute("width"), 0f),
                        ParseFloat(reader.GetAttribute("height"), 0f),
                        ParseColor(reader.GetAttribute("fill"), defaultBlackIfMissing: true),
                        ParseColor(reader.GetAttribute("stroke"), defaultBlackIfMissing: false),
                        ParseFloat(reader.GetAttribute("stroke-width"), 1f),
                        ParseFloat(reader.GetAttribute("opacity"), 1f)));
                    break;

                case "line":
                    shapes.Add(new SvgLine(
                        ParseFloat(reader.GetAttribute("x1"), 0f),
                        ParseFloat(reader.GetAttribute("y1"), 0f),
                        ParseFloat(reader.GetAttribute("x2"), 0f),
                        ParseFloat(reader.GetAttribute("y2"), 0f),
                        ParseColor(reader.GetAttribute("stroke"), defaultBlackIfMissing: false),
                        ParseFloat(reader.GetAttribute("stroke-width"), 1f),
                        ParseFloat(reader.GetAttribute("opacity"), 1f)));
                    break;

                case "path":
                    string? d = reader.GetAttribute("d");
                    if (!string.IsNullOrEmpty(d))
                    {
                        SKPath path = ParsePathData(d);
                        shapes.Add(new SvgPath(
                            path,
                            ParseColor(reader.GetAttribute("fill"), defaultBlackIfMissing: true),
                            ParseColor(reader.GetAttribute("stroke"), defaultBlackIfMissing: false),
                            ParseFloat(reader.GetAttribute("stroke-width"), 1f),
                            ParseFloat(reader.GetAttribute("opacity"), 1f)));
                    }
                    break;
            }
        }

        return new SvgDocument(vbX, vbY, vbW, vbH, [.. shapes]);
    }

    static bool TryParseViewBox(string s, out float x, out float y, out float w, out float h)
    {
        x = y = w = h = 0f;
        string[] parts = s.Split([' ', ',', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return false;
        return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
            && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
            && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out w)
            && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out h);
    }

    static bool TryParseFloat(string? s, out float v)
    {
        if (s is null) { v = 0f; return false; }
        ReadOnlySpan<char> trimmed = s.AsSpan().Trim();
        int unitStart = trimmed.Length;
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if (!char.IsDigit(c) && c != '.' && c != '-' && c != '+' && c != 'e' && c != 'E')
            {
                unitStart = i;
                break;
            }
        }
        return float.TryParse(trimmed[..unitStart], NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    static float ParseFloat(string? s, float fallback) => TryParseFloat(s, out float v) ? v : fallback;

    static SKColor? ParseColor(string? s, bool defaultBlackIfMissing)
    {
        if (s is null) return defaultBlackIfMissing ? SKColors.Black : null;
        string trimmed = s.Trim();
        if (trimmed.Length == 0 || trimmed.Equals("none", StringComparison.OrdinalIgnoreCase)) return null;
        return ColorParser.TryParse(trimmed, out SKColor c) ? c : SKColors.Black;
    }

    static SKPath ParsePathData(string d)
    {
        SKPath path = new();
        int i = 0;
        char cmd = '\0';
        float curX = 0f, curY = 0f;
        while (i < d.Length)
        {
            char c = d[i];
            if (char.IsWhiteSpace(c) || c == ',') { i++; continue; }
            if (char.IsLetter(c)) { cmd = c; i++; continue; }

            switch (cmd)
            {
                case 'M':
                    curX = ReadNumber(d, ref i);
                    curY = ReadNumber(d, ref i);
                    path.MoveTo(curX, curY);
                    cmd = 'L';
                    break;
                case 'm':
                    curX += ReadNumber(d, ref i);
                    curY += ReadNumber(d, ref i);
                    path.MoveTo(curX, curY);
                    cmd = 'l';
                    break;
                case 'L':
                    curX = ReadNumber(d, ref i);
                    curY = ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'l':
                    curX += ReadNumber(d, ref i);
                    curY += ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'H':
                    curX = ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'h':
                    curX += ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'V':
                    curY = ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'v':
                    curY += ReadNumber(d, ref i);
                    path.LineTo(curX, curY);
                    break;
                case 'Z':
                case 'z':
                    path.Close();
                    cmd = '\0';
                    break;
                default:
                    i++;
                    break;
            }
        }
        return path;
    }

    static float ReadNumber(string s, ref int i)
    {
        while (i < s.Length && (char.IsWhiteSpace(s[i]) || s[i] == ',')) i++;
        int start = i;
        if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
        if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
        {
            i++;
            if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
            while (i < s.Length && char.IsDigit(s[i])) i++;
        }
        return float.Parse(s.AsSpan(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
