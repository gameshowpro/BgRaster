namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class TextLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.TextSizePx <= 0f) return;

        float cx = context.CanvasOffsetX + context.Options.TextXPx;
        float cy = context.CanvasOffsetY + context.Options.TextYPx;

        using SKFont font = new(FontManager.Typeface, context.Options.TextSizePx);
        using SKPaint paint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
        };

        string title = context.Options.Title;
        string subtitle = context.Options.Subtitle;

        float lineHeight = context.Options.TextSizePx * 1.2f;
        float totalHeight = (string.IsNullOrEmpty(subtitle) ? 1 : 2) * lineHeight;
        float startY = cy - totalHeight / 2f + lineHeight;

        if (!string.IsNullOrEmpty(title))
            canvas.DrawText(title, cx, startY, font, paint);

        if (!string.IsNullOrEmpty(subtitle))
            canvas.DrawText(subtitle, cx, startY + lineHeight, font, paint);
    }
}
