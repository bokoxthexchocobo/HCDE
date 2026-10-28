namespace HCDE.Scripting.Tests;

public class ZccSpaceshipResolveTests
{
    [Fact]
    public void OperatorVersionPolicy_Requires4151()
    {
        Assert.Equal(
            ZccSpaceshipResolve.VersionCheck.Requires4151,
            ZccSpaceshipResolve.OperatorVersionPolicy(ZccSpaceshipResolve.TkLtEqGt, 40000));
    }

    [Fact]
    public void TryFoldConstantSpaceship_Ordering()
    {
        Assert.Equal(-1, ZccSpaceshipResolve.TryFoldConstantSpaceship(true, 1, 2));
        Assert.Equal(1, ZccSpaceshipResolve.TryFoldConstantSpaceship(true, 3, 2));
    }

    [Fact]
    public void ClassifyOperands_RejectsNonNumeric()
    {
        Assert.Equal(
            ZccSpaceshipResolve.VersionCheck.NonNumericOperands,
            ZccSpaceshipResolve.ClassifyOperands(true, false));
    }

    [Fact]
    public void ShouldDeleteOnVersionError_OnlyFor4151Gate()
    {
        Assert.True(ZccSpaceshipResolve.ShouldDeleteOnVersionError(ZccSpaceshipResolve.VersionCheck.Requires4151));
        Assert.False(ZccSpaceshipResolve.ShouldDeleteOnVersionError(ZccSpaceshipResolve.VersionCheck.DeprecatedLtGtEq));
    }
}
