namespace GameshowPro.BgRaster.Tests;

public class TargetMatcherTests
{
    static HardwareProfile MakeProfile(params (string Id, int Index)[] outputs) =>
        new([.. outputs.Select(o => new OutputRecord { Id = o.Id, Index = o.Index })]);

    [Fact]
    public void Match_IntegerTarget_MatchesByIndex()
    {
        HardwareProfile profile = MakeProfile(("id0", 0), ("id1", 1));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(1) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(profile, configs);

        results.Should().HaveCount(1);
        results[0].Should().BeOfType<MatchResult.Matched>()
            .Which.Output.Id.Should().Be("id1");
    }

    [Fact]
    public void Match_StringTarget_MatchesById()
    {
        HardwareProfile profile = MakeProfile(("MONITOR\\ABC", 0));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromId("MONITOR\\ABC") },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(profile, configs);

        results[0].Should().BeOfType<MatchResult.Matched>();
    }

    [Fact]
    public void Match_NoMatchingOutput_ReturnsNotFound()
    {
        HardwareProfile profile = MakeProfile(("id0", 0));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(99) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(profile, configs);

        results[0].Should().BeOfType<MatchResult.NotFound>();
    }

    [Fact]
    public void Match_DuplicateTarget_SecondIsDuplicate()
    {
        HardwareProfile profile = MakeProfile(("id0", 0));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(0) },
            new OutputOptions { Target = OutputTarget.FromIndex(0) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(profile, configs);

        results.Should().HaveCount(2);
        results[0].Should().BeOfType<MatchResult.Matched>();
        results[1].Should().BeOfType<MatchResult.Duplicate>();
    }

    [Fact]
    public void Match_EmptyProfile_AllNotFound()
    {
        HardwareProfile profile = new([]);
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(0) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(profile, configs);

        results[0].Should().BeOfType<MatchResult.NotFound>();
    }

    [Fact]
    public void Match_NoConfiguredOutputs_IncludesAllDiscovered_WhenSkipIsFalse()
    {
        HardwareProfile profile = MakeProfile(("id0", 0), ("id1", 1));

        ImmutableArray<MatchResult> results = TargetMatcher.Match(
            profile,
            [],
            skipUnspecifiedOutputs: false);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeOfType<MatchResult.Matched>());
    }

    [Fact]
    public void Match_ConfiguredSubset_IncludesUnmatchedDiscovered_WhenSkipIsFalse()
    {
        HardwareProfile profile = MakeProfile(("id0", 0), ("id1", 1));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(0) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(
            profile,
            configs,
            skipUnspecifiedOutputs: false);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeOfType<MatchResult.Matched>());
        results.OfType<MatchResult.Matched>().Select(m => m.Output.Id).Should().BeEquivalentTo(["id0", "id1"]);
    }

    [Fact]
    public void Match_ConfiguredSubset_DoesNotIncludeUnmatchedDiscovered_WhenSkipIsTrue()
    {
        HardwareProfile profile = MakeProfile(("id0", 0), ("id1", 1));
        ImmutableArray<OutputOptions> configs =
        [
            new OutputOptions { Target = OutputTarget.FromIndex(0) },
        ];

        ImmutableArray<MatchResult> results = TargetMatcher.Match(
            profile,
            configs,
            skipUnspecifiedOutputs: true);

        results.Should().HaveCount(1);
        results[0].Should().BeOfType<MatchResult.Matched>();
        results.OfType<MatchResult.Matched>().Single().Output.Id.Should().Be("id0");
    }
}
