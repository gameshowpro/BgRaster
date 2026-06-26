// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

sealed class TextLayer : ILayer
{
    internal const float DefaultLineHeightRatio = 1.2f;
    internal const float DefaultCollisionGapPx = 1f;

    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.TextLines.Length == 0)
            return;

        float cx = context.CanvasOffsetX + context.Options.TextXPx;
        float cy = context.CanvasOffsetY + context.Options.TextYPx;

        ImmutableArray<(string Text, float SizePx, SKColor Color)> lines =
        [
            .. context.Options.TextLines
                .Select((line, index) => (
                    Text: line,
                    SizePx: context.Options.TextSizesPx[index % context.Options.TextSizesPx.Length],
                    Color: context.Options.TextColors[index % context.Options.TextColors.Length]))
                .Where(line => !string.IsNullOrWhiteSpace(line.Text) && line.SizePx > 0f)
        ];

        if (lines.Length == 0)
            return;

        ImmutableArray<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx)>.Builder metricsBuilder =
            ImmutableArray.CreateBuilder<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx)>(lines.Length);
        foreach ((string text, float sizePx, SKColor color) in lines)
        {
            using SKFont metricsFont = new(FontManager.Typeface, sizePx);
            SKFontMetrics metrics = metricsFont.Metrics;
            metricsBuilder.Add((text, sizePx, color, metrics.Ascent, metrics.Descent));
        }

        ImmutableArray<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx)> measuredLines =
            metricsBuilder.ToImmutable();

        ImmutableArray<float>.Builder baselineOffsetsBuilder = ImmutableArray.CreateBuilder<float>(measuredLines.Length);
        baselineOffsetsBuilder.Add(0f);
        for (int index = 1; index < measuredLines.Length; index++)
        {
            (string _, float topSizePx, SKColor _, float _, float topDescentPx) = measuredLines[index - 1];
            (string _, float bottomSizePx, SKColor _, float bottomAscentPx, float _) = measuredLines[index];

            float advance = ComputeBaselineAdvance(
                topSizePx,
                bottomSizePx,
                topDescentPx,
                bottomAscentPx,
                DefaultLineHeightRatio,
                DefaultCollisionGapPx);
            baselineOffsetsBuilder.Add(baselineOffsetsBuilder[index - 1] + advance);
        }

        ImmutableArray<float> baselineOffsets = baselineOffsetsBuilder.ToImmutable();

        float blockTop = float.PositiveInfinity;
        float blockBottom = float.NegativeInfinity;
        for (int index = 0; index < measuredLines.Length; index++)
        {
            (string _, float _, SKColor _, float ascentPx, float descentPx) = measuredLines[index];
            float baseline = baselineOffsets[index];

            blockTop = Math.Min(blockTop, baseline + ascentPx);
            blockBottom = Math.Max(blockBottom, baseline + descentPx);
        }

        float baselineShift = cy - ((blockTop + blockBottom) / 2f);

        for (int index = 0; index < measuredLines.Length; index++)
        {
            (string text, float sizePx, SKColor color, float _, float _) = measuredLines[index];
            float y = baselineOffsets[index] + baselineShift;

            using SKFont font = new(FontManager.Typeface, sizePx);
            using SKPaint paint = new()
            {
                Color = color,
                IsAntialias = true,
            };

            canvas.DrawText(text, cx, y, SKTextAlign.Center, font, paint);
        }
    }

    internal static float ComputeBaselineAdvance(
        float topSizePx,
        float bottomSizePx,
        float topDescentPx,
        float bottomAscentPx,
        float lineHeightRatio,
        float collisionGapPx)
    {
        float opticalAdvance = lineHeightRatio * ((topSizePx + bottomSizePx) / 2f);
        float collisionAdvance = topDescentPx + (-bottomAscentPx) + collisionGapPx;
        return Math.Max(opticalAdvance, collisionAdvance);
    }
}
