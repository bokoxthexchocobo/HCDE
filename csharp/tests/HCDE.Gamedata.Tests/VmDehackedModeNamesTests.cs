namespace HCDE.Gamedata.Tests;

public class VmDehackedModeNamesTests
{
    [Fact]
    public void PatchModes_IncludesThing() =>
        Assert.Contains("Thing", VmDehackedModeNames.PatchModes);
}
