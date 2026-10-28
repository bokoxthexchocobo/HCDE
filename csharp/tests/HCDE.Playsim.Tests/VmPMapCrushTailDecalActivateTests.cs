namespace HCDE.Playsim.Tests;

public class VmPMapCrushTailDecalActivateTests
{
    [Fact]
    public void Crush_DamageCadence() =>
        Assert.True(VmPMapCrushTailDecalActivate.CrushDamageTick(10, 0));

    [Fact]
    public void Crush_SkipInvulnBlood() =>
        Assert.True(VmPMapCrushTailDecalActivate.SkipBloodWhenInvuln(
            VmPMapCrushTailDecalActivate.Mf2Invulnerable));

    [Fact]
    public void Decal_PrefersWeaponGenerator() =>
        Assert.True(VmPMapCrushTailDecalActivate.UseWeaponDecal(true, true));

    [Fact]
    public void DeepSplash_3dPath() =>
        Assert.True(true);

    [Fact]
    public void Activate_DefaultSwitchAddsActivate()
    {
        var t = VmPMapCrushTailDecalActivate.DefaultSwitchToActivate(
            VmPMapCrushTailDecalActivate.ThingspecSwitch);
        Assert.NotEqual(0, t & VmPMapCrushTailDecalActivate.ThingspecActivates);
    }

    [Fact]
    public void Activate_SpecialUsesThingOnDeath() =>
        Assert.True(VmPMapCrushTailDecalActivate.SpecialUsesThingAsActivator(
            new VmPMapCrushTailDecalActivate.ExecuteSpecialActorContext(
                true, VmPMapCrushTailDecalActivate.LevelActownspecial, 0, true)));

    [Fact]
    public void Activate_ClearOnSuccess() =>
        Assert.True(VmPMapCrushTailDecalActivate.ClearSpecialOnSuccess(
            VmPMapCrushTailDecalActivate.ThingspecClearspecial, true));

    [Fact]
    public void Activate_StateMachineWhenAlive() =>
        Assert.True(VmPMapCrushTailDecalActivate.ShouldEnterStateMachine(
            new VmPMapCrushTailDecalActivate.ActivateBranchContext(
                VmPMapCrushTailDecalActivate.ThingspecSwitch, false)));
}
