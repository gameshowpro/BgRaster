// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.StateCache;

internal static class LastRunWriter
{
    internal static string BuildEffectiveConfigToml(GlobalOptions options)
    {
        StringBuilder sb = new();
        WriteEffectiveConfig(sb, options, new RunStatus());
        return sb.ToString().TrimEnd();
    }

    internal static void Write(
        string path,
        LastRunState state,
        string _schemaVersion,
        RunStatus? runStatus = null,
        ILogger? logger = null)
    {
        runStatus ??= new RunStatus();

        try
        {
            string directory = Path.GetDirectoryName(path) ?? ".";
            _ = Directory.CreateDirectory(directory);

            StringBuilder sb = new();
            _ = sb.AppendLine("# $schema: https://raw.githubusercontent.com/gameshowpro/BgRaster/refs/heads/main/docs/schemas/bgraster-lastrun.schema.json");
            _ = sb.AppendLine();

            _ = sb.AppendLine("[meta]");
            _ = sb.AppendLine($"version = \"{Escape(state.Meta.Version)}\"");
            _ = sb.AppendLine($"settingsHash = \"{Escape(state.Meta.SettingsHash)}\"");
            _ = sb.AppendLine($"timestamp = \"{Escape(state.Meta.Timestamp)}\"");
            WriteInlineTable(sb, "assignedFiles", state.Meta.AssignedFiles);
            WriteStringArray(sb, "unrecycledFiles", state.Meta.UnrecycledFiles);
            _ = sb.AppendLine();

            foreach (OutputRecord hw in state.HardwareOutputs.OrderBy(o => o.Id, StringComparer.Ordinal))
            {
                if (runStatus.HardwareStatuses.TryGetValue(hw.Id, out string? status))
                {
                    _ = sb.AppendLine($"# bg-raster: status={status} id=\"{Escape(hw.Id)}\" index={hw.Index} position={hw.DesktopX},{hw.DesktopY} resolution={hw.WidthPx}x{hw.HeightPx}");
                }
                _ = sb.AppendLine("[[hardware_output]]");
                _ = sb.AppendLine($"id = \"{Escape(hw.Id)}\"");
                _ = sb.AppendLine($"index = {hw.Index}");
                _ = sb.AppendLine($"desktopX = {hw.DesktopX}");
                _ = sb.AppendLine($"desktopY = {hw.DesktopY}");
                _ = sb.AppendLine($"widthPx = {hw.WidthPx}");
                _ = sb.AppendLine($"heightPx = {hw.HeightPx}");
                _ = sb.AppendLine($"dpiX = {hw.DpiX}");
                _ = sb.AppendLine($"dpiY = {hw.DpiY}");
                _ = sb.AppendLine($"rotation = {hw.Rotation}");
                _ = sb.AppendLine($"adapterName = \"{Escape(hw.AdapterName)}\"");
                _ = sb.AppendLine($"friendlyName = \"{Escape(hw.FriendlyName)}\"");
                _ = sb.AppendLine();
            }

            WriteEffectiveConfig(sb, state.EffectiveConfig, runStatus);

            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, sb.ToString(), Encoding.UTF8);

            if (!VerifyRoundTrip(tempPath, state, logger))
            {
                logger?.RoundTripFailed(path);
                File.Delete(tempPath);
                return;
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            logger?.WriteFailure(ex, path);
        }
    }

    private static bool VerifyRoundTrip(string path, LastRunState original, ILogger? logger)
    {
        LastRunState? parsed = LastRunReader.Read(path);
        if (parsed is null)
        {
            logger?.RoundTripDidNotParse();
            return false;
        }

        if (parsed.Meta.Version != original.Meta.Version
            || parsed.Meta.SettingsHash != original.Meta.SettingsHash
            || parsed.Meta.Timestamp != original.Meta.Timestamp)
        {
            logger?.RoundTripMetaScalarMismatch();
            return false;
        }

        if (!DictionaryEqual(parsed.Meta.AssignedFiles, original.Meta.AssignedFiles))
        {
            logger?.RoundTripAssignedFilesMismatch();
            return false;
        }

        if (!parsed.Meta.UnrecycledFiles.SequenceEqual(original.Meta.UnrecycledFiles, StringComparer.Ordinal))
        {
            logger?.RoundTripUnrecycledFilesMismatch();
            return false;
        }

        ImmutableArray<OutputRecord> origSorted = [.. original.HardwareOutputs.OrderBy(o => o.Id, StringComparer.Ordinal)];
        ImmutableArray<OutputRecord> parsedSorted = [.. parsed.HardwareOutputs.OrderBy(o => o.Id, StringComparer.Ordinal)];
        if (origSorted.Length != parsedSorted.Length)
        {
            logger?.RoundTripHardwareCountMismatch();
            return false;
        }
        for (int i = 0; i < origSorted.Length; i++)
        {
            if (origSorted[i] != parsedSorted[i])
            {
                logger?.RoundTripHardwareItemMismatch(i);
                return false;
            }
        }
        return true;
    }

    private static string VerbosityToml(LogLevel level) => level switch
    {
        LogLevel.Warning => "quiet",
        LogLevel.Debug or LogLevel.Trace => "verbose",
        _ => "normal",
    };

    private static bool DictionaryEqual(FrozenDictionary<string, string> a, FrozenDictionary<string, string> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, string> kv in a)
        {
            if (!b.TryGetValue(kv.Key, out string? other) || other != kv.Value)
            {
                return false;
            }
        }
        return true;
    }

    private static void WriteEffectiveConfig(StringBuilder sb, GlobalOptions opts, RunStatus runStatus)
    {
        WriteTextSection(sb, opts.Text);
        WriteBackgroundSection(sb, opts.Background);
        WriteGridSection(sb, opts.Grid);
        WriteCircleSection(sb, opts.Circle);
        WriteCrosshairSection(sb, opts.Crosshair);
        WriteLabeledEdgesSection(sb, opts.LabeledEdges);
        WriteNetworkSection(sb, opts.Network);
        WriteLogoSection(sb, opts.Logo);
        WriteRenderSection(sb, opts.Render);
        WriteOutputsSection(sb, opts.Outputs, runStatus);
    }

    private static void WriteTextSection(StringBuilder sb, TextOptions t)
    {
        _ = sb.AppendLine("[text]");
        WriteTomlStringArray(sb, "format", t.Format);
        WriteTomlStringArray(sb, "size", t.Size);
        WriteTomlStringArray(sb, "color", t.Color);
        WriteTomlStringArray(sb, "x", t.X);
        WriteTomlStringArray(sb, "y", t.Y);
        _ = sb.AppendLine();
    }

    private static void WriteBackgroundSection(StringBuilder sb, BackgroundOptions b)
    {
        _ = sb.AppendLine("[background]");
        WriteTomlStringArray(sb, "color", b.Color);
        WriteTomlStringArray(sb, "image", b.Image);
        WriteTomlStringArray(sb, "fit", b.Fit);
        WriteTomlBoolArray(sb, "alternating", b.Alternating);
        WriteTomlBoolArray(sb, "border", b.Border);
        WriteTomlStringArray(sb, "border-color", b.BorderColor);
        _ = sb.AppendLine();
    }

    private static void WriteGridSection(StringBuilder sb, GridOptions g)
    {
        _ = sb.AppendLine("[grid]");
        WriteTomlStringArray(sb, "size", g.Size);
        WriteTomlStringArray(sb, "odd-color", g.OddColor);
        WriteTomlStringArray(sb, "even-color", g.EvenColor);
        WriteTomlStringArray(sb, "stroke", g.Stroke);
        WriteTomlStringArray(sb, "offset-x", g.OffsetX);
        WriteTomlStringArray(sb, "offset-y", g.OffsetY);
        WriteTomlBoolArray(sb, "coordinates", g.Coordinates);
        _ = sb.AppendLine();
    }

    private static void WriteCircleSection(StringBuilder sb, CircleOptions c)
    {
        _ = sb.AppendLine("[circle]");
        WriteTomlStringArray(sb, "x", c.X);
        WriteTomlStringArray(sb, "y", c.Y);
        WriteTomlStringArray(sb, "size", c.Size);
        WriteTomlStringArray(sb, "color", c.Color);
        WriteTomlStringArray(sb, "stroke", c.Stroke);
        _ = sb.AppendLine();
    }

    private static void WriteCrosshairSection(StringBuilder sb, CrosshairOptions c)
    {
        _ = sb.AppendLine("[crosshair]");
        WriteTomlStringArray(sb, "x", c.X);
        WriteTomlStringArray(sb, "y", c.Y);
        WriteTomlStringArray(sb, "length", c.Length);
        WriteTomlStringArray(sb, "color", c.Color);
        WriteTomlStringArray(sb, "stroke", c.Stroke);
        _ = sb.AppendLine();
    }

    private static void WriteLabeledEdgesSection(StringBuilder sb, LabeledEdgesOptions l)
    {
        _ = sb.AppendLine("[labeled-edges]");
        WriteTomlStringArray(sb, "text-size", l.TextSize);
        WriteTomlStringArray(sb, "tail-length", l.TailLength);
        WriteTomlStringArray(sb, "thickness", l.Thickness);
        WriteTomlFloatArray(sb, "head-scale", l.HeadScale);
        WriteTomlStringArray(sb, "scope", [.. l.Scope.Select(scope => scope.ToString())]);
        WriteTomlStringArray(sb, "side", [.. l.Side.Select(side => side.ToString())]);
        _ = sb.AppendLine();
    }

    private static void WriteNetworkSection(StringBuilder sb, NetworkOptions n)
    {
        _ = sb.AppendLine("[network]");
        _ = sb.AppendLine($"render = {n.Render.ToString().ToLowerInvariant()}");
        WriteTomlStringArray(sb, "x", n.X);
        WriteTomlStringArray(sb, "y", n.Y);
        WriteTomlStringArray(sb, "size", n.Size);
        WriteTomlStringArray(sb, "color", n.Color);
        WriteTomlStringArray(sb, "require_adapter_type", n.RequireAdapterType);
        WriteTomlStringArray(sb, "exclude_adapter_type", n.ExcludeAdapterType);
        _ = sb.AppendLine($"require_up = {n.RequireUp.ToString().ToLowerInvariant()}");
        _ = sb.AppendLine($"require_family = \"{Escape(n.RequireFamily)}\"");
        WriteTomlStringArray(sb, "require_mac_address", n.RequireMacAddress);
        WriteTomlStringArray(sb, "require_subnet", n.RequireSubnet);
        _ = sb.AppendLine($"minimum_address_count = {n.MinimumAddressCount}");
        WriteTomlStringArray(sb, "require_name", n.RequireName);
        WriteTomlStringArray(sb, "require_description", n.RequireDescription);
        _ = sb.AppendLine($"ip_address_format = \"{Escape(n.IpAddressFormat)}\"");
        _ = sb.AppendLine($"adapter_format = \"{Escape(n.AdapterFormat)}\"");
        _ = sb.AppendLine($"text-align = \"{n.TextAlign}\"");
        _ = sb.AppendLine($"anchor-x = \"{n.AnchorX}\"");
        _ = sb.AppendLine($"anchor-y = \"{n.AnchorY}\"");
        _ = sb.AppendLine();
    }

    private static void WriteLogoSection(StringBuilder sb, LogoOptions l)
    {
        _ = sb.AppendLine("[logo]");
        WriteTomlStringArray(sb, "source", l.Source);
        WriteTomlStringArray(sb, "x", l.X);
        WriteTomlStringArray(sb, "y", l.Y);
        WriteTomlStringArray(sb, "width", l.Width);
        WriteTomlStringArray(sb, "height", l.Height);
        WriteTomlFloatArray(sb, "opacity", l.Opacity);
        _ = sb.AppendLine();
    }

    private static void WriteRenderSection(StringBuilder sb, RenderOptions r)
    {
        _ = sb.AppendLine("[render]");
        _ = sb.AppendLine($"no-assignment = {r.DryRun.ToString().ToLowerInvariant()}");
        _ = sb.AppendLine($"no-discovery = {r.NoDiscovery.ToString().ToLowerInvariant()}");
        _ = sb.AppendLine($"outputs-skip-unspecified = {r.OutputsSkipUnspecified.ToString().ToLowerInvariant()}");
        _ = sb.AppendLine($"output = \"{Escape(r.Output)}\"");
        _ = sb.AppendLine($"force = {r.ContinueAfterUnchanged.ToString().ToLowerInvariant()}");
        _ = sb.AppendLine($"verbosity = \"{VerbosityToml(r.MinimumLogLevel)}\"");
        if (!string.IsNullOrEmpty(r.MachineName))
        {
            _ = sb.AppendLine($"machine-name = \"{Escape(r.MachineName)}\"");
        }

        if (r.SimulateNetwork)
        {
            _ = sb.AppendLine("simulate-network = true");
        }

        _ = sb.AppendLine();
    }

    private static void WriteOutputsSection(StringBuilder sb, ImmutableArray<OutputOptions> outputs, RunStatus runStatus)
    {
        int idx = 0;
        foreach (OutputOptions o in outputs)
        {
            string target = o.Target switch
            {
                OutputTarget.IndexTarget(int i) => i.ToString(CultureInfo.InvariantCulture),
                OutputTarget.IdTarget(string id) => $"\"{Escape(id)}\"",
                _ => "0",
            };

            ConfiguredOutputStatus? cs = idx < runStatus.ConfiguredOutputs.Length
                ? runStatus.ConfiguredOutputs[idx]
                : null;
            if (cs is not null)
            {
                string reasonSuffix = string.IsNullOrEmpty(cs.Reason) ? "" : $" {cs.Reason}";
                _ = sb.AppendLine($"# bg-raster: status={cs.Status} target={target}{reasonSuffix}");
            }

            _ = sb.AppendLine("[[output]]");
            _ = sb.AppendLine($"target = {target}");
            if (o.HardwareOutput is OutputRecord hardwareOutput)
            {
                _ = sb.AppendLine("[output.hardware_output]");
                _ = sb.AppendLine($"id = \"{Escape(hardwareOutput.Id)}\"");
                _ = sb.AppendLine($"index = {hardwareOutput.Index}");
                _ = sb.AppendLine($"desktopX = {hardwareOutput.DesktopX}");
                _ = sb.AppendLine($"desktopY = {hardwareOutput.DesktopY}");
                _ = sb.AppendLine($"widthPx = {hardwareOutput.WidthPx}");
                _ = sb.AppendLine($"heightPx = {hardwareOutput.HeightPx}");
                _ = sb.AppendLine($"dpiX = {hardwareOutput.DpiX}");
                _ = sb.AppendLine($"dpiY = {hardwareOutput.DpiY}");
                _ = sb.AppendLine($"rotation = {hardwareOutput.Rotation}");
                _ = sb.AppendLine($"adapterName = \"{Escape(hardwareOutput.AdapterName)}\"");
                _ = sb.AppendLine($"friendlyName = \"{Escape(hardwareOutput.FriendlyName)}\"");
            }
            _ = sb.AppendLine();

            int sliceIdx = 0;
            foreach (SliceOptions s in o.Slices)
            {
                if (cs is not null && sliceIdx < cs.Slices.Length)
                {
                    SliceStatus ss = cs.Slices[sliceIdx];
                    string reasonSuffix = string.IsNullOrEmpty(ss.Reason) ? "" : $" reason=\"{Escape(ss.Reason)}\"";
                    _ = sb.AppendLine($"# bg-raster: status={ss.Status}{reasonSuffix}");
                }
                _ = sb.AppendLine("[[output.slice]]");
                _ = sb.AppendLine($"x = \"{Escape(s.X)}\"");
                _ = sb.AppendLine($"y = \"{Escape(s.Y)}\"");
                _ = sb.AppendLine($"width = \"{Escape(s.Width)}\"");
                _ = sb.AppendLine($"height = \"{Escape(s.Height)}\"");
                WriteSliceFeatureSection(sb, "output.slice.text", s.Text);
                WriteSliceFeatureSection(sb, "output.slice.background", s.Background);
                WriteSliceFeatureSection(sb, "output.slice.grid", s.Grid);
                WriteSliceFeatureSection(sb, "output.slice.circle", s.Circle);
                WriteSliceFeatureSection(sb, "output.slice.crosshair", s.Crosshair);
                WriteSliceFeatureSection(sb, "output.slice.labeled-edges", s.LabeledEdges);
                WriteSliceFeatureSection(sb, "output.slice.logo", s.Logo);
                _ = sb.AppendLine();
                sliceIdx++;
            }
            idx++;
        }
    }

    private static void WriteSliceFeatureSection(StringBuilder sb, string key, object? value)
    {
        switch (value)
        {
            case TextOverride textOverride:
                _ = sb.AppendLine($"[{key}]");
                if (textOverride.Format is not null)
                {
                    WriteTomlStringArray(sb, "format", textOverride.Format.Value);
                }

                if (textOverride.Size is not null)
                {
                    WriteTomlStringArray(sb, "size", textOverride.Size.Value);
                }

                if (textOverride.Color is not null)
                {
                    WriteTomlStringArray(sb, "color", textOverride.Color.Value);
                }

                if (textOverride.X is not null)
                {
                    _ = sb.AppendLine($"x = \"{Escape(textOverride.X)}\"");
                }

                if (textOverride.Y is not null)
                {
                    _ = sb.AppendLine($"y = \"{Escape(textOverride.Y)}\"");
                }

                break;
            case BackgroundOverride backgroundOverride:
                _ = sb.AppendLine($"[{key}]");
                if (backgroundOverride.Color is not null)
                {
                    _ = sb.AppendLine($"color = \"{Escape(backgroundOverride.Color)}\"");
                }

                if (backgroundOverride.Image is not null)
                {
                    _ = sb.AppendLine($"image = \"{Escape(backgroundOverride.Image)}\"");
                }

                if (backgroundOverride.Fit is not null)
                {
                    _ = sb.AppendLine($"fit = \"{Escape(backgroundOverride.Fit)}\"");
                }

                if (backgroundOverride.Alternating is not null)
                {
                    _ = sb.AppendLine($"alternating = {backgroundOverride.Alternating.Value.ToString().ToLowerInvariant()}");
                }

                if (backgroundOverride.Border is not null)
                {
                    _ = sb.AppendLine($"border = {backgroundOverride.Border.Value.ToString().ToLowerInvariant()}");
                }

                if (backgroundOverride.BorderColor is not null)
                {
                    _ = sb.AppendLine($"border-color = \"{Escape(backgroundOverride.BorderColor)}\"");
                }

                break;
            case GridOverride gridOverride:
                _ = sb.AppendLine($"[{key}]");
                if (gridOverride.Size is not null)
                {
                    _ = sb.AppendLine($"size = \"{Escape(gridOverride.Size)}\"");
                }

                if (gridOverride.OddColor is not null)
                {
                    _ = sb.AppendLine($"odd-color = \"{Escape(gridOverride.OddColor)}\"");
                }

                if (gridOverride.EvenColor is not null)
                {
                    _ = sb.AppendLine($"even-color = \"{Escape(gridOverride.EvenColor)}\"");
                }

                if (gridOverride.Stroke is not null)
                {
                    _ = sb.AppendLine($"stroke = \"{Escape(gridOverride.Stroke)}\"");
                }

                if (gridOverride.OffsetX is not null)
                {
                    _ = sb.AppendLine($"offset-x = \"{Escape(gridOverride.OffsetX)}\"");
                }

                if (gridOverride.OffsetY is not null)
                {
                    _ = sb.AppendLine($"offset-y = \"{Escape(gridOverride.OffsetY)}\"");
                }

                if (gridOverride.Coordinates is not null)
                {
                    _ = sb.AppendLine($"coordinates = {gridOverride.Coordinates.Value.ToString().ToLowerInvariant()}");
                }

                break;
            case CircleOverride circleOverride:
                _ = sb.AppendLine($"[{key}]");
                if (circleOverride.X is not null)
                {
                    _ = sb.AppendLine($"x = \"{Escape(circleOverride.X)}\"");
                }

                if (circleOverride.Y is not null)
                {
                    _ = sb.AppendLine($"y = \"{Escape(circleOverride.Y)}\"");
                }

                if (circleOverride.Size is not null)
                {
                    _ = sb.AppendLine($"size = \"{Escape(circleOverride.Size)}\"");
                }

                if (circleOverride.Color is not null)
                {
                    _ = sb.AppendLine($"color = \"{Escape(circleOverride.Color)}\"");
                }

                if (circleOverride.Stroke is not null)
                {
                    _ = sb.AppendLine($"stroke = \"{Escape(circleOverride.Stroke)}\"");
                }

                break;
            case CrosshairOverride crosshairOverride:
                _ = sb.AppendLine($"[{key}]");
                if (crosshairOverride.X is not null)
                {
                    _ = sb.AppendLine($"x = \"{Escape(crosshairOverride.X)}\"");
                }

                if (crosshairOverride.Y is not null)
                {
                    _ = sb.AppendLine($"y = \"{Escape(crosshairOverride.Y)}\"");
                }

                if (crosshairOverride.Length is not null)
                {
                    _ = sb.AppendLine($"length = \"{Escape(crosshairOverride.Length)}\"");
                }

                if (crosshairOverride.Color is not null)
                {
                    _ = sb.AppendLine($"color = \"{Escape(crosshairOverride.Color)}\"");
                }

                if (crosshairOverride.Stroke is not null)
                {
                    _ = sb.AppendLine($"stroke = \"{Escape(crosshairOverride.Stroke)}\"");
                }

                break;
            case LabeledEdgesOverride labeledEdgesOverride:
                _ = sb.AppendLine($"[{key}]");
                if (labeledEdgesOverride.TextSize is not null)
                {
                    _ = sb.AppendLine($"text-size = \"{Escape(labeledEdgesOverride.TextSize)}\"");
                }

                if (labeledEdgesOverride.TailLength is not null)
                {
                    _ = sb.AppendLine($"tail-length = \"{Escape(labeledEdgesOverride.TailLength)}\"");
                }

                if (labeledEdgesOverride.Thickness is not null)
                {
                    _ = sb.AppendLine($"thickness = \"{Escape(labeledEdgesOverride.Thickness)}\"");
                }

                if (labeledEdgesOverride.HeadScale is not null)
                {
                    _ = sb.AppendLine($"head-scale = {labeledEdgesOverride.HeadScale.Value.ToString(CultureInfo.InvariantCulture)}");
                }

                if (labeledEdgesOverride.Scope is not null)
                {
                    _ = sb.AppendLine($"scope = \"{Escape(labeledEdgesOverride.Scope)}\"");
                }

                if (labeledEdgesOverride.Side is not null)
                {
                    ImmutableArray<string>.Builder sideValues = ImmutableArray.CreateBuilder<string>(labeledEdgesOverride.Side.Value.Length);
                    foreach (LabeledEdgeSide side in labeledEdgesOverride.Side.Value)
                    {
                        sideValues.Add(side.ToString());
                    }

                    WriteTomlStringArray(sb, "side", sideValues.ToImmutable());
                }
                break;
            case LogoOverride logoOverride:
                _ = sb.AppendLine($"[{key}]");
                if (logoOverride.Source is not null)
                {
                    _ = sb.AppendLine($"source = \"{Escape(logoOverride.Source)}\"");
                }

                if (logoOverride.X is not null)
                {
                    _ = sb.AppendLine($"x = \"{Escape(logoOverride.X)}\"");
                }

                if (logoOverride.Y is not null)
                {
                    _ = sb.AppendLine($"y = \"{Escape(logoOverride.Y)}\"");
                }

                if (logoOverride.Width is not null)
                {
                    _ = sb.AppendLine($"width = \"{Escape(logoOverride.Width)}\"");
                }

                if (logoOverride.Height is not null)
                {
                    _ = sb.AppendLine($"height = \"{Escape(logoOverride.Height)}\"");
                }

                if (logoOverride.Opacity is not null)
                {
                    _ = sb.AppendLine($"opacity = {logoOverride.Opacity.Value.ToString(CultureInfo.InvariantCulture)}");
                }

                break;
        }

        if (value is not null)
        {
            _ = sb.AppendLine();
        }
    }

    private static void WriteTomlStringArray(StringBuilder sb, string key, ImmutableArray<string> values)
    {
        _ = sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append('"').Append(Escape(values[i])).Append('"');
        }
        _ = sb.AppendLine("]");
    }

    private static void WriteTomlBoolArray(StringBuilder sb, string key, ImmutableArray<bool> values)
    {
        _ = sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append(values[i].ToString().ToLowerInvariant());
        }
        _ = sb.AppendLine("]");
    }

    private static void WriteTomlFloatArray(StringBuilder sb, string key, ImmutableArray<float> values)
    {
        _ = sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append(values[i].ToString(CultureInfo.InvariantCulture));
        }
        _ = sb.AppendLine("]");
    }

    private static void WriteInlineTable(StringBuilder sb, string key, FrozenDictionary<string, string> dict)
    {
        _ = sb.Append(key).Append(" = {");
        bool first = true;
        foreach (KeyValuePair<string, string> kv in dict.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (!first)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append('"').Append(Escape(kv.Key)).Append("\" = \"").Append(Escape(kv.Value)).Append('"');
            first = false;
        }
        _ = sb.AppendLine("}");
    }

    private static void WriteStringArray(StringBuilder sb, string key, ImmutableArray<string> values)
    {
        _ = sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(", ");
            }

            _ = sb.Append('"').Append(Escape(values[i])).Append('"');
        }
        _ = sb.AppendLine("]");
    }

    private static string Escape(string s) => s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
}
