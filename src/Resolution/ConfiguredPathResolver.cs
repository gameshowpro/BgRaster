namespace GameshowPro.BgRaster.Resolution;

static class ConfiguredPathResolver
{
    internal static ImmutableArray<string>? ResolveArray(
        ImmutableArray<string>? values,
        string baseDirectory,
        SubstitutionContext? substitutionContext = null)
    {
        if (values is null)
            return null;

        return [.. values.Value.Select(value => Resolve(value, baseDirectory, substitutionContext))];
    }

    internal static string Resolve(
        string? value,
        string baseDirectory,
        SubstitutionContext? substitutionContext = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        string substituted = substitutionContext is null
            ? value
            : FieldSubstitutor.Substitute(value, substitutionContext);

        string expanded = Environment.ExpandEnvironmentVariables(substituted);
        if (string.IsNullOrWhiteSpace(expanded))
            return expanded;

        if (Uri.TryCreate(expanded, UriKind.Absolute, out Uri? _))
            return expanded;

        if (Path.IsPathRooted(expanded))
            return Path.GetFullPath(expanded);

        return Path.GetFullPath(Path.Combine(baseDirectory, expanded));
    }
}