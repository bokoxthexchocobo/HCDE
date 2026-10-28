namespace HCDE.Playsim.Tests;

public class VmPAcsRunscript927SliceTests
{
    [Fact]
    public void TryInternTagStringOnStack_ReplacesTopWithPoolId()
    {
        var stack = new List<int> { 42 };
        Assert.True(VmPAcsRunscript927Slice.TryInternTagStringOnStack(stack, i => $"str{i}", s => s.Length + 1000));
        Assert.Equal(1005, stack[^1]);
    }

    [Fact]
    public void TryPushNumber_ReadsLittleEndianWord()
    {
        var script = new byte[] { 0x34, 0x12, 0, 0, 0 };
        var pc = 0;
        var stack = new List<int>();
        Assert.True(VmPAcsRunscript927Slice.TryPushNumber(script, ref pc, stack));
        Assert.Equal(0x1234, stack[0]);
        Assert.Equal(4, pc);
    }

    [Fact]
    public void TryPushCountedBytes_MatchesNativeLoopOrder()
    {
        var script = new byte[] { 2, 9, 8, 0 };
        var pc = 0;
        var stack = new List<int>();
        Assert.True(VmPAcsRunscript927Slice.TryPushCountedBytes(script, ref pc, stack));
        Assert.Equal(new[] { 9, 8 }, stack);
        Assert.Equal(3, pc);
    }

    [Fact]
    public void TryDup_AndSwap_ManipulateStackTop()
    {
        var stack = new List<int> { 1, 2 };
        var sp = stack.Count;
        Assert.True(VmPAcsRunscript927Slice.TryDup(stack, ref sp));
        Assert.Equal(3, sp);
        Assert.Equal(2, stack[^1]);
        Assert.True(VmPAcsRunscript927Slice.TrySwap(stack, ref sp));
        Assert.Equal(new[] { 1, 2, 2 }, stack);
    }

    [Fact]
    public void TryPopLspecOperands_AppliesMaskAndPops()
    {
        var stack = new List<int> { 0x1ff, 0x102 };
        var sp = stack.Count;
        Span<int> args = stackalloc int[2];
        Assert.True(VmPAcsRunscript927Slice.TryPopLspecOperands(stack, ref sp, 2, 255, args));
        Assert.Equal(0, sp);
        Assert.Equal(255, args[0]);
        Assert.Equal(2, args[1]);
    }

    [Fact]
    public void TryPopLspecOperands_FiveWide_FillsArgs()
    {
        var stack = new List<int> { 5, 4, 3, 2, 1 };
        var sp = stack.Count;
        Span<int> args = stackalloc int[5];
        Assert.True(VmPAcsRunscript927Slice.TryPopLspecOperands(stack, ref sp, 5, -1, args));
        Assert.Equal(0, sp);
        Assert.Equal(new[] { 5, 4, 3, 2, 1 }, args.ToArray());
    }

    [Fact]
    public void ResetStackOnMalformedLspec_ClearsSp()
    {
        var sp = 3;
        VmPAcsRunscript927Slice.ResetStackOnMalformedLspec(ref sp);
        Assert.Equal(0, sp);
    }
}
