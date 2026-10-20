namespace HCDE.Client.Tests;

public class VmDMainWipePolicyTests
{
    [Fact]
    public void Clamp_InvalidReturnsMelt()
    {
        Assert.Equal(VmDMainWipePolicy.DefaultWipeMelt, VmDMainWipePolicy.Clamp(99));
    }

    [Fact]
    public void IsValid_AcceptsInRange()
    {
        Assert.True(VmDMainWipePolicy.IsValid(2));
        Assert.False(VmDMainWipePolicy.IsValid(99));
    }
}
