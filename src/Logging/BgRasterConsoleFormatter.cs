// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Logging;

using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Compact console formatter that outputs "{level} {eventId}: {message}"
/// with the level name colored (background color, black foreground).
/// Key=value pairs in the message body are syntax-highlighted:
///   strings (quoted) = green, numbers = cyan, booleans = magenta.
/// Multi-line messages are left-aligned (no continuation indent).
/// </summary>
sealed class BgRasterConsoleFormatter : ConsoleFormatter
{
    const string Reset = "\x1b[0m";
    const string Dim = "\x1b[2m";
    const string BlackFg = "\x1b[30m";
    const string RedBg = "\x1b[41m";
    const string YellowBg = "\x1b[43m";
    const string BlueBg = "\x1b[44m";
    const string DarkGrayBg = "\x1b[100m";
    const string GreenFg = "\x1b[32m";
    const string CyanFg = "\x1b[36m";
    const string MagentaFg = "\x1b[35m";

    public BgRasterConsoleFormatter() : base("BgRaster") { }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string level;
        string color;
        switch (logEntry.LogLevel)
        {
            case LogLevel.Error:
            case LogLevel.Critical:
                level = "err ";
                color = RedBg + BlackFg;
                break;
            case LogLevel.Warning:
                level = "warn";
                color = YellowBg + BlackFg;
                break;
            case LogLevel.Debug:
            case LogLevel.Trace:
                level = "dbug";
                color = DarkGrayBg + BlackFg;
                break;
            default:
                level = "info";
                color = BlueBg + BlackFg;
                break;
        }

        // "info 25: message" with level name colored bg + black fg
        textWriter.Write(color);
        textWriter.Write(level);
        textWriter.Write(' ');
        textWriter.Write(logEntry.EventId.Id.ToString().PadLeft(2));
        textWriter.Write(": ");
        textWriter.Write(Reset);

        string message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception) ?? logEntry.State?.ToString() ?? "";

        // Syntax-highlight key=value pairs if the message follows the structured pattern
        if (message.StartsWith("# bg-raster:", StringComparison.Ordinal))
        {
            WriteColorizedKeyValues(textWriter, message);
        }
        else
        {
            textWriter.WriteLine(message);
        }
    }

    static void WriteColorizedKeyValues(TextWriter writer, string message)
    {
        // "# bg-raster:" prefix in dim
        writer.Write(Dim);
        writer.Write("# bg-raster:");
        writer.Write(Reset);

        // Parse the rest as space-separated key=value pairs
        ReadOnlySpan<char> rest = message.AsSpan("# bg-raster:".Length);

        while (rest.Length > 0)
        {
            // Skip leading spaces
            rest = rest.TrimStart(' ');

            int eqIdx = rest.IndexOf('=');
            if (eqIdx < 0)
            {
                // No '=' — write remaining text as-is
                if (rest.Length > 0)
                    writer.Write(rest.ToString());
                break;
            }

            // Find end of key (backtrack from '=' past spaces)
            ReadOnlySpan<char> keySpan = rest[..eqIdx].TrimEnd();

            // Write space before key
            writer.Write(' ');

            // Key in dim
            writer.Write(Dim);
            writer.Write(keySpan.ToString());
            writer.Write(Reset);

            // '=' in default
            writer.Write('=');

            // Find value — scan forward from after '='
            int valStart = eqIdx + 1;
            rest = rest[valStart..];

            if (rest.Length == 0 || rest[0] == ' ')
            {
                // Empty value or next pair — continue
                continue;
            }

            // Determine if value is quoted string, number, bool, or identifier
            if (rest[0] == '"')
            {
                // Quoted string — find closing quote
                int closeQuote = rest[1..].IndexOf('"') + 1;
                if (closeQuote > 1)
                {
                    writer.Write(GreenFg);
                    writer.Write(rest[..(closeQuote + 1)].ToString());
                    writer.Write(Reset);
                    rest = rest[(closeQuote + 1)..];
                }
                else
                {
                    // Malformed — write rest
                    writer.Write(rest.ToString());
                    rest = default;
                }
            }
            else
            {
                // Find end of value token (space or end)
                int spaceIdx = rest.IndexOf(' ');
                ReadOnlySpan<char> valueSpan = spaceIdx >= 0 ? rest[..spaceIdx] : rest;
                rest = spaceIdx >= 0 ? rest[spaceIdx..] : default;

                string val = valueSpan.ToString();
                if (val == "true" || val == "false")
                {
                    writer.Write(MagentaFg);
                    writer.Write(val);
                    writer.Write(Reset);
                }
                else if (val.Length > 0 && (char.IsDigit(val[0]) || (val[0] == '-' && val.Length > 1 && char.IsDigit(val[1]))))
                {
                    writer.Write(CyanFg);
                    writer.Write(val);
                    writer.Write(Reset);
                }
                else
                {
                    // Unquoted identifier — default color
                    writer.Write(val);
                }
            }
        }

        writer.WriteLine();
    }
}
