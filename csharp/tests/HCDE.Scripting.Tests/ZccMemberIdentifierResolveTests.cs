namespace HCDE.Scripting.Tests;

public class ZccMemberIdentifierResolveTests
{
    [Fact]
    public void RemapColorMemberTarget_MapsColorToStruct()
    {
        Assert.Equal("ColorStruct", ZccMemberIdentifierResolve.RemapColorMemberTarget("Color"));
        Assert.Equal("Vector3", ZccMemberIdentifierResolve.RemapColorMemberTarget("Vector3"));
    }

    [Fact]
    public void TryFindContainerType_ResolvesRegisteredContainer()
    {
        var map = new Dictionary<string, string> { ["Actor"] = "Actor" };
        Assert.True(ZccMemberIdentifierResolve.TryFindContainerType("Actor", map, out var type));
        Assert.Equal("Actor", type);
    }

    [Theory]
    [InlineData("ColorStruct", true)]
    [InlineData("MyStruct", true)]
    [InlineData("Actor*", true)]
    [InlineData("int", false)]
    public void IsValidMemberAccessTarget_MatchesNativeGates(string valueType, bool expected)
        => Assert.Equal(expected, ZccMemberIdentifierResolve.IsValidMemberAccessTarget(valueType));

    [Fact]
    public void StaticContextError_FormatsNativeMessage()
        => Assert.Contains("static context", ZccMemberIdentifierResolve.StaticContextError("Actor", "Health"));
}
