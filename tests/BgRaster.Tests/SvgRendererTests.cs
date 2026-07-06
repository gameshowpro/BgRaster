// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using System.Text;

public class SvgRendererTests
{
    [Fact]
    public void TryRender_PathWithCubicCurve_RendersSuccessfully()
    {
        const string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><path d=\"M1 9 C1 1 9 1 9 9 L1 9 Z\" fill=\"#ff0000\"/></svg>";

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(svg));
        using SKSurface surface = SKSurface.Create(new SKImageInfo(32, 32));
        surface.Canvas.Clear(SKColors.Transparent);

        bool rendered = SvgRenderer.TryRender(stream, surface.Canvas, SKRect.Create(0, 0, 32, 32), alpha: 255, false);

        rendered.Should().BeTrue();

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        SKColor pixel = bitmap.GetPixel(16, 22);
        pixel.Alpha.Should().BeGreaterThan((byte)0);
        pixel.Red.Should().BeGreaterThan((byte)0);
    }

    static SKColor RenderAndSampleCenter(string svg, bool useDarkTheme)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(svg));
        using SKSurface surface = SKSurface.Create(new SKImageInfo(32, 32));
        surface.Canvas.Clear(SKColors.Transparent);

        bool rendered = SvgRenderer.TryRender(stream, surface.Canvas, SKRect.Create(0, 0, 32, 32), 255, useDarkTheme);
        rendered.Should().BeTrue();

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        return bitmap.GetPixel(16, 16);
    }
}