// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class LogoLayer : ILayer
{
    const string DefaultLogoResourceSuffix = "BgRaster.svg";

    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (LayerSuppression.ShouldSuppressLogo(context.Options))
            return;

        float anchorX = TextLayer.ParseAnchorX(context.Options.LogoAnchorX);
        float anchorY = TextLayer.ParseAnchorY(context.Options.LogoAnchorY);
        float cx = context.CanvasOffsetX + context.Options.LogoXPx;
        float cy = context.CanvasOffsetY + context.Options.LogoYPx;
        float targetWidth = context.Options.LogoWidthPx;
        float targetHeight = context.Options.LogoHeightPx;

        string source = context.Options.LogoSource;
        byte alpha = (byte)(Math.Clamp(context.Options.LogoOpacity, 0f, 1f) * 255f);
        bool useDarkTheme = IsDarkBackground(context.Options.BackgroundColor);

        if (!string.IsNullOrEmpty(source) && TryRenderSvgLogo(source, canvas, cx, cy, targetWidth, targetHeight, anchorX, anchorY, alpha, useDarkTheme))
            return;

        if (!string.IsNullOrEmpty(source) && TryRenderBitmapLogo(source, canvas, cx, cy, targetWidth, targetHeight, anchorX, anchorY, alpha))
            return;

        Console.WriteLine($"LogoLayer: status=logo-fallback-used source=\"{source}\"");
        RenderDefaultSvgLogo(canvas, cx, cy, targetWidth, targetHeight, anchorX, anchorY, alpha, useDarkTheme);
    }

    static bool TryRenderSvgLogo(string source, SKCanvas canvas, float cx, float cy, float targetWidth, float targetHeight, float anchorX, float anchorY, byte alpha, bool _)
    {
        Stream? svgStream = null;
        try
        {
            if (!TryOpenSvgLogoStream(source, out svgStream) || svgStream is null)
                return false;

            SKSvg svg = new();
            svg.Load(svgStream);
            if (svg.Picture is null) return false;

            SKRect svgBounds = svg.Picture.CullRect;
            if (svgBounds.Width <= 0 || svgBounds.Height <= 0) return false;

            (float renderW, float renderH, float scaleX, float scaleY, float offsetX, float offsetY) =
                ComputeRender(targetWidth, targetHeight, svgBounds.Width, svgBounds.Height, anchorX, anchorY);

            canvas.Save();
            canvas.Translate(cx + offsetX, cy + offsetY);
            canvas.Scale(scaleX, scaleY);

            if (alpha < 255)
            {
                using SKPaint layerPaint = new() { Color = SKColors.White.WithAlpha(alpha) };
                canvas.SaveLayer(layerPaint);
                canvas.DrawPicture(svg.Picture);
                canvas.Restore();
            }
            else
            {
                canvas.DrawPicture(svg.Picture);
            }

            canvas.Restore();
            return true;
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

    static bool TryRenderBitmapLogo(string source, SKCanvas canvas, float cx, float cy, float targetWidth, float targetHeight, float anchorX, float anchorY, byte alpha)
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
            if (bitmap is null) return false;

            (float renderW, float renderH, float scaleX, float scaleY, float offsetX, float offsetY) =
                ComputeRender(targetWidth, targetHeight, bitmap.Width, bitmap.Height, anchorX, anchorY);

            SKRect destRect = SKRect.Create(cx + offsetX, cy + offsetY, renderW, renderH);
            using SKPaint paint = new() { IsAntialias = true };
            paint.Color = paint.Color.WithAlpha(alpha);
            canvas.DrawBitmap(bitmap, destRect, SKSamplingOptions.Default, paint);
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

    #pragma warning disable IDE0060 // cx, cy, dark kept for API compatibility
    static void RenderDefaultSvgLogo(SKCanvas canvas, float _cx, float _cy, float targetWidth, float targetHeight, float anchorX, float anchorY, byte alpha, bool _dark)
#pragma warning restore IDE0060
    {
        using Stream svgStream = OpenEmbeddedDefaultLogoStream();
        SKSvg svg = new();
        svg.Load(svgStream);
        if (svg.Picture is null)
            throw new InvalidOperationException("Embedded default logo SVG failed to load.");

        SKRect svgBounds = svg.Picture.CullRect;
        if (svgBounds.Width <= 0 || svgBounds.Height <= 0)
            throw new InvalidOperationException("Embedded default logo SVG has zero-size bounds.");

        (float renderW, float renderH, float scaleX, float scaleY, float offsetX, float offsetY) =
            ComputeRender(targetWidth, targetHeight, svgBounds.Width, svgBounds.Height, anchorX, anchorY);

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scaleX, scaleY);

        if (alpha < 255)
        {
            using SKPaint layerPaint = new() { Color = SKColors.White.WithAlpha(alpha) };
            canvas.SaveLayer(layerPaint);
            canvas.DrawPicture(svg.Picture);
            canvas.Restore();
        }
        else
        {
            canvas.DrawPicture(svg.Picture);
        }

        canvas.Restore();
    }

    /// <summary>
    /// Computes render dimensions and transforms.
    /// - If both targetWidth and targetHeight are set (>0): best-fit inside the rectangle.
    /// - If only one is set: the other is computed from the source's natural aspect ratio.
    /// - If neither is set: source renders at its natural pixel size.
    /// </summary>
    static (float renderW, float renderH, float scaleX, float scaleY, float offsetX, float offsetY)
        ComputeRender(float targetWidth, float targetHeight, float naturalW, float naturalH, float anchorX, float anchorY)
    {
        float scaleX, scaleY, renderW, renderH;

        if (targetWidth > 0 && targetHeight > 0)
        {
            // Both set: best-fit inside the rectangle
            float scale = MathF.Min(targetWidth / naturalW, targetHeight / naturalH);
            scaleX = scale;
            scaleY = scale;
            renderW = naturalW * scale;
            renderH = naturalH * scale;
        }
        else if (targetWidth > 0)
        {
            // Width only: scale proportionally
            scaleX = targetWidth / naturalW;
            scaleY = scaleX;
            renderW = targetWidth;
            renderH = naturalH * scaleY;
        }
        else if (targetHeight > 0)
        {
            // Height only: scale proportionally
            scaleY = targetHeight / naturalH;
            scaleX = scaleY;
            renderW = naturalW * scaleX;
            renderH = targetHeight;
        }
        else
        {
            // Neither set: natural size
            scaleX = 1f;
            scaleY = 1f;
            renderW = naturalW;
            renderH = naturalH;
        }

        // Position anchored rect
        float offsetX = -(renderW * anchorX);
        float offsetY = -(renderH * anchorY);

        return (renderW, renderH, scaleX, scaleY, offsetX, offsetY);
    }

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

    static bool TryOpenSvgLogoStream(string source, out Stream? svgStream)
    {
        svgStream = null;

        if (source.StartsWith("pack://application:,,,/", StringComparison.OrdinalIgnoreCase))
        {
            string resourcePath = ExtractPackUriResourcePath(source);
            if (!string.IsNullOrEmpty(resourcePath))
            {
                try
                {
                    return TryOpenEmbeddedResource(resourcePath, out svgStream);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        if (!TryResolveSvgPath(source, out string? svgPath) || svgPath is null)
            return false;

        svgStream = File.OpenRead(svgPath);
        return true;
    }

    static string ExtractPackUriResourcePath(string packUri)
    {
        int componentIndex = packUri.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
        if (componentIndex < 0)
            return string.Empty;

        string resourcePath = packUri[(componentIndex + ";component/".Length)..];
        int lastSlash = resourcePath.LastIndexOf('/');
        return lastSlash >= 0 ? resourcePath[(lastSlash + 1)..] : resourcePath;
    }

    static bool TryOpenEmbeddedResource(string resourceSuffix, out Stream? resourceStream)
    {
        resourceStream = null;
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? match = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            return false;
        resourceStream = assembly.GetManifestResourceStream(match);
        return resourceStream is not null;
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
            .FirstOrDefault(name => name.EndsWith(DefaultLogoResourceSuffix, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Embedded default logo resource matching '*{DefaultLogoResourceSuffix}' was not found.");

        Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
        return resourceStream ?? throw new InvalidOperationException($"Embedded default logo resource '{resourceName}' could not be opened.");
    }
}