namespace HCDE.Gamedata.Tests;

public class VmAnimSwitchRuntimeTests
{
    [Fact]
    public void ParseSwitchLump_ReadsPairs()
    {
        var lump = "SW1BRCOM SW2BRCOM" + new string('\0', 2) + new string('\0', 20);
        var pairs = VmAnimSwitchRuntime.ParseSwitchLumpEntries(lump);
        Assert.Equal("SW1BRCOM", pairs[0].Off);
        Assert.Equal("SW2BRCOM", pairs[0].On);
    }

    [Fact]
    public void FindSwitch_UsesBinarySearch()
    {
        var defs = new List<VmSwitchDef>
        {
            new() { PreTextureIndex = 10 },
            new() { PreTextureIndex = 20 }
        };
        Assert.Equal(20, VmAnimSwitchRuntime.FindSwitch(defs, 20)!.PreTextureIndex);
    }
}
