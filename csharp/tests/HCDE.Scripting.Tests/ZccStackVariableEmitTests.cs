namespace HCDE.Scripting.Tests;

public class ZccStackVariableEmitTests
{
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
    public void StaticReadonlyPromotesToGlobal_MatchesFlags() =>
        Assert.True(ZccStackVariableEmit.StaticReadonlyPromotesToGlobal(0x4 | 0x8));

    [Fact]
    public void ApplyColorChannelExtract_RedChannel() =>
        Assert.Equal(0xAB, ZccStackVariableEmit.ApplyColorChannelExtract(0x00ABCD00, "r"));
}
