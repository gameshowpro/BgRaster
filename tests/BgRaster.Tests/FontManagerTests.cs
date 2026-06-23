// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster.Rendering;
using GameshowPro.BgRaster.Rendering.Layers;

public class FontManagerTests
{
    [Fact]
    public void Typeface_LoadsEmbeddedFont()
    {
        SKTypeface typeface = FontManager.Typeface;

        typeface.Should().NotBeNull();
        typeface.FamilyName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TextLayer_Render_UsesEmbeddedTypefaceAndProducesPixels()
    {
        SKImageInfo info = new(240, 120);
        using SKSurface surface = SKSurface.Create(info);
        surface.Should().NotBeNull();

        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        RenderContext context = new(
            OutputRecord: new OutputRecord
            {
                Id = "test-output",
                Index = 0,
                WidthPx = info.Width,
                HeightPx = info.Height,
            },
            Options: new ResolvedOptions
            {
                TextLines = ["Embedded Font", "Smoke Test"],
                TextSizesPx = [28f, 22f],
                TextColors = [SKColors.White],
                TextXPx = info.Width / 2f,
                TextYPx = info.Height / 2f,
            },
            ViewportWidth: info.Width,
            ViewportHeight: info.Height,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

        TextLayer layer = new();
        layer.Render(context, canvas);

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);

        bool hasNonTransparentPixel = false;
        for (int y = 0; y < bitmap.Height && !hasNonTransparentPixel; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                {
                    hasNonTransparentPixel = true;
                    break;
                }
            }
        }

        hasNonTransparentPixel.Should().BeTrue("text rendering should draw visible pixels with the embedded font");
    }
}
