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

        // Measure
        List<(float Ascent, float Descent, float Width,
            List<(string Text, float SizePx, SKColor Color, float W, float A, float D)> Segs)> measured = [];

        foreach (var line in visualLines)
        {
            float la = 0f, ld = 0f, lw = 0f;
            var segs = new List<(string, float, SKColor, float, float, float)>();

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
            float x = left;
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

        foreach (AdapterInfo adapter in options.NetworkAdapters)
        {
            int fmtIdx = 0;

            for (int elemIdx = 0; elemIdx < adapterFormat.Length; elemIdx++)
            {
                string template = adapterFormat[elemIdx].Replace("${IpAddresses}", ipMarker);

                float sizePx = options.NetworkSizesPx.Length > 0
                    ? options.NetworkSizesPx[fmtIdx % options.NetworkSizesPx.Length] : 0f;
                SKColor color = options.NetworkColors.Length > 0
                    ? options.NetworkColors[fmtIdx % options.NetworkColors.Length] : SKColors.Transparent;

                if (sizePx <= 0f)
                {
                    fmtIdx++;
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

                        List<(string, float, SKColor)> currentLine = [];

                        if (!string.IsNullOrWhiteSpace(prefix))
                            currentLine.Add((prefix, sizePx, color));

                        if (!ipFormat.IsDefaultOrEmpty)
                        {
                            // Save current format index as the base for IP color/size indexing.
                            // Each IP address uses the same color/size sequence.
                            int ipBaseIdx = fmtIdx;

                            foreach (AdapterIpAddress ip in adapter.IpAddresses)
                            {
                                for (int ipElemIdx = 0; ipElemIdx < ipFormat.Length; ipElemIdx++)
                                {
                                    int idx = ipBaseIdx + ipElemIdx;
                                    float ipSize = options.NetworkSizesPx.Length > 0
                                        ? options.NetworkSizesPx[idx % options.NetworkSizesPx.Length] : 0f;
                                    SKColor ipCol = options.NetworkColors.Length > 0
                                        ? options.NetworkColors[idx % options.NetworkColors.Length] : SKColors.Transparent;

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

                            // Advance past the IP format slots (even if no addresses were rendered)
                            fmtIdx += ipFormat.Length;
                        }

                        if (!string.IsNullOrWhiteSpace(suffix))
                            currentLine.Add((suffix, sizePx, color));

                        if (currentLine.Count > 0)
                            visualLines.Add(currentLine);
                    }
                    else
                    {
                        visualLines.Add([(line, sizePx, color)]);
                        fmtIdx++;
                    }
                }
            }
        }
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