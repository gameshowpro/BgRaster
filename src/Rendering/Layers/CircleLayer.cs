namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class CircleLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.CircleSizePx <= 0f) return;

        float cx = context.CanvasOffsetX + context.ViewportWidth / 2f;
        float cy = context.CanvasOffsetY + context.ViewportHeight / 2f;
        float radius = context.Options.CircleSizePx / 2f;

        using SKPaint paint = new()
        {
            Color = context.Options.CircleColor,
            IsAntialias = true,
        };

        if (context.Options.CircleStrokePx > 0f)
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = context.Options.CircleStrokePx;
        }

        canvas.DrawCircle(cx, cy, radius, paint);
    }
}
