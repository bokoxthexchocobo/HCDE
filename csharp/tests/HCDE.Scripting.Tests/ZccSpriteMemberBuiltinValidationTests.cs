namespace HCDE.Scripting.Tests;

public class ZccSpriteMemberBuiltinValidationTests
{
    [Theory]
    [InlineData("IsEmpty", 0)]
    [InlineData("IsFixed", 1)]
    [InlineData("IsKeep", 2)]
    public void TryGetEqualityConstant_MatchesNativeSpriteIds(string method, int expected)
    {
        Assert.True(ZccSpriteMemberBuiltinValidation.TryGetEqualityConstant(method, out var constant));
        Assert.Equal(expected, constant);
    }

    [Fact]
    public void UsesGreaterOrEqualCompare_ForIsValid()
        => Assert.True(ZccSpriteMemberBuiltinValidation.UsesGreaterOrEqualCompare("IsValid"));
}
