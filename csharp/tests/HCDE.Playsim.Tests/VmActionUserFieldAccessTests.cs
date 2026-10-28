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
    public void IsUserArrayField_RejectsNonArray() =>
        Assert.False(VmActionUserFieldAccess.IsUserArrayField(true, 0, false, true));

    [Fact]
    public void ArrayIndexInBounds_RejectsNegative() =>
        Assert.False(VmActionUserFieldAccess.ArrayIndexInBounds(-1, 4));

    [Fact]
    public void ShouldSetUserArrayElement_RequiresIndexOk() =>
        Assert.False(VmActionUserFieldAccess.ShouldSetUserArrayElement(true, false));

    [Fact]
    public void ShouldSetUserArrayElement_WhenFieldAndIndexOk() =>
        Assert.True(VmActionUserFieldAccess.ShouldSetUserArrayElement(true, true));

    [Fact]
    public void UserArrayElementByteOffset_MatchesStride() =>
        Assert.Equal(20, VmActionUserFieldAccess.UserArrayElementByteOffset(fieldOffset: 8, elementSize: 4, pos: 3));

    [Fact]
    public void CanCommitUserScalarWrite_WhenLookupValid() =>
        Assert.True(VmActionUserFieldAccess.CanCommitUserScalarWrite(true, 0, true));

    [Fact]
    public void CanCommitUserArrayWrite_WhenFieldAndIndexValid() =>
        Assert.True(VmActionUserFieldAccess.CanCommitUserArrayWrite(true, 0, true, true, pos: 1, elementCount: 4));
}
