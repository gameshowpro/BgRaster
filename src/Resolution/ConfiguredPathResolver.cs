// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Resolution;

static class ConfiguredPathResolver
{
    static readonly StringComparison s_pathComparison = StringComparison.OrdinalIgnoreCase;

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
                    : FieldSubstitutor.Substitute(value, substitutionContext)
                        .Replace("\0NETWORK\0", "");

        return NormalizeExpandedPath(substituted, baseDirectory);
    }

    internal static string NormalizeExpandedPath(string? value, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        string expanded = Environment.ExpandEnvironmentVariables(value);
        if (string.IsNullOrWhiteSpace(expanded))
            return expanded;

        if (expanded.StartsWith("pack://application:,,,/", s_pathComparison))
            return expanded;

        if (Uri.TryCreate(expanded, UriKind.Absolute, out Uri? _))
            return expanded;

        if (Path.IsPathRooted(expanded))
            return Path.GetFullPath(expanded);

        return Path.GetFullPath(Path.Combine(baseDirectory, expanded));
    }
}