// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

internal static partial class FontManager
{
    internal static SKTypeface Typeface { get; } = LoadEmbeddedFont();

    private static SKTypeface LoadEmbeddedFont()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        string? resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Gidolinya-Regular.otf", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            string available = string.Join(", ", asm.GetManifestResourceNames().OrderBy(n => n, StringComparer.Ordinal));
            throw new InvalidOperationException(
                $"Embedded font resource 'Gidolinya-Regular.otf' was not found. Available resources: [{available}]");
        }

        using Stream stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded font resource '{resourceName}' could not be opened.");

        byte[] fontBytes = new byte[stream.Length];
        stream.ReadExactly(fontBytes);

        using SKData data = SKData.CreateCopy(fontBytes);
        return SKTypeface.FromData(data)
            ?? throw new InvalidOperationException(
                $"Embedded font resource '{resourceName}' could not be loaded as a typeface.");
    }
}