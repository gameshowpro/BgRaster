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

        float centerX = context.CanvasOffsetX + context.Options.LogoXPx;
        float centerY = context.CanvasOffsetY + context.Options.LogoYPx;
        float w = context.Options.LogoWidthPx;
        float h = context.Options.LogoHeightPx;

        SKRect fitRect = CreateFitRect(centerX, centerY, w, h);
        byte alpha = (byte)(Math.Clamp(context.Options.LogoOpacity, 0f, 1f) * 255f);
        bool useDarkTheme = IsDarkBackground(context.Options.BackgroundColor);

        string source = context.Options.LogoSource;

        // Try to render SVG first (includes pack URIs and file paths)
        if (TryRenderSvgLogo(source, canvas, fitRect, alpha, useDarkTheme))
            return;

        // Try bitmap second
        if (TryRenderBitmapLogo(source, canvas, fitRect, alpha))
            return;

        // Fallback to embedded logo on failure
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

        // Handle pack URIs: extract the resource path and load via assembly
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

        // Handle file paths
        if (!TryResolveSvgPath(source, out string? svgPath) || svgPath is null)
            return false;

        svgStream = File.OpenRead(svgPath);
        return true;
    }

    static string ExtractPackUriResourcePath(string packUri)
    {
        // Format: pack://application:,,,/AssemblyName;component/path/to/resource
        int componentIndex = packUri.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
        if (componentIndex < 0)
            return string.Empty;

        string resourcePath = packUri[(componentIndex + ";component/".Length)..];
        // Convert forward slashes to dots for manifest resource name matching,
        // but drop any leading segments — we'll match by suffix.
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

        using SKPaint paint = new() { IsAntialias = true };
        paint.Color = paint.Color.WithAlpha(alpha);
        canvas.DrawBitmap(bitmap, SKRect.Create(dx, dy, dw, dh), SKSamplingOptions.Default, paint);
    }

}
