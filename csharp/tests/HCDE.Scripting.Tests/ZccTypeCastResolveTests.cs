namespace HCDE.Scripting.Tests;

public class ZccTypeCastResolveTests
{
    [Fact]
    public void NullPtrToPointer_Allowed() =>
        Assert.True(ZccTypeCastResolve.NullPtrToPointer(true, true));

    [Fact]
    public void VersionAtLeast_4141() =>
        Assert.True(ZccTypeCastResolve.VersionAtLeast(
            ZccTypeCastResolve.MakeVersion(4, 14, 1), 4, 14, 1));

    [Fact]
    public void NullPtrToIdTypes_RequiresVersion() =>
        Assert.False(ZccTypeCastResolve.NullPtrToIdTypes(true, true, false, false, 40000, ZccTypeCastResolve.MakeVersion(4, 14, 1)));

    [Fact]
    public void NativeStructToWritablePointer_RejectsReadonlyAt412()
    {
        Assert.False(ZccTypeCastResolve.NativeStructToWritablePointer(
            true, true, true, false, false, ZccTypeCastResolve.MakeVersion(4, 12, 0)));
    }

    [Fact]
    public void ClassDescendantAllowsCast()
    {
        Assert.True(ZccTypeCastResolve.ClassDescendantAllowsCast(true, true, true));
    }

    [Fact]
    public void PlanRawFunction_RejectsVirtual()
    {
        Assert.Equal(
            ZccTypeCastResolve.RawFunctionPointerPlan.RejectVirtualOrAction,
            ZccTypeCastResolve.PlanRawFunctionPointer(false, ZccTypeCastResolve.VarfVirtual));
    }

    [Fact]
    public void CompatiblePointerTypes_RejectsConstStrip()
    {
        var dest = new ZccTypeCastResolve.PointerDesc(ZccTypeCastResolve.PointerKind.Real, false, false, false, true, false, false, true, true, true);
        var source = new ZccTypeCastResolve.PointerDesc(ZccTypeCastResolve.PointerKind.Real, true, false, false, true, false, false, true, true, true);
        Assert.False(ZccTypeCastResolve.CompatiblePointerTypes(dest, source, forCompare: false));
    }

    [Fact]
    public void ClassifyResolveCast_NullPtrToId_Requires4141()
    {
        var path = ZccTypeCastResolve.ClassifyResolveCast(
            false, false, false, false, true, false,
            true, false, false, ZccTypeCastResolve.MakeVersion(4, 14, 1),
            false, false, false, false, false, false, false, false, false, false);
        Assert.Equal(ZccTypeCastResolve.ResolveCastPath.NullPtrToIdConstant, path);
    }

    [Fact]
    public void RunTypeCastResolvePipeline_DelegatesFloatCast()
    {
        var input = MinimalInput() with { TargetFloat = true };
        var (stage, delegateKind, _) = ZccTypeCastResolve.RunTypeCastResolvePipeline(input);
        Assert.Equal(ZccTypeCastResolve.TypeCastResolveStage.DelegateToCastNode, stage);
        Assert.Equal(ZccTypeCastResolve.CastDelegateKind.Float, delegateKind);
    }

    [Fact]
    public void RunTypeCastResolvePipeline_RawFunctionRejectVirtual()
    {
        var input = MinimalInput() with
        {
            BaseConstant = true,
            BaseRawFunction = true,
            TargetFunctionPointer = true,
            VariantFlags = ZccTypeCastResolve.VarfVirtual,
        };
        var (stage, _, _) = ZccTypeCastResolve.RunTypeCastResolvePipeline(input);
        Assert.Equal(ZccTypeCastResolve.TypeCastResolveStage.FailAndDelete, stage);
    }

    [Fact]
    public void NativeStructReadonlyBlocksWritable_At412()
    {
        Assert.True(ZccTypeCastResolve.NativeStructReadonlyBlocksWritable(
            false, false, ZccTypeCastResolve.MakeVersion(4, 12, 0)));
    }

    private static ZccTypeCastResolve.TypeCastResolveInput MinimalInput() =>
        new(false, false, false, 0, false, false, false, false, false, false, false, false, false, false,
            false, false, false, ZccTypeCastResolve.MakeVersion(4, 14, 1), false, false, false, false, false,
            false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false, false, false);
}
