namespace HCDE.Playsim.Tests;

public class VmPStatesParser991SliceTests
{
    private sealed class FrameState : VmPStatesParser991Slice.IMutableFrameState
    {
        public int Frame { get; set; }
        public int StateFlags { get; set; }
        public int DefineFlags { get; set; }
        public int LoopTargetIndex { get; set; }
    }

    [Fact]
    public void SetStopWaitLoopSession()
    {
        var stopFlags = 0;
        Assert.True(VmPStatesParser991Slice.TrySetStopOnParserSession(
            lastState: new object(),
            lastLabel: -1,
            lastStateBeforeLabel: null,
            applyStopToState: _ => stopFlags = VmPStatesParser991Slice.StateDefineStop,
            retargetFromLabelWithStop: _ => { },
            clearLastStateBeforeLabel: _ => { }));
        Assert.Equal(VmPStatesParser991Slice.StateDefineStop, stopFlags);

        var waitApplied = false;
        Assert.True(VmPStatesParser991Slice.TrySetWaitOnParserSession(
            new object(),
            _ => waitApplied = true,
            _ => { }));
        Assert.True(waitApplied);

        var loopTarget = -1;
        Assert.True(VmPStatesParser991Slice.TrySetLoopOnParserSession(
            new object(),
            lastLabel: 2,
            (state, target) => loopTarget = target,
            _ => { }));
        Assert.Equal(3, loopTarget);
    }

    [Fact]
    public void AddStatesFrameDecode()
    {
        var working = new FrameState { StateFlags = VmPStatesParser991Slice.StateFlagNoDelay };
        var pushed = new List<FrameState>();
        var count = VmPStatesParser991Slice.AddStatesFromFrameCharacters(
            "AB#C^",
            working,
            state =>
            {
                pushed.Add(new FrameState
                {
                    Frame = state.Frame,
                    StateFlags = state.StateFlags,
                });
            },
            state => state.StateFlags &= ~VmPStatesParser991Slice.StateFlagNoDelay);
        Assert.Equal(5, count);
        Assert.Equal(0, pushed[0].Frame);
        Assert.Equal(1, pushed[1].Frame);
        Assert.True((pushed[2].StateFlags & VmPStatesParser991Slice.StateFlagSameFrame) != 0);
        Assert.Equal(VmPStatesParser991Slice.CaretFrameIndex, pushed[4].Frame);
        Assert.Equal(0, working.StateFlags & VmPStatesParser991Slice.StateFlagNoDelay);
    }

    [Fact]
    public void FinishStatesHeadPipeline()
    {
        Assert.True(VmPStatesParser991Slice.ShouldMaterializeOwnedStates(3));
        var order = new List<string>();
        VmPStatesParser991Slice.RunFinishStatesLabelFixPipeline(
            stateCount: 2,
            () => order.Add("fix"),
            () => order.Add("resolve"));
        Assert.Equal(["fix", "resolve"], order);

        order.Clear();
        VmPStatesParser991Slice.RunFinishStatesLabelFixPipeline(
            stateCount: 0,
            () => order.Add("fix"),
            () => order.Add("resolve"));
        Assert.Equal(["resolve"], order);

        Assert.True(VmPStatesParser991Slice.ShouldEnterPerStateDefineResolveLoop(4, loopIndex: 0));
        Assert.False(VmPStatesParser991Slice.ShouldEnterPerStateDefineResolveLoop(4, loopIndex: 1));
    }
}
