namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class LogoLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (string.IsNullOrEmpty(context.Options.LogoSource)) return;

        float x = context.CanvasOffsetX + context.Options.LogoXPx;
        float y = context.CanvasOffsetY + context.Options.LogoYPx;
        float w = context.Options.LogoWidthPx;
        float h = context.Options.LogoHeightPx;
        if (w <= 0f || h <= 0f) return;

        SKRect fitRect = SKRect.Create(x, y, w, h);
        byte alpha = (byte)(Math.Clamp(context.Options.LogoOpacity, 0f, 1f) * 255f);

        string source = context.Options.LogoSource;
        bool useFallback = false;

        if (source.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using FileStream svgStream = File.OpenRead(source);
                if (!SvgRenderer.TryRender(svgStream, canvas, fitRect, alpha))
                    useFallback = true;
            }
            catch
            {
                useFallback = true;
            }
        }
        else
        {
            SKBitmap? bitmap = null;
            try
            {
                bitmap = SKBitmap.Decode(source);
                if (bitmap is null) useFallback = true;
                else DrawBestFit(canvas, bitmap, fitRect, alpha);
            }
            catch
            {
                useFallback = true;
            }
            finally
            {
                bitmap?.Dispose();
            }
        }

        if (useFallback)
        {
            Console.WriteLine($"LogoLayer: status=logo-fallback-used source=\"{source}\"");
            DrawFallbackLogo(canvas, fitRect, alpha);
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

    static void DrawFallbackLogo(SKCanvas canvas, SKRect rect, byte alpha)
    {
        try
        {
            using Stream? embedded = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("GameshowPro.BgRaster.resources.fallback-logo.svg");
            if (embedded is not null && SvgRenderer.TryRender(embedded, canvas, rect, alpha))
                return;
        }
        catch { }

        SKColor orange = new(255, 136, 0, alpha);
        using SKPaint paint = new()
        {
            Color = orange,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = MathF.Max(2f, rect.Width * 0.04f),
            IsAntialias = true,
        };
        float margin = paint.StrokeWidth;
        SKRect inner = new(rect.Left + margin, rect.Top + margin,
            rect.Right - margin, rect.Bottom - margin);

        canvas.DrawRect(inner, paint);
        canvas.DrawLine(inner.Left, inner.Top, inner.Right, inner.Bottom, paint);
        canvas.DrawLine(inner.Right, inner.Top, inner.Left, inner.Bottom, paint);
    }
}
