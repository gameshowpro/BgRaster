namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class AlternatingLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (!context.Options.Alternating) return;

        int ox = context.CanvasOffsetX;
        int oy = context.CanvasOffsetY;
        int vw = context.ViewportWidth;
        int vh = context.ViewportHeight;

        SKColor bgColor = context.Options.BackgroundColor;

        using SKBitmap layer = new(vw, vh, SKColorType.Bgra8888, SKAlphaType.Premul);
        unsafe
        {
            uint* ptr = (uint*)layer.GetPixels();
            for (int y = 0; y < vh; y++)
            {
                for (int x = 0; x < vw; x++)
                {
                    // SKColorType.Bgra8888 stores as BGRA in memory, but we pack as ARGB above.
                    // Use SKColor to get correct BGRA packing.
                    ptr[y * vw + x] = (x + y) % 2 == 0
                        ? PackBgra(bgColor)
                        : 0xFF000000u;
                }
            }
        }

        canvas.DrawBitmap(layer, ox, oy);
    }

    static unsafe uint PackBgra(SKColor c) =>
        ((uint)c.Alpha << 24) | ((uint)c.Red << 16) | ((uint)c.Green << 8) | c.Blue;
}
