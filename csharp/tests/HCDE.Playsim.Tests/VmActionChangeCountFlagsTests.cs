namespace HCDE.Playsim.Tests;

public class VmActionChangeCountFlagsTests
{
    [Fact]
    public void ApplyTriStateFlag_ClearsWhenZero() =>
        Assert.Equal(0, VmActionChangeCountFlags.ApplyTriStateFlag(VmActionChangeCountFlags.MfCountkill, VmActionChangeCountFlags.MfCountkill, 0));

    [Fact]
    public void CountsAsKill_RequiresPositiveHealth() =>
        Assert.True(VmActionChangeCountFlags.CountsAsKill(true, 1));
}
