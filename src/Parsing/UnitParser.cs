// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Parsing;

static class UnitParser
{
    internal static UnitValue Parse(string input)
    {
        ReadOnlySpan<char> span = input.AsSpan().Trim();

        if (span.IsEmpty || span.Equals("0", StringComparison.Ordinal))
            return new UnitValue(0f, DimensionUnit.Px);

        // Check suffixes longest-first to avoid "vw" matching inside "vmin".
        if (TryStripSuffix(span, "vmin", out ReadOnlySpan<char> rest))
            return new UnitValue(ParseFloat(rest, input), DimensionUnit.Vmin);
        if (TryStripSuffix(span, "vmax", out rest))
            return new UnitValue(ParseFloat(rest, input), DimensionUnit.Vmax);
        if (TryStripSuffix(span, "vh", out rest))
            return new UnitValue(ParseFloat(rest, input), DimensionUnit.Vh);
        if (TryStripSuffix(span, "vw", out rest))
            return new UnitValue(ParseFloat(rest, input), DimensionUnit.Vw);
        if (TryStripSuffix(span, "px", out rest))
            return new UnitValue(ParseFloat(rest, input), DimensionUnit.Px);

        return new UnitValue(ParseFloat(span, input), DimensionUnit.Px);
    }

    static bool TryStripSuffix(ReadOnlySpan<char> span, string suffix, out ReadOnlySpan<char> remainder)
    {
        if (span.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            remainder = span[..^suffix.Length].TrimEnd();
            return true;
        }
        remainder = default;
        return false;
    }

    static float ParseFloat(ReadOnlySpan<char> span, string original)
    {
        if (float.TryParse(span, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float value))
            return value;
        throw new FormatException($"Cannot parse '{original}' as a dimension value.");
    }
}
