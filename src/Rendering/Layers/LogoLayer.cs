namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class LogoLayer : ILayer
{
    const string DefaultLogoResourceSuffix = "gsp.svg";

    public void Render(RenderContext context, SKCanvas canvas)
    {
        float centerX = context.CanvasOffsetX + context.Options.LogoXPx;
        float centerY = context.CanvasOffsetY + context.Options.LogoYPx;
        float w = context.Options.LogoWidthPx;
        float h = context.Options.LogoHeightPx;
        if (w <= 0f || h <= 0f) return;

        SKRect fitRect = CreateFitRect(centerX, centerY, w, h);
        byte alpha = (byte)(Math.Clamp(context.Options.LogoOpacity, 0f, 1f) * 255f);
        bool useDarkTheme = IsDarkBackground(context.Options.BackgroundColor);

        string source = context.Options.LogoSource;

        if (string.IsNullOrWhiteSpace(source))
        {
            RenderDefaultSvgLogo(canvas, fitRect, alpha, useDarkTheme);
            return;
        }

        if (TryRenderSvgLogo(source, canvas, fitRect, alpha, useDarkTheme))
            return;

        if (!string.IsNullOrWhiteSpace(source) && TryRenderBitmapLogo(source, canvas, fitRect, alpha))
            return;

        Console.WriteLine($"LogoLayer: status=logo-fallback-used source=\"{source}\"");
        RenderDefaultSvgLogo(canvas, fitRect, alpha, useDarkTheme);
    }

    internal static SKRect CreateFitRect(float centerX, float centerY, float width, float height) =>
        SKRect.Create(centerX - (width / 2f), centerY - (height / 2f), width, height);

    internal static bool IsDarkBackground(SKColor color)
    {
        static float ChannelToLinear(byte channel)
        {
            float normalized = channel / 255f;
            return normalized <= 0.04045f
                ? normalized / 12.92f
                : MathF.Pow((normalized + 0.055f) / 1.055f, 2.4f);
        }

        float r = ChannelToLinear(color.Red);
        float g = ChannelToLinear(color.Green);
        float b = ChannelToLinear(color.Blue);

        float relativeLuminance = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
        return relativeLuminance < 0.5f;
    }

    static bool TryRenderSvgLogo(string source, SKCanvas canvas, SKRect fitRect, byte alpha, bool useDarkTheme)
    {
        Stream? svgStream = null;
        try
        {
            if (!TryOpenSvgLogoStream(source, out svgStream) || svgStream is null)
                return false;

            return SvgRenderer.TryRender(svgStream, canvas, fitRect, alpha, useDarkTheme);
        }
        catch
        {
            return false;
        }
        finally
        {
            svgStream?.Dispose();
        }
    }

    static void RenderDefaultSvgLogo(SKCanvas canvas, SKRect fitRect, byte alpha, bool useDarkTheme)
    {
        using Stream svgStream = OpenEmbeddedDefaultLogoStream();
        if (!SvgRenderer.TryRender(svgStream, canvas, fitRect, alpha, useDarkTheme))
            throw new InvalidOperationException("Embedded default logo SVG failed to render.");
    }

    static bool TryOpenSvgLogoStream(string source, out Stream? svgStream)
    {
        svgStream = null;

        if (string.IsNullOrWhiteSpace(source))
        {
            svgStream = OpenEmbeddedDefaultLogoStream();
            return svgStream is not null;
        }

        if (!TryResolveSvgPath(source, out string? svgPath) || svgPath is null)
            return false;

        svgStream = File.OpenRead(svgPath);
        return true;
    }

    static bool TryResolveSvgPath(string source, out string? svgPath)
    {
        svgPath = null;

        if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri))
        {
            if (!uri.IsFile || !uri.LocalPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                return false;

            svgPath = uri.LocalPath;
            return true;
        }

        if (!source.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            return false;

        svgPath = source;
        return true;
    }

    static Stream OpenEmbeddedDefaultLogoStream()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(DefaultLogoResourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new InvalidOperationException($"Embedded default logo resource matching '*{DefaultLogoResourceSuffix}' was not found.");

        Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
        return resourceStream ?? throw new InvalidOperationException($"Embedded default logo resource '{resourceName}' could not be opened.");
    }

    static bool TryRenderBitmapLogo(string source, SKCanvas canvas, SKRect fitRect, byte alpha)
    {
        SKBitmap? bitmap = null;
        try
        {
            string bitmapSource = source;
            if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri))
            {
                if (!uri.IsFile)
                    return false;

                bitmapSource = uri.LocalPath;
            }

            bitmap = SKBitmap.Decode(bitmapSource);
            if (bitmap is null)
                return false;

            DrawBestFit(canvas, bitmap, fitRect, alpha);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            bitmap?.Dispose();
        }
    }

    static void DrawBestFit(SKCanvas canvas, SKBitmap bitmap, SKRect fitRect, byte alpha)
    {
        float scale = MathF.Min(fitRect.Width / bitmap.Width, fitRect.Height / bitmap.Height);
        float dw = bitmap.Width * scale;
        float dh = bitmap.Height * scale;
        float dx = fitRect.Left + (fitRect.Width - dw) / 2f;
        float dy = fitRect.Top + (fitRect.Height - dh) / 2f;

        using SKPaint paint = new() { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        paint.Color = paint.Color.WithAlpha(alpha);
        canvas.DrawBitmap(bitmap, SKRect.Create(dx, dy, dw, dh), paint);
    }

}
