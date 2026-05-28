namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class CrosshairLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.CrosshairLengthPx <= 0f || context.Options.CrosshairStrokePx <= 0f) return;

        float cx = context.CanvasOffsetX + context.ViewportWidth / 2f;
        float cy = context.CanvasOffsetY + context.ViewportHeight / 2f;
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
