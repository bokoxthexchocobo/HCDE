namespace HCDE.Playsim.Tests;

public class VmPStatesParser989SliceTests
{
    private sealed class RetargetDefine : VmPStatesParser989Slice.IMutableRetargetDefine
    {
        public object? State { get; set; }
        public int DefineFlags { get; set; }
        public List<RetargetDefine> Children { get; } = [];
    }

    [Fact]
    public void InstallCacheAndImport()
    {
        object? spawn = null;
        object? see = null;
        VmPStatesParser989Slice.CacheInstalledDefaultStates(
            v => spawn = v,
            v => see = v,
            _ => { },
            _ => { },
            spawn: "S",
            see: "See",
            melee: null,
            missile: null);
        Assert.Equal("S", spawn);
        Assert.Equal("See", see);
        Assert.True(VmPStatesParser989Slice.ShouldRebuildStateDefinesFromClass(
            actorClass: new object(),
            stateLabels: new object()));
    }

    [Fact]
    public void RetargetIndexedDefines()
    {
        var root = new List<RetargetDefine>
        {
            new() { State = 3, DefineFlags = VmPStatesParser989Slice.StateDefineIndexFlag },
        };
        var count = VmPStatesParser989Slice.RetargetIndexedDefinesInTree(
            root,
            targetCount: 3,
            targetLabel: "Idle",
            d => d.State,
            d => d.DefineFlags,
            d => d.Children,
            (d, stop, label) =>
            {
                if (stop)
                    VmPStatesParser989Slice.ApplyRetargetToStop(d);
                else
                    VmPStatesParser989Slice.ApplyRetargetToLabel(d, label!);
            });
        Assert.Equal(1, count);
        Assert.Equal(VmPStatesParser989Slice.StateDefineLabelFlag, root[0].DefineFlags);
    }

    [Fact]
    public void ResolveGotoLabelHead()
    {
        Assert.True(VmPStatesParser989Slice.GotoLabelHasClassScope("Super::Idle", out var sep));
        Assert.True(VmPStatesParser989Slice.GotoClassScopeIsSuper("Super"));
        Assert.Equal("Idle", VmPStatesParser989Slice.ExtractGotoLabelAfterScope("Super::Idle", sep));
        Assert.True(VmPStatesParser989Slice.ShouldMergeFlatStateDefine(
            childLabels: null,
            labelAlreadyPresent: false));
    }
}
