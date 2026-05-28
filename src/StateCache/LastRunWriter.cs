namespace GameshowPro.BgRaster.StateCache;

static class LastRunWriter
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
        string schemaVersion,
        RunStatus? runStatus = null,
        ILogger? logger = null)
    {
        runStatus ??= new RunStatus();

        try
        {
            string directory = Path.GetDirectoryName(path) ?? ".";
            Directory.CreateDirectory(directory);

            StringBuilder sb = new();
            sb.AppendLine($"# $schema: https://raw.githubusercontent.com/gameshowpro/GameshowPro/{schemaVersion}/BgRaster/docs/schemas/bgraster-lastrun.schema.json");
            sb.AppendLine();

            sb.AppendLine("[meta]");
            sb.AppendLine($"version = \"{Escape(state.Meta.Version)}\"");
            sb.AppendLine($"settingsHash = \"{Escape(state.Meta.SettingsHash)}\"");
            sb.AppendLine($"timestamp = \"{Escape(state.Meta.Timestamp)}\"");
            WriteInlineTable(sb, "assignedFiles", state.Meta.AssignedFiles);
            WriteStringArray(sb, "unrecycledFiles", state.Meta.UnrecycledFiles);
            sb.AppendLine();

            foreach (OutputRecord hw in state.HardwareOutputs.OrderBy(o => o.Id, StringComparer.Ordinal))
            {
                if (runStatus.HardwareStatuses.TryGetValue(hw.Id, out string? status))
                {
                    sb.AppendLine($"# bg-raster: status={status} id=\"{Escape(hw.Id)}\" index={hw.Index} position={hw.DesktopX},{hw.DesktopY} resolution={hw.WidthPx}x{hw.HeightPx}");
                }
                sb.AppendLine("[[hardware_output]]");
                sb.AppendLine($"id = \"{Escape(hw.Id)}\"");
                sb.AppendLine($"index = {hw.Index}");
                sb.AppendLine($"desktopX = {hw.DesktopX}");
                sb.AppendLine($"desktopY = {hw.DesktopY}");
                sb.AppendLine($"widthPx = {hw.WidthPx}");
                sb.AppendLine($"heightPx = {hw.HeightPx}");
                sb.AppendLine($"dpiX = {hw.DpiX}");
                sb.AppendLine($"dpiY = {hw.DpiY}");
                sb.AppendLine($"rotation = {hw.Rotation}");
                sb.AppendLine($"refreshRateHz = {hw.RefreshRateHz}");
                sb.AppendLine($"adapterName = \"{Escape(hw.AdapterName)}\"");
                sb.AppendLine($"friendlyName = \"{Escape(hw.FriendlyName)}\"");
                sb.AppendLine();
            }

            WriteEffectiveConfig(sb, state.EffectiveConfig, runStatus);

            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, sb.ToString(), Encoding.UTF8);

            if (!VerifyRoundTrip(tempPath, state, logger))
            {
            logger?.RoundTripFailed(path);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            logger?.WriteFailure(ex, path);
        }
    }

    static bool VerifyRoundTrip(string path, LastRunState original, ILogger? logger)
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

    static string VerbosityToml(LogLevel level) => level switch
    {
        LogLevel.Warning => "quiet",
        LogLevel.Debug or LogLevel.Trace => "verbose",
        _ => "normal",
    };

    static bool DictionaryEqual(FrozenDictionary<string, string> a, FrozenDictionary<string, string> b)
    {
        if (a.Count != b.Count) return false;
        foreach (KeyValuePair<string, string> kv in a)
        {
            if (!b.TryGetValue(kv.Key, out string? other) || other != kv.Value) return false;
        }
        return true;
    }

    static void WriteEffectiveConfig(StringBuilder sb, GlobalOptions opts, RunStatus runStatus)
    {
        WriteTextSection(sb, opts.Text);
        WriteBackgroundSection(sb, opts.Background);
        WriteGridSection(sb, opts.Grid);
        WriteCircleSection(sb, opts.Circle);
        WriteCrosshairSection(sb, opts.Crosshair);
        WriteLogoSection(sb, opts.Logo);
        WriteRenderSection(sb, opts.Render);
        WriteOutputsSection(sb, opts.Outputs, runStatus);
    }

    static void WriteTextSection(StringBuilder sb, TextOptions t)
    {
        sb.AppendLine("[text]");
        WriteTomlStringArray(sb, "text", t.Text);
        WriteTomlStringArray(sb, "size", t.Size);
        WriteTomlStringArray(sb, "color", t.Color);
        WriteTomlStringArray(sb, "x", t.X);
        WriteTomlStringArray(sb, "y", t.Y);
        sb.AppendLine();
    }

    static void WriteBackgroundSection(StringBuilder sb, BackgroundOptions b)
    {
        sb.AppendLine("[background]");
        WriteTomlStringArray(sb, "color", b.Color);
        WriteTomlStringArray(sb, "image", b.Image);
        WriteTomlStringArray(sb, "fit", b.Fit);
        WriteTomlBoolArray(sb, "alternating", b.Alternating);
        WriteTomlBoolArray(sb, "border", b.Border);
        WriteTomlStringArray(sb, "border-color", b.BorderColor);
        sb.AppendLine();
    }

    static void WriteGridSection(StringBuilder sb, GridOptions g)
    {
        sb.AppendLine("[grid]");
        WriteTomlStringArray(sb, "size", g.Size);
        WriteTomlStringArray(sb, "odd-color", g.OddColor);
        WriteTomlStringArray(sb, "even-color", g.EvenColor);
        WriteTomlStringArray(sb, "stroke", g.Stroke);
        WriteTomlStringArray(sb, "offset-x", g.OffsetX);
        WriteTomlStringArray(sb, "offset-y", g.OffsetY);
        WriteTomlBoolArray(sb, "coordinates", g.Coordinates);
        sb.AppendLine();
    }

    static void WriteCircleSection(StringBuilder sb, CircleOptions c)
    {
        sb.AppendLine("[circle]");
        WriteTomlStringArray(sb, "size", c.Size);
        WriteTomlStringArray(sb, "color", c.Color);
        WriteTomlStringArray(sb, "stroke", c.Stroke);
        sb.AppendLine();
    }

    static void WriteCrosshairSection(StringBuilder sb, CrosshairOptions c)
    {
        sb.AppendLine("[crosshair]");
        WriteTomlStringArray(sb, "length", c.Length);
        WriteTomlStringArray(sb, "color", c.Color);
        WriteTomlStringArray(sb, "stroke", c.Stroke);
        sb.AppendLine();
    }

    static void WriteLogoSection(StringBuilder sb, LogoOptions l)
    {
        sb.AppendLine("[logo]");
        WriteTomlStringArray(sb, "source", l.Source);
        WriteTomlStringArray(sb, "x", l.X);
        WriteTomlStringArray(sb, "y", l.Y);
        WriteTomlStringArray(sb, "width", l.Width);
        WriteTomlStringArray(sb, "height", l.Height);
        WriteTomlFloatArray(sb, "opacity", l.Opacity);
        sb.AppendLine();
    }

    static void WriteRenderSection(StringBuilder sb, RenderOptions r)
    {
        sb.AppendLine("[render]");
        sb.AppendLine($"no-assignment = {r.DryRun.ToString().ToLowerInvariant()}");
        sb.AppendLine($"outputs-skip-unspecified = {r.OutputsSkipUnspecified.ToString().ToLowerInvariant()}");
        sb.AppendLine($"output = \"{Escape(r.Output)}\"");
        sb.AppendLine($"force = {r.ContinueAfterUnchanged.ToString().ToLowerInvariant()}");
        sb.AppendLine($"verbosity = \"{VerbosityToml(r.MinimumLogLevel)}\"");
        sb.AppendLine();
    }

    static void WriteOutputsSection(StringBuilder sb, ImmutableArray<OutputOptions> outputs, RunStatus runStatus)
    {
        int idx = 0;
        foreach (OutputOptions o in outputs)
        {
            string target = o.Target switch
            {
                OutputTarget.IndexTarget(int i) => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                OutputTarget.IdTarget(string id) => $"\"{Escape(id)}\"",
                _ => "0",
            };

            ConfiguredOutputStatus? cs = idx < runStatus.ConfiguredOutputs.Length
                ? runStatus.ConfiguredOutputs[idx]
                : null;
            if (cs is not null)
            {
                string reasonSuffix = string.IsNullOrEmpty(cs.Reason) ? "" : $" {cs.Reason}";
                sb.AppendLine($"# bg-raster: status={cs.Status} target={target}{reasonSuffix}");
            }

            sb.AppendLine("[[output]]");
            sb.AppendLine($"target = {target}");
            sb.AppendLine();

            int sliceIdx = 0;
            foreach (SliceOptions s in o.Slices)
            {
                if (cs is not null && sliceIdx < cs.Slices.Length)
                {
                    SliceStatus ss = cs.Slices[sliceIdx];
                    string reasonSuffix = string.IsNullOrEmpty(ss.Reason) ? "" : $" reason=\"{Escape(ss.Reason)}\"";
                    sb.AppendLine($"# bg-raster: status={ss.Status}{reasonSuffix}");
                }
                sb.AppendLine("[[output.slice]]");
                sb.AppendLine($"x = \"{Escape(s.X)}\"");
                sb.AppendLine($"y = \"{Escape(s.Y)}\"");
                sb.AppendLine($"width = \"{Escape(s.Width)}\"");
                sb.AppendLine($"height = \"{Escape(s.Height)}\"");
                sb.AppendLine();
                sliceIdx++;
            }
            idx++;
        }
    }

    static void WriteTomlStringArray(StringBuilder sb, string key, ImmutableArray<string> values)
    {
        sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('"').Append(Escape(values[i])).Append('"');
        }
        sb.AppendLine("]");
    }

    static void WriteTomlBoolArray(StringBuilder sb, string key, ImmutableArray<bool> values)
    {
        sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(values[i].ToString().ToLowerInvariant());
        }
        sb.AppendLine("]");
    }

    static void WriteTomlFloatArray(StringBuilder sb, string key, ImmutableArray<float> values)
    {
        sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(values[i].ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        sb.AppendLine("]");
    }

    static void WriteInlineTable(StringBuilder sb, string key, FrozenDictionary<string, string> dict)
    {
        sb.Append(key).Append(" = {");
        bool first = true;
        foreach (KeyValuePair<string, string> kv in dict.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (!first) sb.Append(", ");
            sb.Append('"').Append(Escape(kv.Key)).Append("\" = \"").Append(Escape(kv.Value)).Append('"');
            first = false;
        }
        sb.AppendLine("}");
    }

    static void WriteStringArray(StringBuilder sb, string key, ImmutableArray<string> values)
    {
        sb.Append(key).Append(" = [");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('"').Append(Escape(values[i])).Append('"');
        }
        sb.AppendLine("]");
    }

    static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
