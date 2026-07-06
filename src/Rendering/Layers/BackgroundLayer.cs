// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class BackgroundLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        SKRect viewport = SKRect.Create(context.CanvasOffsetX, context.CanvasOffsetY,
            context.ViewportWidth, context.ViewportHeight);

        using SKPaint fill = new() { Color = context.Options.BackgroundColor };
        canvas.DrawRect(viewport, fill);

        string imagePath = context.Options.BackgroundImage;
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        SKBitmap? bitmap = null;
        try
        {
            bitmap = SKBitmap.Decode(imagePath);
            if (bitmap is null)
            {
                Console.WriteLine($"BackgroundLayer: could not decode image '{imagePath}' - skipping.");
                return;
            }
            DrawFittedImage(canvas, bitmap, viewport, context.Options.BackgroundFit);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BackgroundLayer: error loading image '{imagePath}': {ex.Message} - skipping.");
        }
        finally
        {
            bitmap?.Dispose();
        }
    }

    private static void DrawFittedImage(SKCanvas canvas, SKBitmap bitmap, SKRect viewport, FitMode fit)
    {
        float iw = bitmap.Width;
        float ih = bitmap.Height;
        float vw = viewport.Width;
        float vh = viewport.Height;

        using SKPaint paint = new() { IsAntialias = true };

        _ = canvas.Save();
        canvas.ClipRect(viewport);

        switch (fit)
        {
            case FitMode.BestFit:
                {
                    float scale = MathF.Min(vw / iw, vh / ih);
                    float dw = iw * scale;
                    float dh = ih * scale;
                    float dx = viewport.Left + (vw - dw) / 2f;
                    float dy = viewport.Top + (vh - dh) / 2f;
                    canvas.DrawBitmap(bitmap, SKRect.Create(dx, dy, dw, dh), SKSamplingOptions.Default, paint);
                    break;
                }
            case FitMode.CropToFill:
                {
                    float scale = MathF.Max(vw / iw, vh / ih);
                    float dw = iw * scale;
                    float dh = ih * scale;
                    float dx = viewport.Left + (vw - dw) / 2f;
                    float dy = viewport.Top + (vh - dh) / 2f;
                    canvas.DrawBitmap(bitmap, SKRect.Create(dx, dy, dw, dh), SKSamplingOptions.Default, paint);
                    break;
                }
            case FitMode.CropTL:
                canvas.DrawBitmap(bitmap,
                    SKRect.Create(0, 0, MathF.Min(iw, vw), MathF.Min(ih, vh)),
                    SKRect.Create(viewport.Left, viewport.Top, MathF.Min(iw, vw), MathF.Min(ih, vh)), SKSamplingOptions.Default, paint);
                break;
            case FitMode.CropTR:
                {
                    float cropW = MathF.Min(iw, vw);
                    float cropH = MathF.Min(ih, vh);
                    canvas.DrawBitmap(bitmap,
                        SKRect.Create(iw - cropW, 0, cropW, cropH),
                        SKRect.Create(viewport.Left, viewport.Top, cropW, cropH), SKSamplingOptions.Default, paint);
                    break;
                }
            case FitMode.CropC:
                {
                    float cropW = MathF.Min(iw, vw);
                    float cropH = MathF.Min(ih, vh);
                    canvas.DrawBitmap(bitmap,
                        SKRect.Create((iw - cropW) / 2f, (ih - cropH) / 2f, cropW, cropH),
                        SKRect.Create(viewport.Left, viewport.Top, cropW, cropH), SKSamplingOptions.Default, paint);
                    break;
                }
            case FitMode.CropBL:
                {
                    float cropW = MathF.Min(iw, vw);
                    float cropH = MathF.Min(ih, vh);
                    canvas.DrawBitmap(bitmap,
                        SKRect.Create(0, ih - cropH, cropW, cropH),
                        SKRect.Create(viewport.Left, viewport.Top, cropW, cropH), SKSamplingOptions.Default, paint);
                    break;
                }
            case FitMode.CropBR:
                {
                    float cropW = MathF.Min(iw, vw);
                    float cropH = MathF.Min(ih, vh);
                    canvas.DrawBitmap(bitmap,
                        SKRect.Create(iw - cropW, ih - cropH, cropW, cropH),
                        SKRect.Create(viewport.Left, viewport.Top, cropW, cropH), SKSamplingOptions.Default, paint);
                    break;
                }
        }

        canvas.Restore();
    }
}
