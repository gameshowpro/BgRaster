// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class CircleLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (LayerSuppression.ShouldSuppressCircle(context.Options))
        {
            return;
        }

        float cx = context.CanvasOffsetX + context.Options.CircleXPx;
        float cy = context.CanvasOffsetY + context.Options.CircleYPx;
        float radius = context.Options.CircleSizePx / 2f;

        using SKPaint paint = new()
        {
            Color = context.Options.CircleColor,
            IsAntialias = true,
        };

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = context.Options.CircleStrokePx;

        canvas.DrawCircle(cx, cy, radius, paint);
    }
}
