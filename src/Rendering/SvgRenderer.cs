// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

static class SvgRenderer
{
    internal static bool TryRender(Stream svgStream, SKCanvas canvas, SKRect fitRect, byte alpha, bool useDarkTheme)
    {
        SKSvg? svg;
        try
        {
            svg = new SKSvg();
            svg.Load(svgStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SvgRenderer: status=svg-parse-failed reason=\"{ex.Message}\"");
            return false;
        }
        if (svg is null || svg.Picture is null) return false;

        SKRect svgBounds = svg.Picture.CullRect;
        if (svgBounds.Width <= 0 || svgBounds.Height <= 0) return false;

        float scale = MathF.Min(fitRect.Width / svgBounds.Width, fitRect.Height / svgBounds.Height);
        float dw = svgBounds.Width * scale;
        float dh = svgBounds.Height * scale;
        float dx = fitRect.Left + (fitRect.Width - dw) / 2f;
        float dy = fitRect.Top + (fitRect.Height - dh) / 2f;

        canvas.Save();
        canvas.Translate(dx, dy);
        canvas.Scale(scale);

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
}