namespace HCDE.Net.Core.Tests;

public class DemoCommandPolicyTests
{
    [Fact]
    public void IsAllowedTicEvent_RejectsUnknownOpcode() =>
        Assert.False(DemoCommandPolicy.IsAllowedTicEvent(255));

    [Fact]
    public void IsAllowedTicEvent_AllowsSayAndTaunt() =>
        Assert.True(DemoCommandPolicy.IsAllowedTicEvent((byte)DemoCommand.Say))
        && Assert.True(DemoCommandPolicy.IsAllowedTicEvent((byte)DemoCommand.Taunt));

    [Fact]
    public void IsAllowedClientInput_BlocksServerOnlyChangemap() =>
        Assert.False(DemoCommandPolicy.IsAllowedClientInput((byte)DemoCommand.ChangeMap))
        && Assert.True(DemoCommandPolicy.IsAllowedClientInput((byte)DemoCommand.Say));

    [Fact]
    public void IsAllowedClientInput_AllowsGiveCheatOnWire() =>
        Assert.True(DemoCommandPolicy.IsAllowedClientInput((byte)DemoCommand.GiveCheat));
}
