using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetSwitchDamageImmunityTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void TargetEligibilityDoesNotRepeatProjectileImmunity(bool group, int infighting)
    {
        var (sim, hunter, source) = Room(group); sim.Infighting = infighting - 1;
        Assert.True(hunter.ProjectileImmune(source));
        Assert.Equal(infighting > 0, hunter.OkayToSwitchTarget(source, hunter.Brain!));
        Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, -1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public void ForcedDamageRetaliatesSubjectToTargetEligibility(bool group, int infighting)
    {
        var (sim, hunter, source) = Room(group); sim.Infighting = infighting;
        Assert.Equal(1, ActorDamage.Apply(hunter, 1, source, DamageFlags.Forced | DamageFlags.NoPain).HealthLost);
        Assert.Equal(infighting < 0 ? (uint?)null : source.Id, hunter.Brain!.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryDamageStillHonorsProjectileImmunity(bool group)
    {
        var (sim, hunter, source) = Room(group); sim.Infighting = 0;
        Assert.Equal(0, ActorDamage.Apply(hunter, 1, source, DamageFlags.NoPain).HealthLost);
        Assert.Null(hunter.Brain!.TargetId);
        Assert.Equal(1000, hunter.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ForcedDamageDoesNotBypassExplicitTargetRestrictions(int restriction)
    {
        var (sim, hunter, source) = Room(false); sim.Infighting = 0;
        if (restriction == 0) hunter.NoInfightSpecies = true;
        else if (restriction == 1) hunter.InfightingGroup = source.InfightingGroup = 7;
        else source.NeverTarget = true;
        Assert.Equal(1, ActorDamage.Apply(hunter, 1, source, DamageFlags.Forced | DamageFlags.NoPain).HealthLost);
        Assert.Null(hunter.Brain!.TargetId);
    }

    [Fact]
    public void SavedForcedRetaliationReplaysTargetAndThreshold()
    {
        var (sim, hunter, source) = Room(false); sim.Infighting = 0;
        var saved = SimSavegame.Write(sim, true);
        ActorDamage.Apply(hunter, 1, source, DamageFlags.Forced | DamageFlags.NoPain);
        Assert.Equal(source.Id, hunter.Brain!.TargetId);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        ActorDamage.Apply(hunter, 1, source, DamageFlags.Forced | DamageFlags.NoPain);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor, Actor) Room(bool group)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var hunter = sim.AddBot(0, 0); var source = sim.AddBot(128, 0, group ? 3001 : 3004);
        hunter.Health = 1000; hunter.NoPain = true; hunter.SetChaseThreshold(0, false);
        if (group) hunter.ProjectileGroup = source.ProjectileGroup = 7;
        return (sim, hunter, source);
    }
}
