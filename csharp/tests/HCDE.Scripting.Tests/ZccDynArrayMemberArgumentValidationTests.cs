namespace HCDE.Scripting.Tests;

public class ZccDynArrayMemberArgumentValidationTests
{
    [Fact]
    public void IsObjectPushArgument_MatchesPushAndInsertSlots()
    {
        Assert.True(ZccDynArrayMemberArgumentValidation.IsObjectPushArgument("Push", 0));
        Assert.True(ZccDynArrayMemberArgumentValidation.IsObjectPushArgument("Insert", 1));
        Assert.False(ZccDynArrayMemberArgumentValidation.IsObjectPushArgument("Push", 1));
    }

    [Fact]
    public void ValidateObjectElementArgument_AllowsNullConstant()
    {
        Assert.True(ZccDynArrayMemberArgumentValidation.ValidateObjectElementArgument(
            true, true, false, false, out _));
    }

    [Fact]
    public void ValidateObjectElementArgument_RejectsIncompatibleType()
    {
        Assert.False(ZccDynArrayMemberArgumentValidation.ValidateObjectElementArgument(
            true, false, false, false, out var error));
        Assert.Contains("Type mismatch", error);
    }
}
