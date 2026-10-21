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
}
