// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Parsing;

static class ColorParser
{
    internal static SKColor Parse(string input)
    {
        if (TryParse(input, out SKColor color))
            return color;
        throw new FormatException($"Cannot parse '{input}' as a color.");
    }

    internal static bool TryParse(string input, out SKColor color)
    {
        ReadOnlySpan<char> span = input.AsSpan().Trim();

        if (span.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            color = SKColors.Transparent;
            return true;
        }

        if (span.StartsWith("#"))
            return TryParseHex(span[1..], out color);

        if (span.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && span.EndsWith(")"))
            return TryParseRgba(span[5..^1], out color);

        if (span.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && span.EndsWith(")"))
            return TryParseRgb(span[4..^1], out color);

        if (span.StartsWith("hsla(", StringComparison.OrdinalIgnoreCase) && span.EndsWith(")"))
            return TryParseHsla(span[5..^1], out color);

        if (span.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase) && span.EndsWith(")"))
            return TryParseHsl(span[4..^1], out color);

        color = default;
        return false;
    }

    static bool TryParseHex(ReadOnlySpan<char> hex, out SKColor color)
    {
        if (hex.Length == 3)
        {
            if (TryParseShorthandHexByte(hex[0], out byte r) &&
                TryParseShorthandHexByte(hex[1], out byte g) &&
                TryParseShorthandHexByte(hex[2], out byte b))
            {
                color = new SKColor(r, g, b);
                return true;
            }
        }
        else if (hex.Length == 4)
        {
            if (TryParseShorthandHexByte(hex[0], out byte r) &&
                TryParseShorthandHexByte(hex[1], out byte g) &&
                TryParseShorthandHexByte(hex[2], out byte b) &&
                TryParseShorthandHexByte(hex[3], out byte a))
            {
                color = new SKColor(r, g, b, a);
                return true;
            }
        }
        else if (hex.Length == 6)
        {
            if (TryParseHexByte(hex[0..2], out byte r) &&
                TryParseHexByte(hex[2..4], out byte g) &&
                TryParseHexByte(hex[4..6], out byte b))
            {
                color = new SKColor(r, g, b);
                return true;
            }
        }
        else if (hex.Length == 8)
        {
            if (TryParseHexByte(hex[0..2], out byte r) &&
                TryParseHexByte(hex[2..4], out byte g) &&
                TryParseHexByte(hex[4..6], out byte b) &&
                TryParseHexByte(hex[6..8], out byte a))
            {
                color = new SKColor(r, g, b, a);
                return true;
            }
        }
        color = default;
        return false;
    }

    static bool TryParseShorthandHexByte(char hex, out byte value)
    {
        Span<char> expanded = [hex, hex];
        return TryParseHexByte(expanded, out value);
    }

    static bool TryParseHexByte(ReadOnlySpan<char> hex, out byte value)
    {
        if (byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;
        value = 0;
        return false;
    }

    static bool TryParseRgb(ReadOnlySpan<char> inner, out SKColor color)
    {
        Span<Range> parts = stackalloc Range[3];
        int count = inner.Split(parts, ',');
        if (count == 3 &&
            TryParseFloat(inner[parts[0]], out float r) &&
            TryParseFloat(inner[parts[1]], out float g) &&
            TryParseFloat(inner[parts[2]], out float b))
        {
            color = new SKColor(ClampByte(r), ClampByte(g), ClampByte(b));
            return true;
        }
        color = default;
        return false;
    }

    static bool TryParseRgba(ReadOnlySpan<char> inner, out SKColor color)
    {
        Span<Range> parts = stackalloc Range[4];
        int count = inner.Split(parts, ',');
        if (count == 4 &&
            TryParseFloat(inner[parts[0]], out float r) &&
            TryParseFloat(inner[parts[1]], out float g) &&
            TryParseFloat(inner[parts[2]], out float b) &&
            TryParseFloat(inner[parts[3]], out float a))
        {
            color = new SKColor(ClampByte(r), ClampByte(g), ClampByte(b), (byte)(Math.Clamp(a, 0f, 1f) * 255f));
            return true;
        }
        color = default;
        return false;
    }

    static bool TryParseHsl(ReadOnlySpan<char> inner, out SKColor color)
    {
        Span<Range> parts = stackalloc Range[3];
        int count = inner.Split(parts, ',');
        if (count == 3 &&
            TryParseFloat(inner[parts[0]], out float h) &&
            TryParsePercentage(inner[parts[1]], out float s) &&
            TryParsePercentage(inner[parts[2]], out float l))
        {
            (byte r, byte g, byte b) = HslToRgb(h, s, l);
            color = new SKColor(r, g, b);
            return true;
        }
        color = default;
        return false;
    }

    static bool TryParseHsla(ReadOnlySpan<char> inner, out SKColor color)
    {
        Span<Range> parts = stackalloc Range[4];
        int count = inner.Split(parts, ',');
        if (count == 4 &&
            TryParseFloat(inner[parts[0]], out float h) &&
            TryParsePercentage(inner[parts[1]], out float s) &&
            TryParsePercentage(inner[parts[2]], out float l) &&
            TryParseFloat(inner[parts[3]], out float a))
        {
            (byte r, byte g, byte b) = HslToRgb(h, s, l);
            color = new SKColor(r, g, b, (byte)(Math.Clamp(a, 0f, 1f) * 255f));
            return true;
        }
        color = default;
        return false;
    }

    static bool TryParseFloat(ReadOnlySpan<char> span, out float value) =>
        float.TryParse(span.Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out value);

    static bool TryParsePercentage(ReadOnlySpan<char> span, out float value)
    {
        ReadOnlySpan<char> trimmed = span.Trim();
        if (trimmed.EndsWith("%"))
        {
            if (TryParseFloat(trimmed[..^1], out value))
            {
                value /= 100f;
                return true;
            }
        }
        else if (TryParseFloat(trimmed, out value))
        {
            return true;
        }
        value = 0;
        return false;
    }

    static byte ClampByte(float value) => (byte)Math.Clamp((int)MathF.Round(value), 0, 255);

    static (byte R, byte G, byte B) HslToRgb(float h, float s, float l)
    {
        if (s == 0f)
        {
            byte grey = ClampByte(l * 255f);
            return (grey, grey, grey);
        }

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;
        float hNorm = ((h % 360f) + 360f) % 360f / 360f;

        return (
            ClampByte(HueToRgb(p, q, hNorm + 1f / 3f) * 255f),
            ClampByte(HueToRgb(p, q, hNorm) * 255f),
            ClampByte(HueToRgb(p, q, hNorm - 1f / 3f) * 255f)
        );
    }

    static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}
