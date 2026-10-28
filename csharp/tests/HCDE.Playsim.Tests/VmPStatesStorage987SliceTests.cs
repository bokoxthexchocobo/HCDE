namespace HCDE.Playsim.Tests;

public class VmPStatesStorage987SliceTests
{
    private sealed class StateDefine
    {
        public required string Label { get; init; }
        public List<StateDefine> Children { get; } = [];
    }

    [Fact]
    public void JumpVerificationAndEncodings()
    {
        Assert.True(VmPStatesStorage987Slice.UsesNamedNameLabelEncoding(0x10000005));
        Assert.Equal(5, VmPStatesStorage987Slice.DecodeNamedNameLabel(0x10000005));
        Assert.Equal(-4, VmPStatesStorage987Slice.DecodeJumpStorageByteOffset(-65536));
        var actorBase = "AActor";
        var mobClass = "Imp";
        object calling = "Call";
        object target = "Target";
        Assert.True(VmPStatesStorage987Slice.VerifyJumpTargetInClassHierarchy(
            startClass: mobClass,
            actorBaseClass: actorBase,
            callingState: calling,
            targetOffset: 3,
            getParentClass: cls => ReferenceEquals(cls, mobClass) ? actorBase : null,
            ownsState: (cls, st) => ReferenceEquals(cls, mobClass) && (ReferenceEquals(st, calling) || ReferenceEquals(st, target)),
            stateAtOffset: (call, off) => off == 3 ? target : null));
    }

    [Fact]
    public void ResolveScriptStateFromStorage()
    {
        object named = "Named";
        var fromNamed = VmPStatesStorage987Slice.ResolveScriptStateFromStorage(
            0x10000002,
            actorClass: new object(),
            exact: false,
            findStateByNamedName: (id, _) => id == 2 ? named : null,
            resolveJumpEncodedState: null,
            findStateFromLabelCount: null);
        Assert.Equal(named, fromNamed);

        object direct = "Direct";
        var fromBundle = VmPStatesStorage987Slice.ResolveBundledStoragePos(
            storagePos: 1,
            actorClass: null,
            exact: false,
            findStateFromLabelCount: null,
            readLabelCount: _ => 0,
            readEmbeddedState: _ => direct);
        Assert.Equal(direct, fromBundle);
    }

    [Fact]
    public void StateDefineListWalk()
    {
        var root = new List<StateDefine>();
        var leaf = VmPStatesStorage987Slice.WalkStateDefineAddress(
            ["Spawn", "See"],
            root,
            d => d.Label,
            d => d.Children,
            label => new StateDefine { Label = label });
        Assert.NotNull(leaf);
        Assert.Equal("See", leaf!.Label);
        Assert.Single(root);
        Assert.Single(root[0].Children);
    }
}
