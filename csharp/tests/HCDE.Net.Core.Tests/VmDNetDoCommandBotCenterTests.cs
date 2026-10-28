namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandBotCenterTests
{
    [Fact]
    public void ApplyAddBotNative_InvokesWhenLevelPresent()
    {
        int? invoked = null;
        VmDNetDoCommandBotCenter.ApplyAddBotNative("level", "stream", 2, (_, _, p) => invoked = p);
        Assert.Equal(2, invoked);
    }

    [Fact]
    public void ApplyKillBotsNative_RemovesAndPrints()
    {
        var removed = false;
        string? printed = null;
        VmDNetDoCommandBotCenter.ApplyKillBotsNative("level", (_, _) => removed = true, s => printed = s);
        Assert.True(removed);
        Assert.Contains("Removed all bots", printed);
    }

    [Fact]
    public void ApplyCenterViewNative_SetsCentering()
    {
        int? player = null;
        VmDNetDoCommandBotCenter.ApplyCenterViewNative(2, (p, c) => { player = p; Assert.True(c); });
        Assert.Equal(2, player);
    }
}
