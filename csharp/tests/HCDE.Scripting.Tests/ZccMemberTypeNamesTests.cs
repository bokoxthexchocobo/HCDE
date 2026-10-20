using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccMemberTypeNamesTests
{
    [Theory]
    [InlineData("Int", VmValueType.Integer)]
    [InlineData("float", VmValueType.Float)]
    [InlineData("Double", VmValueType.Float)]
    public void TryResolveBasicTypeName_MapsNativeAliases(string name, VmValueType expected)
    {
        Assert.True(ZccMemberTypeNames.TryResolveBasicTypeName(name, out var type));
        Assert.Equal(expected, type);
    }

    [Fact]
    public void TryResolveBasicTypeName_RejectsUnknown()
        => Assert.False(ZccMemberTypeNames.TryResolveBasicTypeName("Name", out _));
}
