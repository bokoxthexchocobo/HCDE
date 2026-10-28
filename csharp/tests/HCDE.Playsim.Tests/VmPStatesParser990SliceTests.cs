namespace HCDE.Playsim.Tests;

public class VmPStatesParser990SliceTests
{
    private sealed class GotoDefine : VmPStatesParser990Slice.IMutableGotoDefine
    {
        public object? State { get; set; }
        public int DefineFlags { get; set; }
        public List<GotoDefine> Children { get; } = [];
    }

    [Fact]
    public void ResolveGotoLabelTail()
    {
        Assert.Equal(
            VmPStatesParser990Slice.GotoScopeResolveKind.SuperParent,
            VmPStatesParser990Slice.ClassifyGotoScopeResolution(
                scopeIsSuper: true,
                parentClass: new object(),
                namedClass: null,
                currentType: new object(),
                namedClassIsActor: false,
                namedIsAncestorOfCurrent: false));

        Assert.True(VmPStatesParser990Slice.TrySplitGotoLabelOffset("Spawn+2", out var label, out var offset, out var hasOffset));
        Assert.Equal("Spawn", label);
        Assert.Equal(2, offset);
        Assert.True(hasOffset);

        var resolved = VmPStatesParser990Slice.ResolveGotoLabelInActorType(
            sameType: true,
            label: "Idle",
            findInSelf: s => s == "Idle" ? "state" : null,
            findInOther: _ => null);
        Assert.Equal("state", resolved);

        Assert.True(VmPStatesParser990Slice.TryApplyOwnedStateOffset(
            baseState: new object(),
            stateIndex: 1,
            ownedCount: 4,
            offset: 1,
            out var targetIndex));
        Assert.Equal(2, targetIndex);
        Assert.False(VmPStatesParser990Slice.TryApplyOwnedStateOffset(
            baseState: new object(),
            stateIndex: 3,
            ownedCount: 4,
            offset: 1,
            out _));
    }

    [Fact]
    public void FixAndResolveGotoTree()
    {
        var child = new GotoDefine
        {
            State = "Super::Melee",
            DefineFlags = VmPStatesParser990Slice.StateDefineLabelFlag,
        };
        var root = new List<GotoDefine>
        {
            new()
            {
                State = 2,
                DefineFlags = VmPStatesParser990Slice.StateDefineIndexFlag,
            },
        };
        root[0].Children.Add(child);

        var fixedCount = VmPStatesParser990Slice.FixIndexedStatePointersInTree(
            root,
            d => d.State,
            d => d.DefineFlags,
            d => d.Children,
            index => $"S{index - 1}",
            (d, state, flags) =>
            {
                d.State = state;
                d.DefineFlags = flags;
            });
        Assert.Equal(1, fixedCount);
        Assert.Equal("S1", root[0].State);

        var resolved = VmPStatesParser990Slice.ResolvePendingGotoLabelsInTree(
            root,
            d => d.State,
            d => d.DefineFlags,
            d => d.Children,
            label => label is string s ? s.Replace("Super::", "", StringComparison.Ordinal) : null,
            (d, state, flags) =>
            {
                d.State = state;
                d.DefineFlags = flags;
            });
        Assert.Equal(1, resolved);
        Assert.Equal("Melee", root[0].Children[0].State);
    }

    [Fact]
    public void SetGotoLabelSession()
    {
        object? assigned = null;
        var lastState = new object();
        Assert.True(VmPStatesParser990Slice.TrySetGotoLabelOnParserSession(
            lastState,
            lastLabel: -1,
            lastStateBeforeLabel: null,
            gotoText: "Idle",
            assignPendingGotoLabel: (state, text) => assigned = text,
            retargetFromLabel: (_, _) => { },
            clearLastStateBeforeLabel: _ => { }));
        Assert.Equal("Idle", assigned);

        var retargetIndex = -1;
        object? beforeAssigned = null;
        var before = new object();
        Assert.True(VmPStatesParser990Slice.TrySetGotoLabelOnParserSession(
            lastState: null,
            lastLabel: 0,
            lastStateBeforeLabel: before,
            gotoText: "See",
            assignPendingGotoLabel: (state, text) =>
            {
                if (ReferenceEquals(state, before))
                    beforeAssigned = text;
            },
            retargetFromLabel: (index, _) => retargetIndex = index,
            clearLastStateBeforeLabel: _ => { }));
        Assert.Equal(1, retargetIndex);
        Assert.Equal("See", beforeAssigned);

        Assert.False(VmPStatesParser990Slice.TrySetGotoLabelOnParserSession(
            lastState: null,
            lastLabel: -1,
            lastStateBeforeLabel: null,
            gotoText: "X",
            assignPendingGotoLabel: (_, _) => { },
            retargetFromLabel: (_, _) => { },
            clearLastStateBeforeLabel: _ => { }));
    }
}
