namespace HCDE.Scripting.Tests;

public class ZccFunctionCallBuiltinCastsTests
{
    [Theory]
    [InlineData("int", VmValueType.Integer)]
    [InlineData("Float", VmValueType.Float)]
    [InlineData("Name", VmValueType.String)]
    public void TryResolveTypeCast_MatchesNativeBuiltinTypes(string name, VmValueType expected)
    {
        Assert.True(ZccFunctionCallBuiltinCasts.TryResolveTypeCast(name, 1, out var type));
        Assert.Equal(expected, type);
    }

    [Fact]
    public void TryResolveTypeCast_RejectsWrongArity()
        => Assert.False(ZccFunctionCallBuiltinCasts.TryResolveTypeCast("int", 2, out _));
}
