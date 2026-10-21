namespace HCDE.Playsim.Tests;

public class VmActionUserFieldAccessTests
{
    [Fact]
    public void SetViewRollOnRef_RequiresActor() =>
        Assert.True(VmActionUserFieldAccess.SetViewRollOnRef(new object()));

    [Fact]
    public void IsUserScalarField_RejectsNativeFlag() =>
        Assert.False(VmActionUserFieldAccess.IsUserScalarField(true, VmActionUserFieldAccess.VarfNative, true));

    [Fact]
    public void IsUserScalarField_AcceptsScalar() =>
        Assert.True(VmActionUserFieldAccess.IsUserScalarField(true, 0, true));

    [Fact]
    public void ShouldSetUserScalar_WhenFieldValid() =>
        Assert.True(VmActionUserFieldAccess.ShouldSetUserScalar(true));

    [Fact]
    public void IsUserArrayField_RejectsNonArray() =>
        Assert.False(VmActionUserFieldAccess.IsUserArrayField(true, 0, false, true));

    [Fact]
    public void ArrayIndexInBounds_RejectsNegative() =>
        Assert.False(VmActionUserFieldAccess.ArrayIndexInBounds(-1, 4));

    [Fact]
    public void ShouldSetUserArrayElement_RequiresIndexOk() =>
        Assert.False(VmActionUserFieldAccess.ShouldSetUserArrayElement(true, false));
}
