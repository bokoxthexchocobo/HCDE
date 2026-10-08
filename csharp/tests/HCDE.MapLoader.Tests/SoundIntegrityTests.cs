namespace HCDE.MapLoader.Tests;

public class SoundIntegrityTests
{
    [Fact]
    public void DisablesCycleMembersWhileKeepingIncomingAliases()
    {
        var level = new PlayLevel
        {
            SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            { ["a"] = "B", ["b"] = "a", ["prefix"] = "a", ["missing"] = "unknown" },
        };
        var disabled = SoundIntegrity.Apply(level);
        Assert.Equal(new[] { "a", "b" }, disabled.Order());
        Assert.Equal("a", level.SoundAliases["prefix"]);
        Assert.True(SoundResourceResolver.TryResolve(level, "prefix", out var resource, out var error), error);
        Assert.Equal("", resource);
        Assert.False(SoundResourceResolver.TryResolve(level, "missing", out _, out _));
        Assert.Empty(SoundIntegrity.Apply(level));
    }

    [Fact]
    public void ChecksEveryRandomBranchBeforeDisabling()
    {
        var level = new PlayLevel
        {
            SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["alias"] = "group" },
            RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            { ["group"] = new[] { "safe", "alias" }, ["prefix"] = new[] { "group", "safe" } },
            SoundDefinitions = new Dictionary<string, string> { ["safe"] = "RESOURCE" },
        };
        Assert.Equal(new[] { "alias", "group" }, SoundIntegrity.Apply(level).Order());
        Assert.True(level.RandomSoundGroups.ContainsKey("prefix"));
        Assert.True(SoundResourceResolver.TryResolve(level, "prefix", () => 0, out var resource, out var error), error);
        Assert.Equal("", resource);
    }

    [Fact]
    public void SelfCyclesDisableWithoutMutatingSimulationSource()
    {
        var original = new PlayLevel { SoundAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["self"] = "SELF" } };
        var copy = original.CopyForSimulation();
        Assert.Equal("self", Assert.Single(SoundIntegrity.Apply(copy)));
        Assert.True(original.SoundAliases.ContainsKey("self"));
    }
}
