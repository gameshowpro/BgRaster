// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Hashing;

internal static class SettingsHasher
{
    internal static string Compute(GlobalOptions options)
    {
        StringBuilder sb = new();
        WriteTextOptions(sb, options.Text);
        WriteBackgroundOptions(sb, options.Background);
        WriteGridOptions(sb, options.Grid);
        WriteCircleOptions(sb, options.Circle);
        WriteCrosshairOptions(sb, options.Crosshair);
        WriteLogoOptions(sb, options.Logo);
        WriteRenderOptions(sb, options.Render);
        WriteOutputs(sb, options.Outputs);

        byte[] utf8 = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] hash = SHA256.HashData(utf8);
        return Convert.ToHexStringLower(hash);
    }

    private static void WriteTextOptions(StringBuilder sb, TextOptions t)
    {
        WriteStringArray(sb, "text.format", t.Format);
        WriteStringArray(sb, "text.size", t.Size);
        WriteStringArray(sb, "text.color", t.Color);
        WriteStringArray(sb, "text.x", t.X);
        WriteStringArray(sb, "text.y", t.Y);
    }

    private static void WriteBackgroundOptions(StringBuilder sb, BackgroundOptions b)
    {
        WriteStringArray(sb, "background.color", b.Color);
        WriteStringArray(sb, "background.image", b.Image);
        WriteStringArray(sb, "background.fit", b.Fit);
        WriteBoolArray(sb, "background.alternating", b.Alternating);
        WriteBoolArray(sb, "background.border", b.Border);
        WriteStringArray(sb, "background.border-color", b.BorderColor);
    }

    private static void WriteGridOptions(StringBuilder sb, GridOptions g)
    {
        WriteStringArray(sb, "grid.size", g.Size);
        WriteStringArray(sb, "grid.odd-color", g.OddColor);
        WriteStringArray(sb, "grid.even-color", g.EvenColor);
        WriteStringArray(sb, "grid.stroke", g.Stroke);
        WriteStringArray(sb, "grid.offset-x", g.OffsetX);
        WriteStringArray(sb, "grid.offset-y", g.OffsetY);
        WriteBoolArray(sb, "grid.coordinates", g.Coordinates);
    }

    private static void WriteCircleOptions(StringBuilder sb, CircleOptions c)
    {
        WriteStringArray(sb, "circle.size", c.Size);
        WriteStringArray(sb, "circle.color", c.Color);
        WriteStringArray(sb, "circle.stroke", c.Stroke);
    }

    private static void WriteCrosshairOptions(StringBuilder sb, CrosshairOptions c)
    {
        WriteStringArray(sb, "crosshair.length", c.Length);
        WriteStringArray(sb, "crosshair.color", c.Color);
        WriteStringArray(sb, "crosshair.stroke", c.Stroke);
    }

    private static void WriteLogoOptions(StringBuilder sb, LogoOptions l)
    {
        WriteStringArray(sb, "logo.source", l.Source);
        WriteStringArray(sb, "logo.x", l.X);
        WriteStringArray(sb, "logo.y", l.Y);
        WriteStringArray(sb, "logo.width", l.Width);
        WriteStringArray(sb, "logo.height", l.Height);
        WriteFloatArray(sb, "logo.opacity", l.Opacity);
    }

    private static void WriteRenderOptions(StringBuilder sb, RenderOptions r)
    {
        _ = sb.Append("render.no-assignment=").Append(r.DryRun).Append('\n');
        _ = sb.Append("render.outputs-skip-unspecified=").Append(r.OutputsSkipUnspecified).Append('\n');
        _ = sb.Append("render.output=").Append(r.Output).Append('\n');
        _ = sb.Append("render.force=").Append(r.ContinueAfterUnchanged).Append('\n');
        string verbosity = r.MinimumLogLevel switch
        {
            LogLevel.Warning => "quiet",
            LogLevel.Debug or LogLevel.Trace => "verbose",
            _ => "normal",
        };
        _ = sb.Append("render.verbosity=").Append(verbosity).Append('\n');
    }

    private static void WriteOutputs(StringBuilder sb, ImmutableArray<OutputOptions> outputs)
    {
        int i = 0;
        foreach (OutputOptions o in outputs)
        {
            string prefix = $"output[{i}]";
            string target = o.Target switch
            {
                OutputTarget.IndexTarget(int idx) => $"index:{idx}",
                OutputTarget.IdTarget(string id) => $"id:{id}",
                _ => "unknown",
            };
            _ = sb.Append(prefix).Append(".target=").Append(target).Append('\n');

            if (o.Text is TextOverride text)
            {
                WriteNullableArray(sb, $"{prefix}.text.format", text.Format);
                WriteNullableArray(sb, $"{prefix}.text.size", text.Size);
                WriteNullableArray(sb, $"{prefix}.text.color", text.Color);
                WriteNullable(sb, $"{prefix}.text.x", text.X);
                WriteNullable(sb, $"{prefix}.text.y", text.Y);
            }
            if (o.Background is BackgroundOverride bg)
            {
                WriteNullable(sb, $"{prefix}.background.color", bg.Color);
                WriteNullable(sb, $"{prefix}.background.image", bg.Image);
                WriteNullable(sb, $"{prefix}.background.fit", bg.Fit);
                WriteNullableBool(sb, $"{prefix}.background.alternating", bg.Alternating);
                WriteNullableBool(sb, $"{prefix}.background.border", bg.Border);
                WriteNullable(sb, $"{prefix}.background.border-color", bg.BorderColor);
            }
            if (o.Grid is GridOverride grid)
            {
                WriteNullable(sb, $"{prefix}.grid.size", grid.Size);
                WriteNullable(sb, $"{prefix}.grid.odd-color", grid.OddColor);
                WriteNullable(sb, $"{prefix}.grid.even-color", grid.EvenColor);
                WriteNullable(sb, $"{prefix}.grid.stroke", grid.Stroke);
                WriteNullable(sb, $"{prefix}.grid.offset-x", grid.OffsetX);
                WriteNullable(sb, $"{prefix}.grid.offset-y", grid.OffsetY);
                WriteNullableBool(sb, $"{prefix}.grid.coordinates", grid.Coordinates);
            }
            if (o.Circle is CircleOverride circle)
            {
                WriteNullable(sb, $"{prefix}.circle.size", circle.Size);
                WriteNullable(sb, $"{prefix}.circle.color", circle.Color);
                WriteNullable(sb, $"{prefix}.circle.stroke", circle.Stroke);
            }
            if (o.Crosshair is CrosshairOverride crosshair)
            {
                WriteNullable(sb, $"{prefix}.crosshair.length", crosshair.Length);
                WriteNullable(sb, $"{prefix}.crosshair.color", crosshair.Color);
                WriteNullable(sb, $"{prefix}.crosshair.stroke", crosshair.Stroke);
            }
            if (o.Logo is LogoOverride logo)
            {
                WriteNullable(sb, $"{prefix}.logo.source", logo.Source);
                WriteNullable(sb, $"{prefix}.logo.x", logo.X);
                WriteNullable(sb, $"{prefix}.logo.y", logo.Y);
                WriteNullable(sb, $"{prefix}.logo.width", logo.Width);
                WriteNullable(sb, $"{prefix}.logo.height", logo.Height);
                WriteNullableFloat(sb, $"{prefix}.logo.opacity", logo.Opacity);
            }
            WriteSlices(sb, prefix, o.Slices);
            i++;
        }
    }

    private static void WriteSlices(StringBuilder sb, string prefix, ImmutableArray<SliceOptions> slices)
    {
        int j = 0;
        foreach (SliceOptions s in slices)
        {
            string sp = $"{prefix}.slice[{j}]";
            _ = sb.Append(sp).Append(".x=").Append(s.X).Append('\n');
            _ = sb.Append(sp).Append(".y=").Append(s.Y).Append('\n');
            _ = sb.Append(sp).Append(".width=").Append(s.Width).Append('\n');
            _ = sb.Append(sp).Append(".height=").Append(s.Height).Append('\n');
            j++;
        }
    }

    private static void WriteStringArray(StringBuilder sb, string key, ImmutableArray<string> values)
    {
        _ = sb.Append(key).Append("=[");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(',');
            }

            _ = sb.Append(values[i]);
        }
        _ = sb.Append("]\n");
    }

    private static void WriteBoolArray(StringBuilder sb, string key, ImmutableArray<bool> values)
    {
        _ = sb.Append(key).Append("=[");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(',');
            }

            _ = sb.Append(values[i]);
        }
        _ = sb.Append("]\n");
    }

    private static void WriteFloatArray(StringBuilder sb, string key, ImmutableArray<float> values)
    {
        _ = sb.Append(key).Append("=[");
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(',');
            }

            _ = sb.Append(values[i].ToString(CultureInfo.InvariantCulture));
        }
        _ = sb.Append("]\n");
    }

    private static void WriteNullable(StringBuilder sb, string key, string? value)
    {
        if (value is not null)
        {
            _ = sb.Append(key).Append('=').Append(value).Append('\n');
        }
    }

    private static void WriteNullableArray(StringBuilder sb, string key, ImmutableArray<string>? values)
    {
        if (values is not { Length: > 0 })
        {
            return;
        }

        WriteStringArray(sb, key, values.Value);
    }

    private static void WriteNullableBool(StringBuilder sb, string key, bool? value)
    {
        if (value.HasValue)
        {
            _ = sb.Append(key).Append('=').Append(value.Value).Append('\n');
        }
    }

    private static void WriteNullableFloat(StringBuilder sb, string key, float? value)
    {
        if (value.HasValue)
        {
            _ = sb.Append(key).Append('=').Append(value.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
    }
}
