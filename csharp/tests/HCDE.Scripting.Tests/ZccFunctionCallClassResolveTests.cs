namespace HCDE.Scripting.Tests;

public class ZccFunctionCallClassResolveTests
{
    [Fact]
    public void TryValidateArguments_RejectsNullSlots()
    {
        Assert.False(ZccFunctionCallClassResolve.TryValidateArguments([1, null], out var index));
        Assert.Equal(1, index);
    }

    [Fact]
    public void CanCallFromConstantDeclaration_MatchesNativeGate()
    {
        Assert.False(ZccFunctionCallClassResolve.CanCallFromConstantDeclaration(false));
        Assert.True(ZccFunctionCallClassResolve.CanCallFromConstantDeclaration(true));
    }
}
