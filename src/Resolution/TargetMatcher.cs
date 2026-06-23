// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Resolution;

abstract record MatchResult
{
    internal sealed record Matched(OutputRecord Output, OutputOptions Config) : MatchResult;
    internal sealed record NotFound(OutputOptions Config) : MatchResult;
    internal sealed record Duplicate(OutputOptions Config, OutputRecord AlreadyMatchedOutput) : MatchResult;
}

static class TargetMatcher
{
    internal static ImmutableArray<MatchResult> Match(
        HardwareProfile profile,
        ImmutableArray<OutputOptions> outputs,
        bool skipUnspecifiedOutputs = true)
    {
        HashSet<string> matchedIds = [];
        List<MatchResult> results = [];

        foreach (OutputOptions outputConfig in outputs)
        {
            OutputRecord? found = outputConfig.Target switch
            {
                OutputTarget.IdTarget(string id) =>
                    profile.Outputs.FirstOrDefault(o => o.Id == id),
                OutputTarget.IndexTarget(int idx) =>
                    profile.Outputs.FirstOrDefault(o => o.Index == idx),
                _ => null,
            };

            if (found is null)
            {
                results.Add(new MatchResult.NotFound(outputConfig));
                continue;
            }

            if (matchedIds.Contains(found.Id))
            {
                results.Add(new MatchResult.Duplicate(outputConfig, found));
                continue;
            }

            _ = matchedIds.Add(found.Id);
            results.Add(new MatchResult.Matched(found, outputConfig));
        }

        if (!skipUnspecifiedOutputs)
        {
            foreach (OutputRecord discovered in profile.Outputs)
            {
                if (matchedIds.Contains(discovered.Id))
                    continue;

                _ = matchedIds.Add(discovered.Id);
                results.Add(new MatchResult.Matched(
                    discovered,
                    new OutputOptions { Target = OutputTarget.FromId(discovered.Id) }));
            }
        }

        return [.. results];
    }
}
