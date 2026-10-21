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
    public void MapCvarRealType_StringMapsToString() =>
        Assert.Equal(ZccSuperGlobalCvarResolve.CvarRealType.String, ZccSuperGlobalCvarResolve.MapCvarRealType(5));

    [Fact]
    public void ShouldUseBitLoad_WhenBitValueSet() =>
        Assert.True(ZccSuperGlobalCvarResolve.ShouldUseBitLoad(3));

    [Fact]
    public void EmitLoadOp_FloatUsesLsp() =>
        Assert.Equal("OP_LSP", ZccSuperGlobalCvarResolve.EmitLoadOp(ZccSuperGlobalCvarResolve.CvarRealType.Float));

    [Fact]
    public void GlobalVariableWritable_RequiresBoth() =>
        Assert.False(ZccSuperGlobalCvarResolve.GlobalVariableWritable(false, true));

    [Fact]
    public void CvarEmitUsesStringRegister_ForStringType() =>
        Assert.True(ZccSuperGlobalCvarResolve.CvarEmitUsesStringRegister(ZccSuperGlobalCvarResolve.CvarRealType.String));
}
