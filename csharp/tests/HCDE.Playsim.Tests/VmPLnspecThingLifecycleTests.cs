namespace HCDE.Playsim.Tests;

public class VmPLnspecThingLifecycleTests
{
    [Fact]
    public void SetSpecial_WritesActivatorArgsWhenTidZero() =>
        Assert.True(VmPLnspecThingLifecycle.ApplySpecialToActivator(0, new object()));

    [Fact]
    public void ChangeTid_IteratesWhenOldTidNonZero() =>
        Assert.False(VmPLnspecThingLifecycle.ChangeTidOnActivator(5));

    [Fact]
    public void DamageThing_NegativeArgHealsPlayer()
    {
        var r = VmPLnspecThingLifecycle.EvaluateDamage(-5, true);
        Assert.True(r.HealPlayer);
    }

    [Fact]
    public void HealThing_ClampsToMaxSoulsphere()
    {
        var o = VmPLnspecThingLifecycle.ApplyHeal(90, 20, 100, 100, 80, new object());
        Assert.Equal(100, o.NewHealth);
    }

    [Fact]
    public void Activation_SwapsSwitchBits()
    {
        var t = VmPLnspecThingLifecycle.ThingSpecActivate | VmPLnspecThingLifecycle.ThingSpecSwitch;
        var next = VmPLnspecThingLifecycle.ToggleActivationType(t, true);
        Assert.Equal(VmPLnspecThingLifecycle.ThingSpecDeactivate | VmPLnspecThingLifecycle.ThingSpecSwitch, next);
    }

    [Fact]
    public void Activate_CountsIteratorHits()
    {
        var r = VmPLnspecThingLifecycle.RunActivate(3, 2, false);
        Assert.Equal(2, r.Count);
    }

    [Fact]
    public void Remove_UsesActivatorWhenTidZero() =>
        Assert.True(VmPLnspecThingLifecycle.RemoveByActivator(0));

    [Fact]
    public void Destroy_MassacreWhenBothArgsZero()
    {
        var m = VmPLnspecThingLifecycle.ResolveDestroyMode(0, 0);
        Assert.True(m.Massacre);
    }
}
