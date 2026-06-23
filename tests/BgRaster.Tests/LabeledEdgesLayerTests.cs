// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster.Rendering;
using GameshowPro.BgRaster.Rendering.Layers;

public class LabeledEdgesLayerTests
{
    [Fact]
    public void BuildLabelText_UsesViewportRelativeNumbers()
    {
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.TL, 1920, 1080).Should().Be("0,0");
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.T, 1920, 1080).Should().Be("0");
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.R, 1920, 1080).Should().Be("1920");
    }

    [Fact]
    public void BuildLabelText_UsesInclusiveExtentsForEdges()
    {
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.TR, 1024, 1024).Should().Be("1024,0");
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.BR, 1024, 1024).Should().Be("1024,1024");
        LabeledEdgesLayer.BuildLabelText(LabeledEdgeSide.BL, 1024, 1024).Should().Be("0,1024");
    }

    [Fact]
    public void GetTargetPoint_ResolvesEdgesAndCorners()
    {
        LabeledEdgesLayer.GetTargetPoint(LabeledEdgeSide.TL, 10, 20, 100, 50).Should().Be(new SKPoint(10f, 20f));
        LabeledEdgesLayer.GetTargetPoint(LabeledEdgeSide.T, 10, 20, 100, 50).Should().Be(new SKPoint(60f, 20f));
        LabeledEdgesLayer.GetTargetPoint(LabeledEdgeSide.BR, 10, 20, 100, 50).Should().Be(new SKPoint(109f, 69f));
    }

    [Fact]
    public void Render_DrawsPixelsForConfiguredSides()
    {
        SKImageInfo info = new(200, 120);
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
                LabeledEdgesTextSizePx = 20f,
                LabeledEdgesTailLengthPx = 12f,
                LabeledEdgesThicknessPx = 4f,
                LabeledEdgesHeadScale = 1f,
                LabeledEdgesScope = LabeledEdgesScope.Output,
                LabeledEdgesScopeWidthPx = info.Width,
                LabeledEdgesScopeHeightPx = info.Height,
                LabeledEdgesSides = [LabeledEdgeSide.TL],
            },
            ViewportWidth: info.Width,
            ViewportHeight: info.Height,
            CanvasOffsetX: 0,
            CanvasOffsetY: 0);

        LabeledEdgesLayer layer = new();
        layer.Render(context, canvas);

        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);

        int nonTransparentPixels = 0;
        for (int y = 0; y < 30; y++)
        {
            for (int x = 0; x < 30; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                    nonTransparentPixels++;
            }
        }

        nonTransparentPixels.Should().BeGreaterThan(10);
    }
}