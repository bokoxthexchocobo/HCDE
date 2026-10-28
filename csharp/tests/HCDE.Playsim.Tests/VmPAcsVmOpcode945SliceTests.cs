namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode945SliceTests
{
    [Fact]
    public void AndLogical_TruthyIntsCombineToOne()
    {
        Assert.Equal(1, VmPAcsVmOpcode945Slice.AndLogical(3, 5));
        Assert.Equal(0, VmPAcsVmOpcode945Slice.AndLogical(3, 0));
    }

    [Fact]
    public void PopBinary_AndBitwise_WritesCombinedValue()
    {
        var stack = new int[] { 6, 3 };
        var sp = 2;
        VmPAcsVmOpcode945Slice.PopBinary(stack, ref sp, VmPAcsVmOpcode945Slice.AndBitwise);
        Assert.Equal(2, stack[0]);
        Assert.Equal(1, sp);
    }

    [Fact]
    public void IfNotGotoBranches_WhenConditionZero() =>
        Assert.True(VmPAcsVmOpcode945Slice.IfNotGotoBranches(0));

    [Fact]
    public void ResolveScriptWaitState_UsesPreWhenNotRunning() =>
        Assert.Equal(
            VmPAcsIsClientSideScript.ScriptState.ScriptWaitPre,
            VmPAcsVmOpcode945Slice.ResolveScriptWaitState(false));

    [Fact]
    public void ResolveScriptWaitDirect_CompatPathSkipsPreWait()
    {
        var (_, state) = VmPAcsVmOpcode945Slice.ResolveScriptWaitDirect(true, 9, false);
        Assert.Equal(VmPAcsIsClientSideScript.ScriptState.ScriptWait, state);
    }

    [Fact]
    public void EncodeScriptWaitNamedKey_NegatesNameIndex() =>
        Assert.Equal(-12, VmPAcsVmOpcode945Slice.EncodeScriptWaitNamedKey(12));

    [Fact]
    public void ClearLineSpecial_HasActivationLine() =>
        Assert.True(VmPAcsVmOpcode945Slice.ClearLineSpecialHasActivationLine(new object()));

    [Fact]
    public void BinaryStackReady_RequiresTwoSlots() =>
        Assert.False(VmPAcsVmOpcode945Slice.BinaryStackReady(1));
}
