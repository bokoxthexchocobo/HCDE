namespace HCDE.Playsim.Tests;

public class VmPStatesDefine988SliceTests
{
    private sealed class StateDefine : VmPStatesDefine988Slice.IMutableStateDefine
    {
        public required string Label { get; init; }
        public object? State { get; set; }
        public int DefineFlags { get; set; }
        public List<StateDefine> Children { get; } = [];
    }

    [Fact]
    public void StateLabelAssignmentAndIndexEncoding()
    {
        var def = new StateDefine { Label = "Idle" };
        VmPStatesDefine988Slice.ApplyStateLabelAssignment(def, state: "S0", defineFlags: 0);
        Assert.Equal("S0", def.State);
        VmPStatesDefine988Slice.AssignPendingStateIndexLabel(def, arraySize: 4, out var lastLabel);
        Assert.Equal(4, lastLabel);
        Assert.True(VmPStatesDefine988Slice.TryDecodeStateLabelIndex(5, arraySize: 4, out var idx));
        Assert.Equal(4, idx);
    }

    [Fact]
    public void ResolveAndPruneStateDefines()
    {
        var root = new List<StateDefine>
        {
            new() { Label = "Spawn", State = "spawn" },
            new() { Label = "", State = null },
        };
        Assert.Equal(1, VmPStatesDefine988Slice.PruneEmptyStateDefines(
            root,
            d => d.Label,
            d => d.State,
            d => d.Children.Count));

        var resolved = VmPStatesDefine988Slice.ResolveStateDefineByNameParts(
            ["Spawn"],
            (list, name) => list.Cast<StateDefine>().FirstOrDefault(d => d.Label == name),
            d => ((StateDefine)d).Children,
            root);
        Assert.Equal("spawn", resolved);
    }

    [Fact]
    public void InstallStatesHeadPredicates()
    {
        Assert.True(VmPStatesDefine988Slice.CanInstallStates(defaults: new object(), actorInfo: new object()));
        Assert.Equal("fallback", VmPStatesDefine988Slice.SelectSpawnStateForInstall(
            foundSpawn: null,
            defaultSpawn: "fallback"));
        Assert.True(VmPStatesDefine988Slice.ShouldDestroyExistingStateList(
            stateList: new object(),
            reinstalling: true));
        Assert.True(VmPStatesDefine988Slice.MaterializeStateLabelListEnabled(countAfterPrune: 2));
        Assert.Equal(4, VmPStatesDefine988Slice.CountCachedInstallStates(1, 1, 1, 1));
    }
}
