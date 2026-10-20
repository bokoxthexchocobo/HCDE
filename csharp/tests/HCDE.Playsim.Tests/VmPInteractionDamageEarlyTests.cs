namespace HCDE.Playsim.Tests;

public class VmPInteractionDamageEarlyTests
{
    [Fact]
    public void DormantBlocks_ReturnsMinusOne()
    {
        var ctx = new VmPInteractionDamageEarly.DamageEarlyContext(
            false, true, false, true, 0, 10, 1000, 0, false, false, false, false, false, 0, false, 10);
        Assert.Equal(-1, VmPInteractionDamageEarly.EvaluateEarlyDamage(ctx));
    }

    [Fact]
    public void SpectralPlayerBlock_ReturnsMinusOne()
    {
        var ctx = new VmPInteractionDamageEarly.DamageEarlyContext(
            false, true, false, false, 0, 10, 1000, 0, true, false, true, false, false, 1, false, 10);
        Assert.Equal(-1, VmPInteractionDamageEarly.EvaluateEarlyDamage(ctx));
    }
}
