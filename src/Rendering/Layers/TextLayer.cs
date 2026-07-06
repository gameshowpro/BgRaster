// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class TextLayer : ILayer
{
    internal const float DefaultLineHeightRatio = 1.2f;
    internal const float DefaultCollisionGapPx = 1f;

    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.TextLines.Length == 0)
        {
            return;
        }

        float cx = context.CanvasOffsetX + context.Options.TextXPx;
        float cy = context.CanvasOffsetY + context.Options.TextYPx;
        SKTextAlign align = ParseTextAlign(context.Options.TextTextAlign);
        float anchorX = ParseAnchorX(context.Options.TextAnchorX);
        float anchorY = ParseAnchorY(context.Options.TextAnchorY);

        const string networkMarker = "\0NETWORK\0";

        ImmutableArray<(string Text, float SizePx, SKColor Color)>.Builder linesBuilder =
            ImmutableArray.CreateBuilder<(string Text, float SizePx, SKColor Color)>();

        int networkLineIndex = 0;

        for (int i = 0; i < context.Options.TextLines.Length; i++)
        {
            string originalLine = context.Options.TextLines[i];
            float size = context.Options.TextSizesPx[i % context.Options.TextSizesPx.Length];
            SKColor color = context.Options.TextColors[i % context.Options.TextColors.Length];

            if (size > 0f)
            {
                string[] subLines = originalLine.Split(["\n", "<br>"], StringSplitOptions.None);
                for (int j = 0; j < subLines.Length; j++)
                {
                    string subLine = subLines[j];
                    // Don't add the trailing empty string if the original text ends with a newline
                    if (j == subLines.Length - 1 && string.IsNullOrEmpty(subLine) && j > 0)
                    {
                        continue;
                    }

                    if (subLine.Contains(networkMarker))
                    {
                        string[] parts = subLine.Split([networkMarker], 2, StringSplitOptions.None);
                        if (!string.IsNullOrEmpty(parts[0]))
                        {
                            linesBuilder.Add((parts[0], size, color));
                        }

                        NetworkLayer.BuildNetworkLines(context.Options, ref networkLineIndex, linesBuilder);

                        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                        {
                            linesBuilder.Add((parts[1], size, color));
                        }
                    }
                    else
                    {
                        linesBuilder.Add((subLine, size, color));
                    }
                }
            }
        }

        ImmutableArray<(string Text, float SizePx, SKColor Color)> lines = linesBuilder.ToImmutable();

        if (lines.Length == 0)
        {
            return;
        }

        ImmutableArray<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx, float WidthPx)>.Builder metricsBuilder =
            ImmutableArray.CreateBuilder<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx, float WidthPx)>(lines.Length);
        foreach ((string text, float sizePx, SKColor color) in lines)
        {
            using SKFont metricsFont = new(FontManager.Typeface, sizePx);
            SKFontMetrics metrics = metricsFont.Metrics;
            metricsBuilder.Add((text, sizePx, color, metrics.Ascent, metrics.Descent, metricsFont.MeasureText(text)));
        }

        ImmutableArray<(string Text, float SizePx, SKColor Color, float AscentPx, float DescentPx, float WidthPx)> measuredLines =
            metricsBuilder.ToImmutable();

        ImmutableArray<float>.Builder baselineOffsetsBuilder = ImmutableArray.CreateBuilder<float>(measuredLines.Length);
        baselineOffsetsBuilder.Add(0f);
        for (int index = 1; index < measuredLines.Length; index++)
        {
            (string _, float topSizePx, SKColor _, float _, float topDescentPx, float _) = measuredLines[index - 1];
            (string _, float bottomSizePx, SKColor _, float bottomAscentPx, float _, float _) = measuredLines[index];

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
        float blockWidth = 0f;
        for (int index = 0; index < measuredLines.Length; index++)
        {
            (string _, float _, SKColor _, float ascentPx, float descentPx, float widthPx) = measuredLines[index];
            float baseline = baselineOffsets[index];

            blockTop = Math.Min(blockTop, baseline + ascentPx);
            blockBottom = Math.Max(blockBottom, baseline + descentPx);
            blockWidth = Math.Max(blockWidth, widthPx);
        }

        float blockHeight = blockBottom - blockTop;
        float anchorPointOffset = blockTop + (blockHeight * anchorY);
        float baselineShift = cy - anchorPointOffset;

        float blockLeft = cx - (blockWidth * anchorX);
        float drawX = align switch
        {
            SKTextAlign.Left => blockLeft,
            SKTextAlign.Center => blockLeft + (blockWidth / 2f),
            SKTextAlign.Right => blockLeft + blockWidth,
            _ => blockLeft
        };

        for (int index = 0; index < measuredLines.Length; index++)
        {
            (string text, float sizePx, SKColor color, float _, float _, float _) = measuredLines[index];
            float y = baselineOffsets[index] + baselineShift;

            using SKFont font = new(FontManager.Typeface, sizePx);
            using SKPaint paint = new()
            {
                Color = color,
                IsAntialias = true,
            };

            canvas.DrawText(text, drawX, y, align, font, paint);
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

    internal static SKTextAlign ParseTextAlign(string justify) => justify?.ToLowerInvariant() switch
    {
        "left" or "start" => SKTextAlign.Left,
        "right" or "end" => SKTextAlign.Right,
        _ => SKTextAlign.Center,
    };

    internal static float ParseAnchorX(string anchor) => anchor?.ToLowerInvariant() switch
    {
        "left" or "start" => 0.0f,
        "right" or "end" => 1.0f,
        "center" => 0.5f,
        _ => float.TryParse(anchor, out float f) ? f : 0.5f,
    };

    internal static float ParseAnchorY(string anchor) => anchor?.ToLowerInvariant() switch
    {
        "top" or "start" => 0.0f,
        "bottom" or "end" or "middle" => 1.0f,
        "center" => 0.5f,
        _ => float.TryParse(anchor, out float f) ? f : 0.5f,
    };
}
