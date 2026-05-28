namespace GameshowPro.BgRaster.Rendering;

static partial class FontManager
{
    private static readonly SKTypeface s_typeface = LoadEmbeddedFont();

    internal static SKTypeface Typeface => s_typeface;

    static SKTypeface LoadEmbeddedFont()
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        // Resource logical names can vary depending on project layout/linking,
        // so resolve by suffix rather than hard-coding a full name.
        string? resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Gidolinya-Regular.otf", StringComparison.OrdinalIgnoreCase));

        if (resourceName is not null)
        {
            using Stream? stream = asm.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                SKTypeface? embedded = SKTypeface.FromStream(stream);
                if (embedded is not null)
                    return embedded;
            }
        }

        // Fallback keeps rendering functional even if embedded font loading fails.
        return SKTypeface.FromFamilyName("Segoe UI") ?? SKTypeface.Default;
    }
}
