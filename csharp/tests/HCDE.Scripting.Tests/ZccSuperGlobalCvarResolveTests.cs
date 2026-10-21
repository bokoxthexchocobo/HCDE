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
    public void GlobalVariableBitMask_MatchesEmitLbit()
    {
        Assert.True(ZccSuperGlobalCvarResolve.ShouldUseBitLoad(3));
        Assert.Equal(8, ZccSuperGlobalCvarResolve.GlobalVariableBitMask(3));
        Assert.Equal("OP_LBIT", ZccSuperGlobalCvarResolve.GlobalVariableLoadOp(true, "OP_LW"));
    }

    [Fact]
    public void FlagCvarEmit_AppliesShiftAndMask()
    {
        const int bitNum = 2;
        var shifted = ZccSuperGlobalCvarResolve.ApplyFlagCvarShift(4, bitNum);
        Assert.Equal(1, ZccSuperGlobalCvarResolve.ApplyFlagCvarMask(shifted, 1));
    }

    [Fact]
    public void MaskCvarEmit_MatchesNativeSequence() =>
        Assert.Equal(7, ZccSuperGlobalCvarResolve.ApplyMaskCvar(0xFE, 0xF, 1));

    [Fact]
    public void MapCvarRealType_StringMapsToString() =>
        Assert.Equal(ZccSuperGlobalCvarResolve.CvarRealType.String, ZccSuperGlobalCvarResolve.MapCvarRealType(3));

    [Fact]
    public void MapCvarRealType_FlagUsesNativeEnumIndex() =>
        Assert.Equal(ZccSuperGlobalCvarResolve.CvarRealType.Flag, ZccSuperGlobalCvarResolve.MapCvarRealType(5));

    [Fact]
    public void EmitLoadOp_FloatUsesLsp() =>
        Assert.Equal("OP_LSP", ZccSuperGlobalCvarResolve.EmitLoadOp(ZccSuperGlobalCvarResolve.CvarRealType.Float));

    [Fact]
    public void CvarEmitUsesStringRegister_ForStringType() =>
        Assert.True(ZccSuperGlobalCvarResolve.CvarEmitUsesStringRegister(ZccSuperGlobalCvarResolve.CvarRealType.String));
}
