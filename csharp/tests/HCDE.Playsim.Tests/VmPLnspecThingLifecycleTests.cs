namespace HCDE.Playsim.Tests;

public class VmPLnspecThingLifecycleTests
{
    [Fact]
    public void SetSpecial_WritesActivatorArgsWhenTidZero()
    {
        Assert.True(VmPLnspecThingLifecycle.SetSpecialOnActivator(0));
    }

    [Fact]
    public void ChangeTid_IteratesWhenOldTidNonZero()
    {
        Assert.False(VmPLnspecThingLifecycle.ChangeTidOnActivator(5));
    }

    [Fact]
    public void DamageThing_NegativeArgHealsPlayer()
    {
        Assert.True(VmPLnspecThingLifecycle.DamageThingNegativeHeals(-10));
    }

    [Fact]
    public void HealThing_ClampsToMaxSoulsphere()
    {
        Assert.Equal(80, VmPLnspecThingLifecycle.HealClampedHealth(120, 80));
    }

    [Fact]
    public void Activation_SwapsSwitchBits()
    {
        var t = VmPLnspecThingLifecycle.ThingSpecActivate | VmPLnspecThingLifecycle.ThingSpecSwitch;
        var cleared = VmPLnspecThingLifecycle.ApplyActivationClear(t, VmPLnspecThingLifecycle.ThingSpecActivate);
        var next = VmPLnspecThingLifecycle.ApplyActivationSetDeactivate(
            cleared, VmPLnspecThingLifecycle.ThingSpecDeactivate, true);
        Assert.Equal(VmPLnspecThingLifecycle.ThingSpecDeactivate | VmPLnspecThingLifecycle.ThingSpecSwitch, next);
    }

    [Fact]
    public void Activate_CountsIteratorHits()
    {
        Assert.True(VmPLnspecThingLifecycle.ActivateIterateTid(3));
    }

    [Fact]
    public void Remove_UsesActivatorWhenTidZero()
    {
        Assert.True(VmPLnspecThingLifecycle.RemoveActivatorPath(0));
    }

    [Fact]
    public void Destroy_MassacreWhenBothArgsZero()
    {
        Assert.True(VmPLnspecThingLifecycle.DestroyMassacre(0, 0));
    }
}
