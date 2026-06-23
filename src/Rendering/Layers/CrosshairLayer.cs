// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class CrosshairLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (LayerSuppression.ShouldSuppressCrosshair(context.Options))
            return;

        float cx = context.CanvasOffsetX + context.Options.CrosshairXPx;
        float cy = context.CanvasOffsetY + context.Options.CrosshairYPx;
        float halfLen = context.Options.CrosshairLengthPx / 2f;

        using SKPaint paint = new()
        {
            Color = context.Options.CrosshairColor,
            StrokeWidth = context.Options.CrosshairStrokePx,
            IsAntialias = true,
        };

        canvas.DrawLine(cx - halfLen, cy, cx + halfLen, cy, paint);
        canvas.DrawLine(cx, cy - halfLen, cx, cy + halfLen, paint);
    }
}
