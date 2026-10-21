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
}
