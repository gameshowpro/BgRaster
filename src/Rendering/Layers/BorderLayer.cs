// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class BorderLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (!context.Options.Border)
        {
            return;
        }

        int ox = context.CanvasOffsetX;
        int oy = context.CanvasOffsetY;
        int vw = context.ViewportWidth;
        int vh = context.ViewportHeight;

        using SKPaint paint = new()
        {
            Color = context.Options.BorderColor,
            StrokeWidth = 1f,
            IsAntialias = false,
        };

        float right = ox + vw - 1f;
        float bottom = oy + vh - 1f;

        // Top edge
        canvas.DrawLine(ox, oy, right, oy, paint);
        // Bottom edge
        canvas.DrawLine(ox, bottom, right, bottom, paint);
        // Left edge
        canvas.DrawLine(ox, oy, ox, bottom, paint);
        // Right edge
        canvas.DrawLine(right, oy, right, bottom, paint);
    }
}
