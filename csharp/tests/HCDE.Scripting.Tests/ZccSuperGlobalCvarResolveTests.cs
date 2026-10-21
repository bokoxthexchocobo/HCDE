namespace HCDE.Scripting.Tests;

public class ZccSuperGlobalCvarResolveTests
{
    [Fact]
    public void TryResolveSuper_RejectsOutsideMemberFunction()
    {
        Assert.False(ZccSuperGlobalCvarResolve.TryResolveSuper(false, true, out var invalid));
        Assert.True(invalid);
    }

    [Fact]
    public void GlobalVariableWritable_RequiresBoth() =>
        Assert.False(ZccSuperGlobalCvarResolve.GlobalVariableWritable(false, true));

    [Fact]
    public void ShouldUseBitLoad_WhenBitValueSet() =>
        Assert.True(ZccSuperGlobalCvarResolve.ShouldUseBitLoad(3));

    [Fact]
    public void PlanGlobalVariableEmit_BitLoadUsesLbit()
    {
        var plan = ZccSuperGlobalCvarResolve.PlanGlobalVariableEmit(false, 3, "OP_LW");
        Assert.True(plan.UseBitLoad);
        Assert.Equal(8, plan.BitMask);
    }

    [Fact]
    public void PlanCvarEmit_FlagAppliesShiftAndMask()
    {
        var plan = ZccSuperGlobalCvarResolve.PlanCvarEmit(ZccSuperGlobalCvarResolve.CvarRealType.Flag, 2);
        var shifted = ZccSuperGlobalCvarResolve.ApplyFlagCvarShift(4, plan.ExtraShift);
        Assert.Equal(1, ZccSuperGlobalCvarResolve.ApplyFlagCvarMask(shifted, plan.AndMask));
    }

    [Fact]
    public void PlanCvarEmit_MaskMatchesNativeSequence()
    {
        var plan = ZccSuperGlobalCvarResolve.PlanCvarEmit(
            ZccSuperGlobalCvarResolve.CvarRealType.Mask,
            maskBitVal: 0xF,
            maskBitNum: 1);
        Assert.Equal(7, ZccSuperGlobalCvarResolve.ApplyMaskCvar(0xFE, 0xF, plan.ExtraShift));
    }

    [Fact]
    public void MapCvarRealType_StringMapsToString() =>
        Assert.Equal(ZccSuperGlobalCvarResolve.CvarRealType.String, ZccSuperGlobalCvarResolve.MapCvarRealType(5));

    [Fact]
    public void EmitLoadOp_FloatUsesLsp() =>
        Assert.Equal("OP_LSP", ZccSuperGlobalCvarResolve.EmitLoadOp(ZccSuperGlobalCvarResolve.CvarRealType.Float));

    [Fact]
    public void CvarEmitUsesStringRegister_ForStringType() =>
        Assert.True(ZccSuperGlobalCvarResolve.CvarEmitUsesStringRegister(ZccSuperGlobalCvarResolve.CvarRealType.String));
}
