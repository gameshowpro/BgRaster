// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal sealed class NetworkLayer : ILayer
{
    public void Render(RenderContext context, SKCanvas canvas)
    {
        if (!context.Options.NetworkOptions.Render)
            return;

        if (context.Options.NetworkOptions.X.Length == 0 || context.Options.NetworkOptions.Y.Length == 0)
            return;

        if (context.Options.NetworkOptions.AdapterFormat.IsDefaultOrEmpty)
            return;

        if (context.Options.NetworkAdapters.Length == 0)
            return;

        // Build visual lines: each line has segments with individual sizes/colors
        List<List<(string Text, float SizePx, SKColor Color)>> visualLines = [];
        BuildNetworkLines(context.Options, visualLines);

        if (visualLines.Count == 0)
            return;

        float cx = context.CanvasOffsetX + context.Options.NetworkXPx;
        float cy = context.CanvasOffsetY + context.Options.NetworkYPx;
        float anchorX = TextLayer.ParseAnchorX(context.Options.NetworkAnchorX);
        float anchorY = TextLayer.ParseAnchorY(context.Options.NetworkAnchorY);
        SKTextAlign align = TextLayer.ParseTextAlign(context.Options.NetworkTextAlign);

        // Measure
        List<(float Ascent, float Descent, float Width,
            List<(string Text, float SizePx, SKColor Color, float W, float A, float D)> Segs)> measured = [];

        foreach (var line in visualLines)
        {
            float la = 0f, ld = 0f, lw = 0f;
            var segs = new List<(string, float, SKColor, float, float, float)>();

            foreach (var s in line)
            {
                string measureText = s.Text.Length > 0 && s.Text[^1] == ' '
                    ? s.Text + '\u200C'
                    : s.Text;
                using SKFont f = new(FontManager.Typeface, s.SizePx);
                SKFontMetrics m = f.Metrics;
                float sw = f.MeasureText(measureText);
                float sa = -m.Ascent;
                float sd = m.Descent;
                la = Math.Max(la, sa);
                ld = Math.Max(ld, sd);
                segs.Add((measureText, s.SizePx, s.Color, sw, sa, sd));
                lw += sw;
            }
            measured.Add((la, ld, lw, segs));
        }

        // Baselines
        List<float> baseline = [0f];
        for (int i = 1; i < measured.Count; i++)
        {
            float topSz = measured[i - 1].Segs.Max(s => s.SizePx);
            float botSz = measured[i].Segs.Max(s => s.SizePx);
            float adv = TextLayer.ComputeBaselineAdvance(topSz, botSz,
                measured[i - 1].Descent, -measured[i].Ascent,
                TextLayer.DefaultLineHeightRatio, TextLayer.DefaultCollisionGapPx);
            baseline.Add(baseline[i - 1] + adv);
        }

        // Block bounds
        float bt = float.PositiveInfinity, bb = float.NegativeInfinity, bw = 0f;
        for (int i = 0; i < measured.Count; i++)
        {
            float bl = baseline[i];
            bt = Math.Min(bt, bl - measured[i].Ascent);
            bb = Math.Max(bb, bl + measured[i].Descent);
            bw = Math.Max(bw, measured[i].Width);
        }

        float shift = cy - (bt + (bb - bt) * anchorY);
        float left = cx - bw * anchorX;

        // Draw
        for (int i = 0; i < measured.Count; i++)
        {
            float y = baseline[i] + shift;
            float lineStartX = left;
            if (align == SKTextAlign.Center)
                lineStartX += (bw - measured[i].Width) / 2f;
            else if (align == SKTextAlign.Right)
                lineStartX += bw - measured[i].Width;
            float x = lineStartX;
            foreach (var seg in measured[i].Segs)
            {
                using SKFont f = new(FontManager.Typeface, seg.SizePx);
                using SKPaint p = new() { Color = seg.Color, IsAntialias = true };
                canvas.DrawText(seg.Text, x, y, SKTextAlign.Left, f, p);
                x += seg.W;
            }
        }
    }

    internal static void BuildNetworkLines(
        ResolvedOptions options,
        List<List<(string Text, float SizePx, SKColor Color)>> visualLines)
    {
        if (options.NetworkAdapters.Length == 0)
            return;

        string ipMarker = "\0IP_MARKER\0";
        ImmutableArray<string> adapterFormat = options.NetworkOptions.AdapterFormat;
        ImmutableArray<string> ipFormat = options.NetworkOptions.IpAddressFormat;

        List<(string Text, float SizePx, SKColor Color)> currentLine = [];

        foreach (AdapterInfo adapter in options.NetworkAdapters)
        {
            int sizeIdx = 0;
            int colorIdx = 0;

            for (int elemIdx = 0; elemIdx < adapterFormat.Length; elemIdx++)
            {
                string template = adapterFormat[elemIdx].Replace("${IpAddresses}", ipMarker);
                bool consumedIpSlots = false;
                int ipSlotCount = 0;

                float sizePx = options.NetworkSizesPx.Length > 0
                    ? options.NetworkSizesPx[sizeIdx % options.NetworkSizesPx.Length] : 0f;
                SKColor color = options.NetworkColors.Length > 0
                    ? options.NetworkColors[colorIdx % options.NetworkColors.Length] : SKColors.Transparent;

                if (sizePx <= 0f)
                {
                    sizeIdx++;
                    colorIdx++;
                    continue;
                }

                string formatted = NetworkFormatter.FormatAdapter(adapter,
                    ImmutableArray.Create(template));
                string[] subLines = formatted.Split(["\n", "<br>"], StringSplitOptions.None);

                for (int subIdx = 0; subIdx < subLines.Length; subIdx++)
                {
                    string line = subLines[subIdx];

                    if (subIdx == subLines.Length - 1 && string.IsNullOrEmpty(line) && subIdx > 0)
                        continue;

                    if (line.Contains(ipMarker))
                    {
                        string[] parts = line.Split([ipMarker], StringSplitOptions.None);
                        string prefix = parts[0];
                        string suffix = parts.Length > 1 ? parts[1] : "";

                        if (!string.IsNullOrEmpty(prefix))
                            currentLine.Add((prefix, sizePx, color));

                        if (!ipFormat.IsDefaultOrEmpty)
                        {
                            consumedIpSlots = true;
                            ipSlotCount = ipFormat.Length;
                            int ipBaseSizeIdx = sizeIdx;
                            int ipBaseColorIdx = colorIdx;

                            foreach (AdapterIpAddress ip in adapter.IpAddresses)
                            {
                                for (int ipElemIdx = 0; ipElemIdx < ipFormat.Length; ipElemIdx++)
                                {
                                    int sIdx = ipBaseSizeIdx + ipElemIdx;
                                    int cIdx = ipBaseColorIdx + ipElemIdx;
                                    float ipSize = options.NetworkSizesPx.Length > 0
                                        ? options.NetworkSizesPx[sIdx % options.NetworkSizesPx.Length] : 0f;
                                    SKColor ipCol = options.NetworkColors.Length > 0
                                        ? options.NetworkColors[cIdx % options.NetworkColors.Length] : SKColors.Transparent;

                                    if (ipSize <= 0f)
                                        continue;

                                    string ipFmt = NetworkFormatter.FormatIpAddress(ip,
                                        ImmutableArray.Create(ipFormat[ipElemIdx]));
                                    string[] ipSegs = ipFmt.Split(["\n", "<br>"], StringSplitOptions.None);

                                    for (int s = 0; s < ipSegs.Length; s++)
                                    {
                                        string seg = ipSegs[s];
                                        bool isLast = s == ipSegs.Length - 1;

                                        if (isLast && string.IsNullOrEmpty(seg) && s > 0)
                                            continue;

                                        currentLine.Add((seg, ipSize, ipCol));

                                        if (!isLast)
                                        {
                                            visualLines.Add(currentLine);
                                            currentLine = [];
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(suffix))
                        {
                            // Suffix inherits the last IP sub-element's index so it visually belongs
                            int suffixSIdx = sizeIdx + (ipFormat.IsDefaultOrEmpty ? 0 : ipFormat.Length - 1);
                            int suffixCIdx = colorIdx + (ipFormat.IsDefaultOrEmpty ? 0 : ipFormat.Length - 1);
                            float suffixSize = options.NetworkSizesPx.Length > 0
                                ? options.NetworkSizesPx[suffixSIdx % options.NetworkSizesPx.Length] : 0f;
                            SKColor suffixColor = options.NetworkColors.Length > 0
                                ? options.NetworkColors[suffixCIdx % options.NetworkColors.Length] : SKColors.Transparent;
                            currentLine.Add((suffix, suffixSize, suffixColor));
                        }
                    }
                    else
                    {
                        currentLine.Add((line, sizePx, color));
                    }

                    if (subIdx < subLines.Length - 1 && currentLine.Count > 0)
                    {
                        visualLines.Add(currentLine);
                        currentLine = [];
                    }
                }
                if (consumedIpSlots)
                {
                    sizeIdx += ipSlotCount;
                    colorIdx += ipSlotCount;
                }
                else
                {
                    sizeIdx++;
                    colorIdx++;
                }
            }
        }

        if (currentLine.Count > 0)
            visualLines.Add(currentLine);
    }

    // Legacy overload for TextLayer inline network rendering — flattens to simple lines
    internal static void BuildNetworkLines(
        ResolvedOptions options,
        ImmutableArray<(string Text, float SizePx, SKColor Color)>.Builder linesBuilder)
    {
        List<List<(string Text, float SizePx, SKColor Color)>> visualLines = [];
        BuildNetworkLines(options, visualLines);
        foreach (var line in visualLines)
        {
            foreach (var seg in line)
                linesBuilder.Add((seg.Text, seg.SizePx, seg.Color));
        }
    }
}