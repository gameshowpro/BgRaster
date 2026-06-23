// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

using GameshowPro.BgRaster.Rendering.Layers;
using GameshowPro.BgRaster.Resolution;

sealed class OutputRenderer
{
    int _sliceSequence;

    readonly BackgroundLayer _background = new();
    readonly GridLayer _grid = new();
    readonly AlternatingLayer _alternating = new();
    readonly BorderLayer _border = new();
    readonly CircleLayer _circle = new();
    readonly CrosshairLayer _crosshair = new();
    readonly LabeledEdgesLayer _labeledEdges = new();
    readonly LogoLayer _logo = new();
    readonly TextLayer _text = new();

    internal async Task<RenderOutcome> RenderOutputAsync(
        OutputRecord output, OutputOptions? outputConfig,
        GlobalOptions globalOptions, string outputFilePath,
        int systemWidthPx, int systemHeightPx)
    {
        using SKBitmap bitmap = new(output.WidthPx, output.HeightPx, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(SKColors.Black);

        ImmutableArray<SliceStatus>.Builder sliceStatuses = ImmutableArray.CreateBuilder<SliceStatus>();
        bool isImplicitSliceSet = outputConfig is null || outputConfig.Slices.IsEmpty;

        ImmutableArray<SliceOptions> effectiveSlices = GetEffectiveSlices(outputConfig);
        for (int sliceIndex = 0; sliceIndex < effectiveSlices.Length; sliceIndex++)
        {
            SliceOptions slice = effectiveSlices[sliceIndex];
            if (!TryResolveSliceGeometry(output, slice, out int sx, out int sy, out int sw, out int sh, out string? oobReason))
            {
                sliceStatuses.Add(new SliceStatus { Status = "slice-out-of-bounds", Reason = oobReason });
                continue;
            }

            int sequenceIndex = _sliceSequence;
            _sliceSequence++;

            ResolvedOptions options = OptionsResolver.ResolveForSlice(
                globalOptions, output, outputConfig, slice, sw, sh, sequenceIndex, sliceIndex, isImplicitSliceSet, systemWidthPx, systemHeightPx);
            RenderContext ctx = new(output, options, sw, sh, sx, sy);

            canvas.Save();
            canvas.ClipRect(SKRect.Create(sx, sy, sw, sh));
            RenderLayers(ctx, canvas);
            canvas.Restore();

            sliceStatuses.Add(new SliceStatus { Status = "slice-rendered" });
        }

        using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        await using FileStream fs = File.OpenWrite(outputFilePath);
        data.SaveTo(fs);

        return new RenderOutcome(outputFilePath, sliceStatuses.ToImmutable());
    }

    void RenderLayers(RenderContext ctx, SKCanvas canvas)
    {
        _background.Render(ctx, canvas);
        _grid.Render(ctx, canvas);
        _alternating.Render(ctx, canvas);
        _border.Render(ctx, canvas);
        _circle.Render(ctx, canvas);
        _crosshair.Render(ctx, canvas);
        _labeledEdges.Render(ctx, canvas);
        _logo.Render(ctx, canvas);
        _text.Render(ctx, canvas);
    }

    static bool TryResolveSliceGeometry(OutputRecord output, SliceOptions slice,
        out int sx, out int sy, out int sw, out int sh, out string? outOfBoundsReason)
    {
        outOfBoundsReason = null;
        float vw = output.WidthPx;
        float vh = output.HeightPx;

        try
        {
            sx = (int)UnitParser.Parse(slice.X).ResolvePixels(vw, vh);
            sy = (int)UnitParser.Parse(slice.Y).ResolvePixels(vw, vh);
            sw = (int)UnitParser.Parse(slice.Width).ResolvePixels(vw, vh);
            sh = (int)UnitParser.Parse(slice.Height).ResolvePixels(vw, vh);
        }
        catch
        {
            sx = sy = sw = sh = 0;
            outOfBoundsReason = "slice geometry parse error";
            Console.WriteLine("OutputRenderer: slice geometry parse error — skipping slice.");
            return false;
        }

        if (sx < 0 || sy < 0 || sw <= 0 || sh <= 0 ||
            sx + sw > output.WidthPx || sy + sh > output.HeightPx)
        {
            outOfBoundsReason = $"slice rect (x={sx},y={sy},w={sw},h={sh}) exceeds output bounds ({output.WidthPx}x{output.HeightPx})";
            Console.WriteLine($"# bg-raster: status=slice-out-of-bounds reason=\"{outOfBoundsReason}\"");
            return false;
        }

        return true;
    }

    static ImmutableArray<SliceOptions> GetEffectiveSlices(OutputOptions? outputConfig)
    {
        if (outputConfig is null || outputConfig.Slices.IsEmpty)
            return [new SliceOptions()];

        return outputConfig.Slices;
    }
}

sealed record RenderOutcome(string FilePath, ImmutableArray<SliceStatus> SliceStatuses);
