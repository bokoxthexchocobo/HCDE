namespace HCDE.MapLoader.Tests;

public class SoundResourceResolverTests
{
    [Theory]
    [InlineData(0, "FIRST")]
    [InlineData(1, "SECOND")]
    [InlineData(2, "FIRST")]
    [InlineData(4, "SECOND")]
    public void RandomSelectionUsesModuloAndRetainsDuplicateWeight(int draw, string expected)
    {
        var level = RandomLevel();
        var calls = 0;
        Assert.True(SoundResourceResolver.TryResolve(level, "group", () => { calls++; return draw; }, out var resource, out var error), error);
        Assert.Equal(expected, resource); Assert.Equal(1, calls);
    }

    [Fact]
    public void NestedGroupsAndAliasesConsumeOneDrawPerGroup()
    {
        var level = RandomLevel();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        { ["group"] = new[] { "alias", "A" }, ["nested"] = new[] { "A", "B" } };
        level.SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["alias"] = "NESTED" };
        var draws = new Queue<int>(new[] { 0, 1 });
        Assert.True(SoundResourceResolver.TryResolve(level, "group", () => draws.Dequeue(), out var resource, out var error), error);
        Assert.Equal("SECOND", resource); Assert.Empty(draws);
    }

    [Theory]
    [InlineData("A", true)]
    [InlineData("missing", false)]
    public void DirectAndMissingSoundsDoNotDraw(string sound, bool expected)
    {
        Assert.Equal(expected, SoundResourceResolver.TryResolve(RandomLevel(), sound, () => throw new InvalidOperationException("Unexpected draw"), out _, out _));
    }

    [Fact]
    public void RandomCycleFailsAfterFirstDraw()
    {
        var level = RandomLevel();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "group", "A" } };
        var calls = 0;
        Assert.False(SoundResourceResolver.TryResolve(level, "group", () => { calls++; return 0; }, out _, out var error));
        Assert.StartsWith("sound-alias-cycle", error); Assert.Equal(1, calls);
    }

    [Fact]
    public void InvalidRandomValueFailsExplicitly()
    {
        Assert.False(SoundResourceResolver.TryResolve(RandomLevel(), "group", () => -1, out _, out var error));
        Assert.Equal("sound-random-value-negative", error);
    }

    private static PlayLevel RandomLevel() => new()
    {
        SoundDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["A"] = "FIRST", ["B"] = "SECOND" },
        RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase) { ["group"] = new[] { "A", "B", "A" } },
    };

    [Theory]
    [InlineData("target")]
    [InlineData("ALIAS")]
    [InlineData("chain")]
    public void ResolvesDirectAndChainedSoundsCaseInsensitively(string sound)
    {
        var level = new PlayLevel
        {
            SoundDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["target"] = "DSPISTOL" },
            SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["alias"] = "TARGET", ["chain"] = "Alias" },
        };
        Assert.True(SoundResourceResolver.TryResolve(level, sound, out var resource, out var error), error);
        Assert.Equal("DSPISTOL", resource);
        Assert.True(SoundResourceResolver.TryResolve(level.CopyForSimulation(), sound, out resource, out error), error);
        Assert.Equal("DSPISTOL", resource);
    }

    [Theory]
    [InlineData("missing", "sound-resource-missing")]
    [InlineData("dangling", "sound-resource-missing")]
    [InlineData("self", "sound-alias-cycle")]
    [InlineData("loop1", "sound-alias-cycle")]
    [InlineData("prefix", "sound-alias-cycle")]
    public void InvalidAliasGraphsFailExplicitly(string sound, string expected)
    {
        var level = new PlayLevel
        {
            SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["dangling"] = "missing", ["self"] = "SELF", ["loop1"] = "loop2", ["loop2"] = "loop1", ["prefix"] = "loop1",
            },
        };
        Assert.False(SoundResourceResolver.TryResolve(level, sound, out var resource, out var error));
        Assert.Equal("", resource); Assert.StartsWith(expected, error);
        Assert.Equal(5, level.SoundAliases.Count);
    }

    [Fact]
    public void EmptyDefinedResourceIsPreserved()
    {
        var level = new PlayLevel { SoundDefinitions = new Dictionary<string, string> { ["empty"] = "" } };
        Assert.True(SoundResourceResolver.TryResolve(level, "empty", out var resource, out var error), error);
        Assert.Equal("", resource);
    }

    [Fact]
    public void ResolvesLongChainsWithoutRecursion()
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < 10000; i++) aliases["sound" + i] = "sound" + (i + 1);
        var level = new PlayLevel { SoundAliases = aliases, SoundDefinitions = new Dictionary<string, string> { ["sound10000"] = "FINAL" } };
        Assert.True(SoundResourceResolver.TryResolve(level, "sound0", out var resource, out var error), error);
        Assert.Equal("FINAL", resource);
    }
}
