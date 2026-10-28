namespace HCDE.Playsim.Tests;

public class VmADynLightTickTests
{
    [Fact]
    public void ReleaseLight_InvokesPoolReturn()
    {
        var pool = false;
        VmADynLightTick.ReleaseLight(() => { }, () => { }, () => pool = true);
        Assert.True(pool);
    }

    [Fact]
    public void Activate_PointLightSetsRadius()
    {
        var light = new VmADynLightTick.LightState { LightType = VmADynLightTick.PointLight };
        VmADynLightTick.Activate(light, () => 8, () => 0, () => 0, (_, _, _, _) => { });
        Assert.Equal(8, light.CurrentRadius);
    }

    [Fact]
    public void Tick_NoTarget_Releases()
    {
        var light = new VmADynLightTick.LightState { Active = true, LightType = VmADynLightTick.PointLight };
        var released = false;
        VmADynLightTick.Tick(
            light,
            () => true,
            () => { },
            () => released = true,
            () => true,
            () => true,
            () => 0,
            () => true,
            () => 4,
            () => 0,
            () => 0,
            _ => { },
            () => 4,
            _ => 0,
            () => 0,
            () => { });
        Assert.True(released);
    }

    [Fact]
    public void Tick_FlickerLight_PicksIntensity()
    {
        var light = new VmADynLightTick.LightState { Active = true, LightType = VmADynLightTick.FlickerLight, SpecialF1 = 180 };
        VmADynLightTick.Tick(
            light,
            () => false,
            () => { },
            () => { },
            () => true,
            () => true,
            () => 0,
            () => true,
            () => 10,
            () => 20,
            () => 0,
            _ => { },
            () => 4,
            _ => 100,
            () => 0,
            () => { });
        Assert.Equal(10, light.CurrentRadius);
    }

    [Fact]
    public void UpdateLocation_RelinksWhenActive()
    {
        var light = new VmADynLightTick.LightState { Active = true, CurrentRadius = 5 };
        var relinked = false;
        VmADynLightTick.UpdateLocation(
            light,
            () => true,
            () => (3, 4, 0, 0, 256, 0, 0, 1, 0, 0, 0, 0),
            () => 5,
            () => 0,
            (_, _, _, _) => relinked = true);
        Assert.True(relinked);
        Assert.Equal(3, light.X);
    }
}
