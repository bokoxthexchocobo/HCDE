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
    public void SelectMinusEmitOp_PicksNegV3() =>
        Assert.Equal(ZccUnarySignCodegen.MinusEmitOp.NegV3, ZccUnarySignCodegen.SelectMinusEmitOp(false, 3));

    [Fact]
    public void NegateKonstIntFromPool_MatchesEmitBranch() =>
        Assert.Equal(-9, ZccUnarySignCodegen.NegateKonstIntFromPool(3, _ => 3, _ => 9));

    [Fact]
    public void TypeCastEmitIsUnreachable() =>
        Assert.True(ZccUnarySignCodegen.TypeCastEmitIsUnreachable());

    [Fact]
    public void ClassifyPlusResolve_UnwrapsNumeric() =>
        Assert.Equal(ZccUnarySignCodegen.PlusResolveOutcome.UnwrapOperand, ZccUnarySignCodegen.ClassifyPlusResolve(true, false, false));

    [Fact]
    public void PlanMinusEmit_AllocatesWhenFixed()
    {
        var plan = ZccUnarySignCodegen.PlanMinusEmit(false, 3, false, true);
        Assert.True(plan.AllocateNewRegister);
        Assert.Equal(ZccUnarySignCodegen.MinusEmitOp.NegV3, plan.FloatVectorOp);
    }
}
