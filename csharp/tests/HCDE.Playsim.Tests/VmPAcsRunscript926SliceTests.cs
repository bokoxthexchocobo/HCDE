namespace HCDE.Playsim.Tests;

public class VmPAcsRunscript926SliceTests
{
    [Fact]
    public void TryPopCharArrayParms_Ranged_RejectsInvalidCapacity()
    {
        var stack = new List<int> { 0, 5, 2, 0 };
        var sp = stack.Count;
        Assert.False(VmPAcsRunscript926Slice.TryPopCharArrayParms(stack, ref sp, true, out _, out _, out _));
        Assert.Equal(0, sp);
    }

    [Fact]
    public void TryPopCharArrayParms_NonRanged_CombinesOffset()
    {
        var stack = new List<int> { 7, 3 };
        var sp = stack.Count;
        Assert.True(VmPAcsRunscript926Slice.TryPopCharArrayParms(stack, ref sp, false, out var cap, out var off, out var idx));
        Assert.Equal(int.MaxValue, cap);
        Assert.Equal(7, off);
        Assert.Equal(3, idx);
        Assert.Equal(0, sp);
    }

    [Fact]
    public void ComputeSpecialArgMask_HexenOld_TruncatesToByte()
    {
        Assert.Equal(255, VmPAcsRunscript926Slice.ComputeSpecialArgMask(true, AcsFormat.Old));
        Assert.Equal(-1, VmPAcsRunscript926Slice.ComputeSpecialArgMask(true, AcsFormat.LittleEnhanced));
    }

    [Fact]
    public void TickPreRunWaitState_DelayedBecomesRunningWhenZero()
    {
        var state = VmPAcsIsClientSideScript.ScriptState.Delayed;
        var next = state;
        var data = 1;
        var outcome = VmPAcsRunscript926Slice.TickPreRunWaitState(
            state, ref next, ref data, _ => false, _ => true, _ => false, null);
        Assert.Equal(VmPAcsRunscript926Slice.WaitTickOutcome.Continue, outcome);
        Assert.Equal(VmPAcsIsClientSideScript.ScriptState.Running, next);
    }

    [Fact]
    public void TickPreRunWaitState_TagWaitReturnsWhileSectorBusy()
    {
        var state = VmPAcsIsClientSideScript.ScriptState.TagWait;
        var next = state;
        var data = 4;
        var outcome = VmPAcsRunscript926Slice.TickPreRunWaitState(
            state, ref next, ref data, tag => tag == 4, _ => false, _ => false, null);
        Assert.Equal(VmPAcsRunscript926Slice.WaitTickOutcome.ReturnResult, outcome);
    }

    [Fact]
    public void ReadNextPcode_LittleEnhanced_DecodesEscapedOpcode()
    {
        var bytes = new byte[] { 250, 9, 0 };
        var pc = 0;
        var pcd = VmPAcsRunscript926Slice.ReadNextPcode(bytes, ref pc, AcsFormat.LittleEnhanced);
        Assert.Equal(2809, pcd);
        Assert.Equal(2, pc);
    }

    [Fact]
    public void ApplyRunningOpcode_SuspendSetsSuspended()
    {
        var state = VmPAcsIsClientSideScript.ScriptState.Running;
        VmPAcsRunscript926Slice.ApplyRunningOpcode((int)HCDE.MapLoader.AcsPcode.Suspend, false, ref state);
        Assert.Equal(VmPAcsIsClientSideScript.ScriptState.Suspended, state);
    }

    [Fact]
    public void ShouldTerminateRunaway_RespectsBusyFlag()
    {
        uint counter = 0;
        Assert.False(VmPAcsRunscript926Slice.ShouldTerminateRunaway(true, ref counter));
        Assert.Equal(0u, counter);
    }
}
