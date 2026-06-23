// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster.Rendering;
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

        bool rendered = SvgRenderer.TryRender(stream, surface.Canvas, SKRect.Create(0, 0, 32, 32), alpha: 255, useDarkTheme: false);

        rendered.Should().BeTrue();

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        SKColor pixel = bitmap.GetPixel(16, 22);
        pixel.Alpha.Should().BeGreaterThan((byte)0);
        pixel.Red.Should().BeGreaterThan((byte)0);
    }

    [Fact]
    public void TryRender_GroupInheritedLightDarkFill_UsesThemeBranch()
    {
        const string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><g fill=\"light-dark(#ffffff,#000000)\"><path d=\"M0 0 L10 0 L10 10 L0 10 Z\"/></g></svg>";

        SKColor lightPixel = RenderAndSampleCenter(svg, useDarkTheme: false);
        SKColor darkPixel = RenderAndSampleCenter(svg, useDarkTheme: true);

        lightPixel.Red.Should().Be(255);
        lightPixel.Green.Should().Be(255);
        lightPixel.Blue.Should().Be(255);

        darkPixel.Red.Should().Be(0);
        darkPixel.Green.Should().Be(0);
        darkPixel.Blue.Should().Be(0);
    }

        [Fact]
        public void TryRender_ClassStyleMediaOverride_UsesLightAndDarkThemeBranches()
        {
                const string svg = """
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10">
                            <defs>
                                <style>
                                    .color { fill: #000; }
                                    @media (prefers-color-scheme: dark) {
                                        .color { fill: #fff; }
                                    }
                                </style>
                            </defs>
                            <g class="color">
                                <path d="M0 0 L10 0 L10 10 L0 10 Z" />
                            </g>
                        </svg>
                        """;

                SKColor lightPixel = RenderAndSampleCenter(svg, useDarkTheme: false);
                SKColor darkPixel = RenderAndSampleCenter(svg, useDarkTheme: true);

                lightPixel.Red.Should().Be(0);
                lightPixel.Green.Should().Be(0);
                lightPixel.Blue.Should().Be(0);
                lightPixel.Alpha.Should().BeGreaterThan((byte)0);

                darkPixel.Red.Should().Be(255);
                darkPixel.Green.Should().Be(255);
                darkPixel.Blue.Should().Be(255);
                darkPixel.Alpha.Should().BeGreaterThan((byte)0);
        }

    static SKColor RenderAndSampleCenter(string svg, bool useDarkTheme)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(svg));
        using SKSurface surface = SKSurface.Create(new SKImageInfo(32, 32));
        surface.Canvas.Clear(SKColors.Transparent);

        bool rendered = SvgRenderer.TryRender(stream, surface.Canvas, SKRect.Create(0, 0, 32, 32), alpha: 255, useDarkTheme);
        rendered.Should().BeTrue();

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        return bitmap.GetPixel(16, 16);
    }
}