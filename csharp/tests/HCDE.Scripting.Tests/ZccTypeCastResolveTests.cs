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
    public void PlanRoute_DelegatesFloat()
    {
        var route = ZccTypeCastResolve.PlanRoute(
            false, false, false, false, false,
            isFloat: true,
            targetInt: false, targetBool: false, targetString: false, targetName: false,
            targetSound: false, targetTranslation: false, targetColor: false,
            targetSpriteFromInt: false, targetTextureFromInt: false, targetClassPointer: false,
            targetFont: false, compatibleVector: false, compatiblePointer: false);
        Assert.Equal(ZccTypeCastResolve.CastResolveRoute.DelegateFloat, route);
    }

    [Fact]
    public void ReadonlyToWritableError_At412()
    {
        Assert.True(ZccTypeCastResolve.ReadonlyToWritableError(
            false, false, ZccTypeCastResolve.MakeVersion(4, 12, 0), ZccTypeCastResolve.MakeVersion(4, 12, 0)));
    }
}
