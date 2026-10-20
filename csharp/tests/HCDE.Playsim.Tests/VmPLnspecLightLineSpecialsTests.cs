namespace HCDE.Playsim.Tests;

public class VmPLnspecLightLineSpecialsTests
{
    [Fact]
    public void LightFade_UsesTicsMacro()
    {
        var invoke = VmPLnspecLightLineSpecials.Fade(1, 128, 8);
        Assert.Equal(VmPLnspecArgMacros.Tics(8), invoke.Arg2);
    }

    [Fact]
    public void LightLower_NegatesValue()
    {
        var invoke = VmPLnspecLightLineSpecials.LowerByValue(3, 10);
        Assert.Equal(-10, invoke.Arg1);
    }
}
