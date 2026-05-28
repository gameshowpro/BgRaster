namespace GameshowPro.BgRaster.FileLifecycle;

static class FileNamer
{
    private static readonly System.Text.RegularExpressions.Regex s_bgRasterPattern =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}\.\d+Z_.+\.png$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    internal static string GenerateFileName(string outputId)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.fffffffZ",
            System.Globalization.CultureInfo.InvariantCulture);
        string safeId = SanitizeId(outputId);
        return $"{timestamp}_{safeId}.png";
    }

    internal static string GetOutputDirectory(string? overrideDir) =>
        string.IsNullOrEmpty(overrideDir)
            ? Path.Combine(Path.GetTempPath(), "BgRaster")
            : overrideDir;

    internal static bool IsBgRasterFile(string fileName) =>
        s_bgRasterPattern.IsMatch(Path.GetFileName(fileName));

    static string SanitizeId(string id)
    {
        Span<char> buffer = stackalloc char[Math.Min(id.Length, 48)];
        int len = 0;
        foreach (char c in id)
        {
            if (len >= 48) break;
            buffer[len++] = char.IsLetterOrDigit(c) || c == '_' ? c : '_';
        }
        return new string(buffer[..len]);
    }
}
