namespace GameshowPro.BgRaster.FileLifecycle;

static class FileNamer
{
    private static readonly System.Text.RegularExpressions.Regex s_bgRasterPattern =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}\.\d{2}_.+\.png$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex s_tokenPattern =
        new(@"\{([^{}]+)\}", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly FrozenSet<char> s_invalidFileNameChars =
        Path.GetInvalidFileNameChars().ToFrozenSet();

    internal sealed record RenderOutputPathResult(string FilePath, ImmutableArray<string> Warnings);

    internal static string GetOutputTemplate(string? overrideTemplate)
    {
        string defaultTemplate = Path.Combine(Path.GetTempPath(), "BgRaster", "{now}_{index}");

        if (string.IsNullOrWhiteSpace(overrideTemplate))
            return defaultTemplate;

        string expanded = Environment.ExpandEnvironmentVariables(overrideTemplate);
        if (expanded.EndsWith(Path.DirectorySeparatorChar) || expanded.EndsWith(Path.AltDirectorySeparatorChar))
            return Path.Combine(expanded, "{now}_{index}");

        return expanded;
    }

    internal static string GetOutputDirectory(string outputTemplate)
    {
        string expanded = Environment.ExpandEnvironmentVariables(outputTemplate);
        string? directory = Path.GetDirectoryName(expanded);
        return string.IsNullOrWhiteSpace(directory) ? "." : directory;
    }

    internal static bool ContainsToken(string outputTemplate, string token) =>
        outputTemplate.IndexOf($"{{{token}}}", StringComparison.OrdinalIgnoreCase) >= 0;

    internal static RenderOutputPathResult ResolveRenderOutputPath(string outputTemplate, OutputRecord output, string? configuredMachineName = null)
    {
        string machineName = string.IsNullOrWhiteSpace(configuredMachineName) ? Environment.MachineName : configuredMachineName;

        SubstitutionContext substitutionContext = new(
            MachineName: machineName,
            OutputWidth: output.WidthPx,
            OutputHeight: output.HeightPx,
            OutputIndex: output.Index,
            OutputName: output.FriendlyName,
            SliceWidth: output.WidthPx,
            SliceHeight: output.HeightPx);

        string resolvedTemplate = ConfiguredPathResolver.Resolve(outputTemplate, Directory.GetCurrentDirectory(), substitutionContext);
        List<string> warnings = [];

        string substituted = s_tokenPattern.Replace(resolvedTemplate, match =>
        {
            string token = match.Groups[1].Value;
            return ResolveToken(token, output, warnings);
        });

        string? directory = Path.GetDirectoryName(substituted);
        if (string.IsNullOrWhiteSpace(directory))
            directory = ".";

        string fileStem = Path.GetFileName(substituted);
        string safeFileStem = SanitizeId(fileStem);
        if (string.IsNullOrWhiteSpace(safeFileStem))
            safeFileStem = "output";

        string filePath = Path.Combine(directory, safeFileStem + ".png");
        return new RenderOutputPathResult(filePath, [.. warnings]);

        static string ResolveToken(string token, OutputRecord output, List<string> warnings)
        {
            return token.ToLowerInvariant() switch
            {
                "now" => DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss.ff", System.Globalization.CultureInfo.InvariantCulture),
                "index" => output.Index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "friendlyname" => output.FriendlyName,
                _ => UnknownToken(token, warnings),
            };
        }

        static string UnknownToken(string token, List<string> warnings)
        {
            warnings.Add($"Unknown render-output token '{{{token}}}' was resolved to an empty string.");
            return string.Empty;
        }
    }

    internal static bool IsBgRasterFile(string fileName) =>
        s_bgRasterPattern.IsMatch(Path.GetFileName(fileName));

    static string SanitizeId(string id)
    {
        string trimmed = id.Trim();
        Span<char> buffer = stackalloc char[Math.Min(trimmed.Length, 48)];
        int len = 0;
        foreach (char c in trimmed)
        {
            if (len >= 48) break;

            // Keep user-friendly characters (including '-') and replace only invalid filename chars.
            buffer[len++] = s_invalidFileNameChars.Contains(c) ? '_' : c;
        }

        while (len > 0 && (buffer[len - 1] == ' ' || buffer[len - 1] == '.'))
            len--;

        return new string(buffer[..len]);
    }
}
