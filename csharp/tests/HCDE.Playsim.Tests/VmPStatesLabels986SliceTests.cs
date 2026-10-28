namespace HCDE.Playsim.Tests;

public class VmPStatesLabels986SliceTests
{
    [Fact]
    public void DestroyNestedLabelChildren_TearsDownTrees()
    {
        var freed = new List<object>();
        var count = VmPStatesLabels986Slice.DestroyNestedLabelChildren(
            numLabels: 2,
            i => i == 0 ? new object() : null,
            _ => { },
            node => freed.Add(node!));
        Assert.Equal(1, count);
        Assert.Single(freed);
    }

    [Fact]
    public void SpecialDeathStatesAndLegacyAliases()
    {
        var deathChildren = new object?[] { new object(), null };
        Assert.True(VmPStatesLabels986Slice.ActorHasSpecialDeathStates(
            stateLabels: new object(),
            _ => new object(),
            _ => deathChildren.Length,
            (_, i) => deathChildren[i]));

        Assert.True(VmPStatesLabels986Slice.TryRemapLegacyStateAlias(
            "Burn",
            out var first,
            out var second));
        Assert.Equal(VmPStatesLabels986Slice.DeathLabel, first);
        Assert.Equal("Fire", second);

        var parts = new List<string>();
        Assert.Equal(3, VmPStatesLabels986Slice.BuildStateNameParts(["XDeath", "Extra"], parts));
        Assert.Equal("Death", parts[0]);
        Assert.Equal("Extreme", parts[1]);
        Assert.Equal("Extra", parts[2]);
    }

    [Fact]
    public void FindState_BestMatchAndExact()
    {
        object stateA = "A";
        var best = VmPStatesLabels986Slice.FindBestStateForNames(
            rootLabels: new object(),
            names: ["Idle", "See"],
            exact: false,
            (_, label) => label switch
            {
                "Idle" => (new object(), null),
                "See" => (null, stateA),
                _ => (null, null),
            });
        Assert.Equal(stateA, best);

        Assert.Null(VmPStatesLabels986Slice.FindBestStateForNames(
            rootLabels: new object(),
            names: ["Idle", "Miss"],
            exact: true,
            (_, label) => label == "Idle" ? (new object(), null) : (null, null)));
    }
}
