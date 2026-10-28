namespace HCDE.Gamedata.Tests;

public class VmMapInfoToggleOptionsTests
{
    [Fact]
    public void ParseOptionalBool_FlagWhenOmitted()
    {
        Assert.True(VmMapInfoToggleOptions.ParseOptionalBool(false, 0, true));
        Assert.False(VmMapInfoToggleOptions.ParseOptionalBool(true, 0, true));
    }
}
