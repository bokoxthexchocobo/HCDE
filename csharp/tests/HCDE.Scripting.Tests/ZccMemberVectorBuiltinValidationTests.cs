namespace HCDE.Scripting.Tests;

public class ZccMemberVectorBuiltinValidationTests
{
    [Fact]
    public void TryValidateUnaryBuiltin_RejectsLengthArity()
        => Assert.False(ZccMemberVectorBuiltinValidation.TryValidateUnaryBuiltin(1, "Length", out _));

    [Fact]
    public void TryValidatePlusZ_RequiresSingleArgument()
    {
        Assert.True(ZccMemberVectorBuiltinValidation.TryValidatePlusZ(1, out _));
        Assert.False(ZccMemberVectorBuiltinValidation.TryValidatePlusZ(0, out _));
    }
}
