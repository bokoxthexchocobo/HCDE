namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandBotCenterTests
{
    [Fact]
    public void ApplyCenterViewNative_SetsCentering()
    {
        int? player = null;
        VmDNetDoCommandBotCenter.ApplyCenterViewNative(2, (p, c) => { player = p; Assert.True(c); });
        Assert.Equal(2, player);
    }
}
