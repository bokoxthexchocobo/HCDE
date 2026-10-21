namespace HCDE.Scripting.Tests;

public class ZccStackVariableEmitTests
{
    [Fact]
    public void StackOffsetRegister_ZeroMeansDefault() =>
        Assert.Equal(-1, ZccStackVariableEmit.StackOffsetRegister(0));

    [Fact]
    public void StackLoadOpForMember_RewritesLoToLp() =>
        Assert.Equal("OP_LP", ZccStackVariableEmit.StackLoadOpForMember("OP_LO"));

    [Fact]
    public void StackVariableWritable_RequiresCtx() =>
        Assert.False(ZccStackVariableEmit.StackVariableWritable(true, false));

    [Fact]
    public void StackUseBitLoad_WhenBitSet() =>
        Assert.True(ZccStackVariableEmit.StackUseBitLoad(2));

    [Fact]
    public void PlanStackVariableEmit_OutVariableLoadBranch() =>
        Assert.True(ZccStackVariableEmit.PlanStackVariableEmit(true, false, 0, 0, -1, "OP_LW").UseOutPointerLoad);

    [Fact]
    public void PlanStackVariableEmit_BitMemberUsesLbit()
    {
        var plan = ZccStackVariableEmit.PlanStackVariableEmit(false, false, 0, 4, 2, "OP_LW");
        Assert.True(plan.UseBitLoad);
        Assert.Equal(4, plan.BitMask);
    }

    [Fact]
    public void StructMemberWritable_RequiresScopeBarrier() =>
        Assert.False(ZccStackVariableEmit.StructMemberWritable(true, true, 0, false, false, false));

    [Fact]
    public void StructMemberResolve_RejectsUnreadable() =>
        Assert.False(ZccStackVariableEmit.StructMemberResolveReadable(false));

    [Fact]
    public void StructMemberResolve_StaticReadonlyPromotesGlobal() =>
        Assert.True(ZccStackVariableEmit.StaticReadonlyPromotesToGlobal(0x4 | 0x8));

    [Fact]
    public void ApplyColorChannelExtract_RedChannel() =>
        Assert.Equal(0xAB, ZccStackVariableEmit.ApplyColorChannelExtract(0x00ABCD00, "r"));
}
