namespace HCDE.Client.Tests;

public class VmAutomapColorsetIndexTests
{
    [Fact]
    public void ColorNames_MatchesNativeTableLength()
    {
        Assert.Equal(26, VmAutomapColorsetIndex.ColorNameCount);
    }

    [Fact]
    public void Indices_AlignWithFirstEntries()
    {
        Assert.Equal("Background", VmAutomapColorsetIndex.ColorNames[VmAutomapColorsetIndex.Background]);
        Assert.Equal("PortalColor", VmAutomapColorsetIndex.ColorNames[VmAutomapColorsetIndex.PortalColor]);
    }
}
