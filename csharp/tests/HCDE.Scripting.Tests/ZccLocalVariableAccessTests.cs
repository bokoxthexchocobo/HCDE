namespace HCDE.Scripting.Tests;

public class ZccLocalVariableAccessTests
{
    [Fact]
    public void ShouldLoadThroughOutPointer_MatchesNativeEmitBranch()
    {
        Assert.True(ZccLocalVariableAccess.ShouldLoadThroughOutPointer(false, true));
        Assert.False(ZccLocalVariableAccess.ShouldLoadThroughOutPointer(true, true));
    }

    [Fact]
    public void IsOutVariable_DetectsVarfOut()
        => Assert.True(ZccLocalVariableAccess.IsOutVariable(ZccLocalVariableAccess.VarFlagOut));
}
