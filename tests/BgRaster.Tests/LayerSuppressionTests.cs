// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster.Rendering;
using GameshowPro.BgRaster.Rendering.Layers;

public class LayerSuppressionTests
{
    [Fact]
    public void Circle_IsSuppressed_WhenStrokeIsNonPositive()
    {
        RenderContext context = CreateContext(new ResolvedOptions
        {
            CircleXPx = 64f,
            CircleYPx = 64f,
            CircleSizePx = 64f,
            CircleStrokePx = 0f,
            CircleColor = SKColors.Red,
        });

        bool hasPixels = RenderAndDetectPixels(new CircleLayer(), context);
        hasPixels.Should().BeFalse();
    }

    [Fact]
    public void Circle_IsSuppressed_WhenColorIsTransparent()
    {
        RenderContext context = CreateContext(new ResolvedOptions
        {
            CircleXPx = 64f,
            CircleYPx = 64f,
            CircleSizePx = 64f,
            CircleStrokePx = 2f,
            CircleColor = SKColors.Transparent,
        });

        bool hasPixels = RenderAndDetectPixels(new CircleLayer(), context);
        hasPixels.Should().BeFalse();
    }

    [Fact]
    public void Crosshair_IsSuppressed_WhenColorIsTransparent()
    {
        RenderContext context = CreateContext(new ResolvedOptions
        {
            CrosshairXPx = 64f,
            CrosshairYPx = 64f,
            CrosshairLengthPx = 64f,
            CrosshairStrokePx = 2f,
            CrosshairColor = SKColors.Transparent,
        });

        bool hasPixels = RenderAndDetectPixels(new CrosshairLayer(), context);
        hasPixels.Should().BeFalse();
    }

    [Fact]
    public void Logo_IsSuppressed_WhenOpacityIsNonPositive()
    {
        RenderContext context = CreateContext(new ResolvedOptions
        {
            LogoSource = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg",
            LogoXPx = 64f,
            LogoYPx = 64f,
            LogoWidthPx = 64f,
            LogoHeightPx = 64f,
            LogoOpacity = 0f,
        });

        bool hasPixels = RenderAndDetectPixels(new LogoLayer(), context);
        hasPixels.Should().BeFalse();
    }

    [Fact]
        public void Logo_IsSuppressed_WhenBothDimensionsAreNonPositive()
        {
            RenderContext zeroWidth = CreateContext(new ResolvedOptions
            {
                LogoSource = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg",
                LogoXPx = 64f,
                LogoYPx = 64f,
                LogoWidthPx = 0f,
                LogoHeightPx = 64f,
                LogoOpacity = 1f,
            });
            RenderContext zeroHeight = CreateContext(new ResolvedOptions
            {
                LogoSource = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg",
                LogoXPx = 64f,
                LogoYPx = 64f,
                LogoWidthPx = 64f,
                LogoHeightPx = 0f,
                LogoOpacity = 1f,
            });
            RenderContext bothZero = CreateContext(new ResolvedOptions
            {
                LogoSource = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg",
                LogoXPx = 64f,
                LogoYPx = 64f,
                LogoWidthPx = 0f,
                LogoHeightPx = 0f,
                LogoOpacity = 1f,
            });

            // One dimension zero — still renders (uniform scale from the non-zero dimension)
            RenderAndDetectPixels(new LogoLayer(), zeroWidth).Should().BeTrue();
            RenderAndDetectPixels(new LogoLayer(), zeroHeight).Should().BeTrue();
            // Both dimensions zero — suppressed
            RenderAndDetectPixels(new LogoLayer(), bothZero).Should().BeFalse();
        }

    static RenderContext CreateContext(ResolvedOptions options) =>
        new(
            OutputRecord: new OutputRecord
            {
                Id = "test-output",
                Index = 0,
                WidthPx = 128,
                HeightPx = 128,
            },
            Options: options,
            ViewportWidth: 128,
            ViewportHeight: 128,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

    static bool RenderAndDetectPixels(ILayer layer, RenderContext context)
    {
        SKImageInfo info = new(context.ViewportWidth, context.ViewportHeight);
        using SKSurface surface = SKSurface.Create(info);
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        layer.Render(context, canvas);

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                    return true;
            }
        }

        return false;
    }
}
