namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode911SliceTests
{
    [Fact]
    public void ResolveEmbeddedCallTarget_UsesActiveModuleAndByte()
    {
        var (moduleKey, funcnum) = VmPAcsVmOpcode911Slice.ResolveEmbeddedCallTarget(7, 42);
        Assert.Equal(7, moduleKey);
        Assert.Equal(42, funcnum);
    }

    [Fact]
    public void ResolveCallStackTarget_StripsLibraryTag()
    {
        var tagged = (3 << VmPAcsVmOpcode911Slice.LibraryIdShift) | 9;
        var (moduleKey, funcnum) = VmPAcsVmOpcode911Slice.ResolveCallStackTarget(
            tagged,
            lib => lib == 3 ? 99 : null);
        Assert.Equal(99, moduleKey);
        Assert.Equal(9, funcnum);
    }

    [Fact]
    public void WouldOverflowStackForCall_RespectsMargin()
    {
        Assert.True(VmPAcsVmOpcode911Slice.WouldOverflowStackForCall(
            VmPAcsVmOpcode911Slice.StackSize - 50, 100));
        Assert.False(VmPAcsVmOpcode911Slice.WouldOverflowStackForCall(10, 4));
    }

    [Fact]
    public void TryApplyAdd_CombinesTopTwoSlots()
    {
        var stack = new int[8];
        var sp = 2;
        stack[0] = 10;
        stack[1] = 32;
        var state = VmPAcsVmOpcode911Slice.TryApplyAdd(stack, ref sp);
        Assert.Equal(VmPAcsVmOpcode911Slice.ScriptState.Running, state);
        Assert.Equal(1, sp);
        Assert.Equal(42, stack[0]);
    }

    [Fact]
    public void TryApplyAdd_MalformedWhenStackTooShallow()
    {
        var stack = new int[4];
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode911Slice.ScriptState.PleaseRemove,
            VmPAcsVmOpcode911Slice.TryApplyAdd(stack, ref sp));
    }

    [Fact]
    public void TryApplyDivide_SetsDivideByZeroState()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 5;
        stack[1] = 0;
        Assert.Equal(
            VmPAcsVmOpcode911Slice.ScriptState.DivideBy0,
            VmPAcsVmOpcode911Slice.TryApplyDivide(stack, ref sp));
    }

    [Fact]
    public void TryApplyModulus_SetsModulusByZeroState()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 5;
        stack[1] = 0;
        Assert.Equal(
            VmPAcsVmOpcode911Slice.ScriptState.ModulusBy0,
            VmPAcsVmOpcode911Slice.TryApplyModulus(stack, ref sp));
    }

    [Fact]
    public void TryApplyEq_ReturnsOneWhenEqual()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 7;
        stack[1] = 7;
        VmPAcsVmOpcode911Slice.TryApplyEq(stack, ref sp);
        Assert.Equal(1, stack[0]);
        Assert.Equal(1, sp);
    }

    [Fact]
    public void TryApplyLt_ComparesSecondToTop()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 3;
        stack[1] = 9;
        VmPAcsVmOpcode911Slice.TryApplyLt(stack, ref sp);
        Assert.Equal(1, stack[0]);
    }

    [Fact]
    public void RestoreReturnFrame_PushesValueWhenNotDiscarded()
    {
        var stack = new int[16];
        var frame = VmPAcsVmOpcode911Slice.BuildCallReturnFrame(
            100, 1, 2, 0, 2, discardResult: false, entryInstrCount: 5);
        var words = VmPAcsVmOpcode911Slice.CallReturnStackWords();
        var sp = words;
        VmPAcsVmOpcode911Slice.RestoreReturnFrame(
            ref sp, stack, frame, 55,
            out var localsBase, out var localsCount,
            out var returnPc, out _, out _);
        Assert.Equal(0, localsBase);
        Assert.Equal(2, localsCount);
        Assert.Equal(100, returnPc);
        Assert.Equal(3, sp);
        Assert.Equal(55, stack[2]);
    }

    [Fact]
    public void TryEnterScriptFunction_InitializesLocalsAndAdvancesPc()
    {
        var stack = new int[32];
        var sp = 3;
        stack[0] = 1;
        stack[1] = 2;
        stack[2] = 4;
        var func = new VmPAcsVmOpcode911Slice.ScriptFunction(ArgCount: 2, LocalCount: 2, Address: 500);
        Assert.True(VmPAcsVmOpcode911Slice.TryEnterScriptFunction(
            func, returnPc: 80, returnFunctionKey: 9, returnModuleKey: 3,
            localsBase: 1, localsCount: 2, discardResult: true, runaway: 12,
            stack, ref sp, out var frame, out var newPc, out _, out _));
        Assert.Equal(500, newPc);
        Assert.Equal(0, stack[3]);
        Assert.Equal(0, stack[4]);
        Assert.True(frame.DiscardResult);
        Assert.Equal(12u, frame.EntryInstrCount);
    }
}
