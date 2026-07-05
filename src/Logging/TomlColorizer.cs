// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Logging;

/// <summary>
/// Colorizes effective-config TOML output with provenance-based coloring.
/// Uses ANSI escape codes (Windows 10 1903+ native support).
///
/// Color rules per key=value line:
///   Field name (key) = color of LOWEST-priority source that actively set it
///   Value            = color of HIGHEST-priority source that actively set it
///
/// Source priority (low &#x2192; high): Default (dim) &#x2192; TOML (cyan) &#x2192; CLI (yellow)
///
/// Matrix:
///   CLI set? TOML set?  | Field name          | Value
///   &#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;&#x2014;
///   &#x2713;        &#x2713;         | cyan (TOML)         | yellow bold (CLI)
///   &#x2713;        -           | yellow bold (CLI)   | yellow bold (CLI)
///   -          &#x2713;         | cyan (TOML)         | cyan (TOML)
///   -          -           | dim (Default)       | dim (Default)
///
/// Section headers: bold cyan. Comments: dim.
/// </summary>
static class TomlColorizer
{
    const string Reset = "\x1b[0m";
    const string Dim = "\x1b[2m";
    const string Cyan = "\x1b[36m";
    const string Yellow = "\x1b[33m";
    const string Bold = "\x1b[1m";

    /// <summary>
    /// Colorize effective TOML with provenance tracking.
    /// </summary>
    /// <param name="effectiveToml">The full effective-config TOML text.</param>
    /// <param name="tomlPaths">Paths explicitly set in the TOML config file.</param>
    /// <param name="cliPaths">Paths overridden via CLI arguments.</param>
    internal static string ColorizeProvenance(
        string effectiveToml,
        HashSet<string> tomlPaths,
        HashSet<string> cliPaths)
    {
        StringBuilder sb = new();
        string currentSection = "";
        int outputIndex = -1;
        int sliceIndex = -1;
        string subSection = "";

        // Header with legend
        sb.Append(Bold);
        sb.Append("### Effective configuration (");
        sb.Append(Reset);

        sb.Append(Dim); sb.Append(Bold); sb.Append("Defaults"); sb.Append(Reset);
        sb.Append(" \u2192 ");
        sb.Append(Cyan); sb.Append("TOML"); sb.Append(Reset);
        sb.Append(" \u2192 ");
        sb.Append(Yellow); sb.Append(Bold); sb.Append("CLI"); sb.Append(Reset);

        sb.Append(Bold);
        sb.Append(')');
        sb.Append(Reset);
        sb.Append('\n');

        using StringReader reader = new(effectiveToml);
        while (reader.ReadLine() is string line)
        {
            string trimmed = line.TrimStart();
            bool isComment = trimmed.StartsWith('#');

            if (string.IsNullOrWhiteSpace(line))
            {
                sb.Append('\n');
                continue;
            }

            if (isComment)
            {
                sb.Append(Dim);
                sb.Append(line);
                sb.Append(Reset);
                sb.Append('\n');
                continue;
            }

            if (trimmed.StartsWith('['))
            {
                ParseHeader(trimmed, ref currentSection, ref outputIndex, ref sliceIndex, ref subSection);
                sb.Append(Bold);
                sb.Append(Cyan);
                sb.Append(line);
                sb.Append(Reset);
                sb.Append('\n');
                continue;
            }

            int eq = line.IndexOf('=');
            if (eq < 0)
            {
                sb.Append(line);
                sb.Append('\n');
                continue;
            }

            string key = line[..eq].TrimEnd();
            string value = line[(eq + 1)..];
            string path = BuildPath(currentSection, outputIndex, sliceIndex, subSection, key);

            bool inToml = tomlPaths.Contains(path);
            bool inCli = cliPaths.Contains(path);

            (string? keyColor, string? valColor) = ResolveColors(inToml, inCli);

            string indent = line[..(line.Length - trimmed.Length)];
            sb.Append(indent);

            // Field name
            if (keyColor is not null)
            {
                sb.Append(keyColor);
                sb.Append(Bold);
                sb.Append(key);
                sb.Append(Reset);
            }
            else
            {
                sb.Append(Bold);
                sb.Append(key);
                sb.Append(Reset);
            }

            sb.Append(" = ");

            // Value
            if (valColor is not null)
            {
                sb.Append(valColor);
                sb.Append(value.TrimStart());
                sb.Append(Reset);
            }
            else
            {
                sb.Append(value.TrimStart());
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }

    static (string? keyColor, string? valColor) ResolveColors(bool inToml, bool inCli)
    {
        if (inCli && inToml)
            return (Cyan, Bold + Yellow);
        if (inCli)
            return (Bold + Yellow, Bold + Yellow);
        if (inToml)
            return (Cyan, Cyan);

        // Default only - both field name and value are dim
        return (Dim, Dim);
    }

    static string BuildPath(string section, int outputIdx, int sliceIdx, string subSection, string key)
    {
        if (outputIdx >= 0)
        {
            if (sliceIdx >= 0)
            {
                if (!string.IsNullOrEmpty(subSection))
                    return $"output[{outputIdx}].slice[{sliceIdx}].{subSection}.{key}";
                return $"output[{outputIdx}].slice[{sliceIdx}].{key}";
            }
            if (!string.IsNullOrEmpty(subSection))
                return $"output[{outputIdx}].{subSection}.{key}";
            return $"output[{outputIdx}].{key}";
        }
        return $"{section}.{key}";
    }

    static void ParseHeader(string trimmed, ref string section, ref int outputIdx, ref int sliceIdx, ref string subSection)
    {
        if (trimmed.StartsWith("[["))
        {
            string inner = trimmed.TrimStart('[').TrimEnd(']').Trim();
            if (inner.StartsWith("output.slice", StringComparison.Ordinal))
            {
                sliceIdx++;
                subSection = "";
                return;
            }
            if (inner == "output")
            {
                outputIdx++;
                sliceIdx = -1;
                subSection = "";
                return;
            }
            return;
        }

        string name = trimmed.TrimStart('[').TrimEnd(']').Trim();

        int dot = name.IndexOf('.');
        if (dot > 0)
        {
            string parent = name[..dot];
            string child = name[(dot + 1)..];

            if (parent == "output.slice" && sliceIdx >= 0)
            {
                subSection = child;
                return;
            }
            if (parent == "output" && outputIdx >= 0)
            {
                subSection = child;
                return;
            }
        }

        section = name;
        outputIdx = -1;
        sliceIdx = -1;
        subSection = "";
    }
}
