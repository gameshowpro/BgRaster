// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class NetworkLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (!context.Options.NetworkOptions.Render)
        {
            return;
        }

        if (context.Options.NetworkOptions.X.Length == 0 || context.Options.NetworkOptions.Y.Length == 0)
        {
            return;
        }

        if (context.Options.NetworkOptions.AdapterFormat.IsDefaultOrEmpty)
        {
            return;
        }

        if (context.Options.NetworkAdapters.Length == 0)
        {
            return;
        }

        ImmutableArray<(string Text, float SizePx, SKColor Color)>.Builder linesBuilder =
            ImmutableArray.CreateBuilder<(string Text, float SizePx, SKColor Color)>();

        BuildNetworkLines(context.Options, linesBuilder);

        ImmutableArray<(string Text, float SizePx, SKColor Color)> lines = linesBuilder.ToImmutable();

        if (lines.Length == 0)
        {
            return;
        }

        float cx = context.CanvasOffsetX + context.Options.NetworkXPx;
        float cy = context.CanvasOffsetY + context.Options.NetworkYPx;
        SKTextAlign align = TextLayer.ParseTextAlign(context.Options.NetworkTextAlign);
        float anchorX = TextLayer.ParseAnchorX(context.Options.NetworkAnchorX);
        float anchorY = TextLayer.ParseAnchorY(context.Options.NetworkAnchorY);

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

            float advance = TextLayer.ComputeBaselineAdvance(
                topSizePx,
                bottomSizePx,
                topDescentPx,
                bottomAscentPx,
                TextLayer.DefaultLineHeightRatio,
                TextLayer.DefaultCollisionGapPx);
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

    internal static void BuildNetworkLines(
        ResolvedOptions options,
        ImmutableArray<(string Text, float SizePx, SKColor Color)>.Builder linesBuilder)
    {
        if (options.NetworkAdapters.Length == 0)
        {
            return;
        }

        string ipMarker = "\0IP_MARKER\0";
        ImmutableArray<string> adapterFormat = options.NetworkOptions.AdapterFormat;
        ImmutableArray<string> ipFormat = options.NetworkOptions.IpAddressFormat;

        foreach (AdapterInfo adapter in options.NetworkAdapters)
        {
            for (int elemIdx = 0; elemIdx < adapterFormat.Length; elemIdx++)
            {
                string template = adapterFormat[elemIdx]
                    .Replace("${IpAddresses}", ipMarker);

                float sizePx = options.NetworkSizesPx.Length > 0
                    ? options.NetworkSizesPx[elemIdx % options.NetworkSizesPx.Length]
                    : 0f;
                SKColor color = options.NetworkColors.Length > 0
                    ? options.NetworkColors[elemIdx % options.NetworkColors.Length]
                    : SKColors.Transparent;

                if (sizePx <= 0f)
                {
                    continue;
                }

                string formatted = NetworkFormatter.FormatAdapter(adapter,
                    ImmutableArray.Create(template));
                string[] subLines = formatted.Split(["\n", "<br>"], StringSplitOptions.None);

                for (int subIdx = 0; subIdx < subLines.Length; subIdx++)
                {
                    string line = subLines[subIdx];

                    // Skip trailing empty from split
                    if (subIdx == subLines.Length - 1 && string.IsNullOrEmpty(line) && subIdx > 0)
                    {
                        continue;
                    }

                    if (line.Contains(ipMarker))
                    {
                        string[] parts = line.Split([ipMarker], StringSplitOptions.None);
                        string prefix = parts[0];
                        string suffix = parts.Length > 1 ? parts[1] : "";

                        if (!string.IsNullOrWhiteSpace(prefix))
                        {
                            linesBuilder.Add((prefix, sizePx, color));
                        }

                        if (!ipFormat.IsDefaultOrEmpty)
                        {
                            foreach (AdapterIpAddress ip in adapter.IpAddresses)
                            {
                                for (int ipElemIdx = 0; ipElemIdx < ipFormat.Length; ipElemIdx++)
                                {
                                    float ipSizePx = options.NetworkSizesPx.Length > 0
                                        ? options.NetworkSizesPx[ipElemIdx % options.NetworkSizesPx.Length]
                                        : 0f;
                                    SKColor ipColor = options.NetworkColors.Length > 0
                                        ? options.NetworkColors[ipElemIdx % options.NetworkColors.Length]
                                        : SKColors.Transparent;

                                    if (ipSizePx <= 0f)
                                    {
                                        continue;
                                    }

                                    string ipFormatted = NetworkFormatter.FormatIpAddress(ip,
                                        ImmutableArray.Create(ipFormat[ipElemIdx]));
                                    string[] ipSubLines = ipFormatted.Split(["\n", "<br>"], StringSplitOptions.None);

                                    for (int ipSubIdx = 0; ipSubIdx < ipSubLines.Length; ipSubIdx++)
                                    {
                                        string ipLine = ipSubLines[ipSubIdx];
                                        if (ipSubIdx == ipSubLines.Length - 1 && string.IsNullOrEmpty(ipLine) && ipSubIdx > 0)
                                        {
                                            continue;
                                        }
                                        linesBuilder.Add((ipLine, ipSizePx, ipColor));
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(suffix))
                        {
                            linesBuilder.Add((suffix, sizePx, color));
                        }
                    }
                    else
                    {
                        linesBuilder.Add((line, sizePx, color));
                    }
                }
            }
        }
    }
}