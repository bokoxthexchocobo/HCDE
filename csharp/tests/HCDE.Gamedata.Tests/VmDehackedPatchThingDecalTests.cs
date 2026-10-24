namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingDecalTests
{
    [Fact]
    public void NormalizeDecalName_TrimsWhitespace() =>
        Assert.Equal("BLOOD", VmDehackedPatchThingDecal.NormalizeDecalName("  BLOOD "));
}
