namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfParamDefaultsTests
{
    [Fact]
    public void MushroomVerticalRange_UsesDefaultWhenZero()
    {
        Assert.Equal(4.0, VmDehackedMbfParamDefaults.MushroomVerticalRange(0));
    }
}
