// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class TextLayer : ILayer
{
    internal const float DefaultLineHeightRatio = 1.2f;
    internal const float DefaultCollisionGapPx = 1f;

    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (context.Options.TextLines.Length == 0)
            return;

        float cx = context.CanvasOffsetX + context.Options.TextXPx;
        float cy = context.CanvasOffsetY + context.Options.TextYPx;
        SKTextAlign align = ParseTextAlign(context.Options.TextTextAlign);
        float anchorX = ParseAnchorX(context.Options.TextAnchorX);
        float anchorY = ParseAnchorY(context.Options.TextAnchorY);

        const string networkMarker = "\0NETWORK\0";

        List<List<(string Text, float SizePx, SKColor Color)>> visualLines = [];
        List<(string Text, float SizePx, SKColor Color)> currentLine = [];

        for (int i = 0; i < context.Options.TextLines.Length; i++)
        {
            string originalLine = context.Options.TextLines[i];
            float size = context.Options.TextSizesPx[i % context.Options.TextSizesPx.Length];
            SKColor color = context.Options.TextColors[i % context.Options.TextColors.Length];

            if (size <= 0f)
                continue;

            string[] segments = originalLine.Split(["\n", "<br>"], StringSplitOptions.None);

            for (int j = 0; j < segments.Length; j++)
            {
                string segment = segments[j];
                bool isLast = j == segments.Length - 1;

                if (isLast && string.IsNullOrEmpty(segment) && j > 0)
                    continue;

                if (segment.Contains(networkMarker))
                {
                    string[] parts = segment.Split([networkMarker], 2, StringSplitOptions.None);

                    if (!string.IsNullOrEmpty(parts[0]))
                        currentLine.Add((parts[0], size, color));

                    if (currentLine.Count > 0)
                    {
                        visualLines.Add(currentLine);
                        currentLine = [];
                    }

                    ImmutableArray<(string Text, float SizePx, SKColor Color)>.Builder netLines =
                        ImmutableArray.CreateBuilder<(string Text, float SizePx, SKColor Color)>();
                    NetworkLayer.BuildNetworkLines(context.Options, netLines);
                    foreach (var ns in netLines.ToImmutable())
                        visualLines.Add([ns]);

                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                        currentLine.Add((parts[1], size, color));
                }
                else
                {
                    currentLine.Add((segment, size, color));
                }

                if (!isLast)
                {
                    visualLines.Add(currentLine);
                    currentLine = [];
                }
            }
        }

        if (currentLine.Count > 0)
            visualLines.Add(currentLine);

        if (visualLines.Count == 0)
            return;

        List<(float LineAscent, float LineDescent, float LineWidth,
            List<(string Text, float SizePx, SKColor Color, float Width, float Ascent, float Descent)> Segs)> measuredLines = [];

        foreach (var line in visualLines)
        {
            float la = 0f, ld = 0f, lw = 0f;
            var segs = new List<(string Text, float SizePx, SKColor Color, float Width, float Ascent, float Descent)>();

            foreach (var s in line)
            {
                using SKFont f = new(FontManager.Typeface, s.SizePx);
                SKFontMetrics m = f.Metrics;
                float sw = f.MeasureText(s.Text);
                float sa = -m.Ascent;
                float sd = m.Descent;
                la = Math.Max(la, sa);
                ld = Math.Max(ld, sd);
                segs.Add((s.Text, s.SizePx, s.Color, sw, sa, sd));
                lw += sw;
            }

            measuredLines.Add((la, ld, lw, segs));
        }

        List<float> baselineOffsets = [0f];
        for (int i = 1; i < measuredLines.Count; i++)
        {
            float topSz = measuredLines[i - 1].Segs.Max(s => s.SizePx);
            float botSz = measuredLines[i].Segs.Max(s => s.SizePx);
            float adv = ComputeBaselineAdvance(topSz, botSz,
                measuredLines[i - 1].LineDescent, -measuredLines[i].LineAscent,
                DefaultLineHeightRatio, DefaultCollisionGapPx);
            baselineOffsets.Add(baselineOffsets[i - 1] + adv);
        }

        float blockTop = float.PositiveInfinity;
        float blockBottom = float.NegativeInfinity;
        float blockWidth = 0f;
        for (int i = 0; i < measuredLines.Count; i++)
        {
            float bl = baselineOffsets[i];
            blockTop = Math.Min(blockTop, bl - measuredLines[i].LineAscent);
            blockBottom = Math.Max(blockBottom, bl + measuredLines[i].LineDescent);
            blockWidth = Math.Max(blockWidth, measuredLines[i].LineWidth);
        }

        float blockHeight = blockBottom - blockTop;
        float baselineShift = cy - (blockTop + blockHeight * anchorY);
        float blockLeft = cx - blockWidth * anchorX;

        for (int i = 0; i < measuredLines.Count; i++)
        {
            float y = baselineOffsets[i] + baselineShift;
            float x = blockLeft;
            foreach (var seg in measuredLines[i].Segs)
            {
                using SKFont font = new(FontManager.Typeface, seg.SizePx);
                using SKPaint paint = new() { Color = seg.Color, IsAntialias = true };
                canvas.DrawText(seg.Text, x, y, SKTextAlign.Left, font, paint);
                x += seg.Width;
            }
        }
    }

    internal static float ComputeBaselineAdvance(
        float topSizePx, float bottomSizePx,
        float topDescentPx, float bottomAscentPx,
        float lineHeightRatio, float collisionGapPx)
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