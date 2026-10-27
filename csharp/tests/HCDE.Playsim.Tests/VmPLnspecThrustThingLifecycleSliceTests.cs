namespace HCDE.Playsim.Tests;

public class VmPLnspecThrustThingLifecycleSliceTests
{
    [Fact]
    public void ThrustThingHelper_ClampsVelocity()
    {
        VmPLnspecThrustThingLifecycleSlice.ThrustThingHelper(0, 0, 0, 100, false, out var vx, out var vy);
        Assert.Equal(VmPLnspecThrustThingLifecycleSlice.MaxMove, vx);
        Assert.Equal(0, vy);
    }

    [Fact]
    public void ThrustThing_HexenBacksideRejectsActivator()
    {
        Assert.False(VmPLnspecThrustThingLifecycleSlice.ThrustThing(0, 8, false, 0, new VmPLnspecThrustThingLifecycleSlice.LineContext
        {
            HasActivator = true,
            BackSide = true,
            LevelFlags2 = VmPLnspecThrustThingLifecycleSlice.Level2HexenHack,
            ThrustActivator = (_, _, _) => true,
        }));
    }

    [Fact]
    public void ThrustThingZ_DownUpNegatesThrust()
    {
        Assert.Equal(-2, VmPLnspecThrustThingLifecycleSlice.ThrustZValue(8, true));
    }

    [Fact]
    public void ToggleActivationType_SwitchClearsActivate()
    {
        var next = VmPLnspecThrustThingLifecycleSlice.ToggleActivationType(
            VmPLnspecThrustThingLifecycleSlice.ThingSpecActivate | VmPLnspecThrustThingLifecycleSlice.ThingSpecSwitch,
            true);
        Assert.Equal(
            VmPLnspecThrustThingLifecycleSlice.ThingSpecDeactivate | VmPLnspecThrustThingLifecycleSlice.ThingSpecSwitch,
            next);
    }

    [Fact]
    public void DamageThing_ZeroUsesTelefrag()
    {
        var dmg = 0;
        VmPLnspecThrustThingLifecycleSlice.DamageThing(0, 0, new VmPLnspecThrustThingLifecycleSlice.LineContext
        {
            HasActivator = true,
            DamageActivator = (d, _) => { dmg = d; return true; },
        });
        Assert.Equal(VmPLnspecThrustThingLifecycleSlice.TelefragDamage, dmg);
    }

    [Fact]
    public void ApplyHealCap_RespectsMax()
    {
        Assert.Equal(100, VmPLnspecThrustThingLifecycleSlice.ApplyHealCap(90, 20, 100));
    }
}
