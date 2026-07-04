// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster.Rendering;
using GameshowPro.BgRaster.Rendering.Layers;
using System.IO;

public class LogoLayerTests
{
    [Fact]
    public void IsDarkBackground_UsesRelativeLuminance()
    {
        LogoLayer.IsDarkBackground(SKColors.Black).Should().BeTrue();
        LogoLayer.IsDarkBackground(SKColors.White).Should().BeFalse();
    }

    [Fact]
        public void Render_SvgLightDark_UsesDarkBranchOnDarkBackground()
    {
        string svgPath = Path.Combine(Path.GetTempPath(), $"bgraster-logo-{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(svgPath, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><rect x=\"0\" y=\"0\" width=\"10\" height=\"10\" fill=\"light-dark(#ffffff,#000000)\"/></svg>");

            SKColor pixel = RenderSinglePixel(svgPath, SKColors.Black);
            pixel.Red.Should().Be(0);
            pixel.Green.Should().Be(0);
            pixel.Blue.Should().Be(0);
        }
        finally
        {
            if (File.Exists(svgPath))
                File.Delete(svgPath);
        }
    }

    [Fact]
    public void Render_FileUriSvg_RendersViaSharedSvgPath()
    {
        string svgPath = Path.Combine(Path.GetTempPath(), $"bgraster-logo-{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(svgPath, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><rect x=\"0\" y=\"0\" width=\"10\" height=\"10\" fill=\"#00ff00\"/></svg>");

            SKColor pixel = RenderSinglePixel(new Uri(svgPath).AbsoluteUri, SKColors.White);
            pixel.Green.Should().Be(255);
        }
        finally
        {
            if (File.Exists(svgPath))
                File.Delete(svgPath);
        }
    }

    [Fact]
    public void Render_EmptyLogoSource_SuppressesLogo()
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
                LogoSource = "",
                LogoXPx = 40f,
                LogoYPx = 20f,
                LogoWidthPx = 120f,
                LogoHeightPx = 80f,
                LogoOpacity = 1f,
            },
            ViewportWidth: info.Width,
            ViewportHeight: info.Height,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

        LogoLayer layer = new();
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

        hasNonTransparentPixel.Should().BeFalse("empty logo source should suppress logo rendering entirely");
    }

    [Fact]
    public void Render_PackUriLogo_RendersEmbeddedResource()
    {
        // Arrange: Use the pack URI that points to the embedded gsp.svg resource
        string packUri = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg";
        
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
                LogoSource = packUri,
                LogoXPx = 40f,
                LogoYPx = 20f,
                LogoWidthPx = 120f,
                LogoHeightPx = 80f,
                LogoOpacity = 1f,
            },
            ViewportWidth: info.Width,
            ViewportHeight: info.Height,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

        LogoLayer layer = new();
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

        hasNonTransparentPixel.Should().BeTrue("pack URI should resolve and render the embedded logo");
    }

    static SKColor RenderSinglePixel(string logoSource, SKColor backgroundColor)
    {
        SKImageInfo info = new(64, 64);
        using SKSurface surface = SKSurface.Create(info);
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
                BackgroundColor = backgroundColor,
                LogoSource = logoSource,
                LogoXPx = 32f,
                LogoYPx = 32f,
                LogoWidthPx = 32f,
                LogoHeightPx = 32f,
                LogoOpacity = 1f,
            },
            ViewportWidth: info.Width,
            ViewportHeight: info.Height,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

        LogoLayer layer = new();
        layer.Render(context, canvas);

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        return bitmap.GetPixel(32, 32);
    }
}
