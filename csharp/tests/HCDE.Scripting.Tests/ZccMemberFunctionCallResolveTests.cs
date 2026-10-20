namespace HCDE.Scripting.Tests;

public class ZccMemberFunctionCallResolveTests
{
    [Fact]
    public void RequiresEnclosingClass_RejectsGlobalContext()
    {
        Assert.False(ZccMemberFunctionCallResolve.RequiresEnclosingClass(false, out var error));
        Assert.Contains("constant", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("String", "StringStruct")]
    [InlineData("FQuat", "QuaternionStruct")]
    public void TryMapStaticTypeName_MatchesNativeSubstitutions(string name, string expected)
    {
        Assert.True(ZccMemberFunctionCallResolve.TryMapStaticTypeName(name, out var container));
        Assert.Equal(expected, container);
    }

    [Fact]
    public void TryValidateSoundIsValid_RejectsExtraArgs()
        => Assert.False(ZccMemberFunctionCallResolve.TryValidateSoundIsValid(1, out _));

    [Fact]
    public void ShouldMarkReadonlyConstPointer_Requires412()
    {
        Assert.True(ZccMemberFunctionCallResolve.ShouldMarkReadonlyConstPointer(true, 4, 12, 1));
        Assert.False(ZccMemberFunctionCallResolve.ShouldMarkReadonlyConstPointer(true, 4, 11, 0));
    }
}
