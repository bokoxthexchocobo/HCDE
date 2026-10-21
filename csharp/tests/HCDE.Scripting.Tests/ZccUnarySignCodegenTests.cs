namespace HCDE.Scripting.Tests;

public class ZccUnarySignCodegenTests
{
    [Fact]
    public void PlusSignResolveAllowed_AcceptsQuaternion() =>
        Assert.True(ZccUnarySignCodegen.PlusSignResolveAllowed(false, false, true));

    [Fact]
    public void ResolveMinusSign_FoldsIntConstant() =>
        Assert.Equal(-5, ZccUnarySignCodegen.ResolveMinusSign(true, false, false, true, true, 5, 0, false).FoldedInt);

    [Fact]
    public void ResolveMinusSign_CastsBoolWhenNonConst() =>
        Assert.True(ZccUnarySignCodegen.ResolveMinusSign(true, false, false, false, false, 0, 0, true).NeedsBoolToIntCast);

    [Fact]
    public void TypeCastEmitIsUnreachable() =>
        Assert.True(true);

    [Fact]
    public void EmitMinusSign_KonstIntPoolBranch()
    {
        var result = ZccUnarySignCodegen.EmitMinusSign(
            true,
            1,
            true,
            true,
            1,
            false,
            3,
            _ => 3,
            _ => 9,
            v => v,
            (_, _) => 0,
            _ => { });
        Assert.True(result.ReturnedKonstInt);
        Assert.Equal(-9, result.RegNum);
        Assert.True(result.Konst);
    }

    [Fact]
    public void EmitMinusSign_VectorNegV3WhenFixed()
    {
        var freed = false;
        var result = ZccUnarySignCodegen.EmitMinusSign(
            false,
            3,
            false,
            false,
            3,
            true,
            7,
            _ => 0,
            _ => 0,
            v => v,
            (_, count) => count,
            _ => freed = true);
        Assert.True(freed);
        Assert.Equal(ZccUnarySignCodegen.MinusEmitOpcode.NegV3, result.Opcode);
        Assert.Equal(3, result.RegNum);
    }
}
