using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponPuffInflictorTests
{
    [Theory]
    [InlineData(WeaponKind.Shotgun, 7)]
    [InlineData(WeaponKind.SuperShotgun, 20)]
    [InlineData(WeaponKind.Fist, 1)]
    [InlineData(WeaponKind.Chainsaw, 1)]
    public void EveryActorHitPelletConsumesAndDiscardsItsOwnPuff(WeaponKind weapon, uint count)
    {
        var (sim, source, target) = Scene(); target.X = Fixed.FromInt(32); target.NoPain = true;
        source.Inventory.Weapons |= weapon; source.Inventory.Selected = weapon; source.Inventory.Shells = 100;
        var next = sim.CaptureState(true).NextActorId;
        Assert.True(HitscanCombat.Fire(sim, source)); Assert.True(target.Health < 1000);
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Equal(next + count, sim.CaptureState(true).NextActorId);
        var control = AuthoritySimulation.Start(new PlayLevel());
        for (var i = 0; i < count; i++) control.SpawnHitscanPuff(0, 0, 0);
        Assert.Equal(control.CaptureState().PuffRandomState, sim.CaptureState().PuffRandomState);
        Assert.Equal(source.Id, target.LastDamageSourceId);
    }

    [Fact]
    public void ShooterForcePainDoesNotBecomePuffForcePain()
    {
        var (sim, source, target) = Scene(); source.ForcePain = true;
        target.PainChance = 0; target.PainThreshold = 100;
        Assert.True(HitscanCombat.Fire(sim, source));
        Assert.Equal(target.SpawnState, target.States.Current); Assert.Equal(source.Id, target.LastDamageSourceId);
        Assert.Empty(sim.Actors.OfType<PuffActor>());
    }

    [Fact]
    public void ShooterPainlessDoesNotSuppressPuffPain()
    {
        var (sim, source, target) = Scene(); source.Painless = true; target.PainChance = 256;
        Assert.True(HitscanCombat.Fire(sim, source)); Assert.Equal(target.PainState, target.States.Current);
    }

    [Fact]
    public void ShooterFoilBuddhaDoesNotBypassProtectionThroughOrdinaryPuff()
    {
        var (sim, source, target) = Scene(); source.FoilBuddha = true; target.Buddha = true; target.Health = 1;
        Assert.True(HitscanCombat.Fire(sim, source)); Assert.Equal(1, target.Health); Assert.False(target.Killed);
    }

    [Fact]
    public void SpectralShooterDoesNotMakeDefaultPuffSpectral()
    {
        var (sim, source, target) = Scene(); source.Spectral = true; target.Spectral = true;
        Assert.True(HitscanCombat.Fire(sim, source)); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void ShooterPierceArmorDoesNotBecomePuffPierceArmor()
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 100 }]
        };
        var sim = AuthoritySimulation.Start(level); var control = AuthoritySimulation.Start(level);
        var source = sim.Players.Single(player => player.PlayerNum == 0); source.PierceArmor = true;
        var target = sim.Players.Single(player => player.PlayerNum == 1);
        var controlTarget = control.Players.Single(player => player.PlayerNum == 1);
        foreach (var player in new[] { target, controlTarget }) { player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50; }
        Assert.True(HitscanCombat.Fire(sim, source));
        Assert.True(HitscanCombat.Fire(control, control.Players.Single(player => player.PlayerNum == 0)));
        Assert.Equal(controlTarget.Health, target.Health); Assert.Equal(controlTarget.Inventory.Armor, target.Inventory.Armor);
        Assert.True(target.Inventory.Armor < 100);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProtectedActorHitLeavesVisiblePuff(bool dormant)
    {
        var (sim, source, target) = Scene(); target.Dormant = dormant; target.Invulnerable = !dormant;
        Assert.True(HitscanCombat.Fire(sim, source)); Assert.Equal(1000, target.Health);
        var puff = sim.Actors.OfType<PuffActor>().Single(); Assert.Same(source, puff.DamageSource);
        Assert.False(puff.Destroyed); Assert.Equal(BamAngle.FromDegrees(180), puff.Angle);
        var before = SimSavegame.Write(sim, true); puff.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OrdinaryHitConsumesPuffDrawsAndIdWithoutLeavingSavedActor()
    {
        var (sim, source, _) = Scene(); var next = sim.CaptureState(true).NextActorId;
        Assert.True(HitscanCombat.Fire(sim, source));
        Assert.Empty(sim.Actors.OfType<PuffActor>());
        var state = sim.CaptureState(true);
        Assert.Equal(next + 1, state.NextActorId); Assert.NotNull(state.PuffRandomState);
        Assert.DoesNotContain(state.Actors, pose => pose.PuffIdentity.HasValue);
        var control = AuthoritySimulation.Start(new PlayLevel()); control.SpawnHitscanPuff(0, 0, 0);
        Assert.Equal(control.CaptureState().PuffRandomState, state.PuffRandomState);
    }

    [Fact]
    public void TemporaryPuffIsDiscardedWhenDamageCallbackThrows()
    {
        var (sim, source, _) = Scene(); var target = new ThrowingTarget { Health = 100 };
        var hit = new CombatTrace.LineAttackHit(true, target, 100, 0, 28);
        Assert.Throws<InvalidOperationException>(() => LineAttackImpact.ApplyActorDamage(
            sim, source, target, hit, new BamAngle(0), 7, "Hitscan", false));
        Assert.Empty(sim.Actors.OfType<PuffActor>());
        Assert.DoesNotContain(sim.Thinkers.SnapshotOrder(), thinker => thinker is PuffActor);
        var puff = Assert.IsType<PuffActor>(target.Inflictor);
        Assert.True(puff.Destroyed); Assert.Same(source, puff.DamageSource);
    }

    [Fact]
    public void TemporaryPuffDamageAndRandomnessReplayFromIdentitySnapshot()
    {
        var (sim, source, target) = Scene(); var before = SimSavegame.Write(sim, true);
        Assert.True(HitscanCombat.Fire(sim, source)); sim.Tick(); var expected = SimSavegame.Write(sim, true); var hash = sim.Checksum;
        SimSavegame.Apply(sim, before); Assert.True(HitscanCombat.Fire(sim, source)); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(source.Id, target.LastDamageSourceId);
    }

    private sealed class ThrowingTarget : Actor
    {
        public Actor? Inflictor { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        { Inflictor = inflictor; throw new InvalidOperationException("Expected damage callback failure."); }
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) Scene()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        var target = sim.AddBot(100, 0); target.Brain = null; target.Health = 1000;
        return (sim, sim.Players.Single(), target);
    }
}
