using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorImpactLifetimeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void OrdinaryAcsHitDiscardsPuffAndDoesNotAssignRequestedTid(int flags)
    {
        var (sim, source, target) = Scene(); var next = sim.CaptureState(true).NextActorId;
        Attack(sim, source, 7, flags);
        Assert.Equal(993, target.Health); Assert.Equal(source.Id, target.LastDamageSourceId);
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.DoesNotContain(sim.Actors, actor => actor.ThingId == 77);
        Assert.DoesNotContain(sim.Thinkers.SnapshotOrder(), thinker => thinker is PuffActor);
        var saved = sim.CaptureState(true); Assert.Equal(next + 1, saved.NextActorId);
        Assert.DoesNotContain(saved.Actors, actor => actor.PuffIdentity.HasValue);
        var control = AuthoritySimulation.Start(new PlayLevel()); control.SpawnHitscanPuff(0, 0, 0, noRandomZ: flags == 1);
        Assert.Equal(control.CaptureState().PuffRandomState, saved.PuffRandomState);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public void ZeroDamageOrdinaryHitConsumesNeitherPuffIdNorRandomness(bool shooterForcePain, int flags)
    {
        var (sim, source, target) = Scene(); source.ForcePain = shooterForcePain;
        var next = sim.CaptureState(true).NextActorId; var state = target.States.Current;
        Attack(sim, source, 0, flags);
        Assert.Equal(1000, target.Health); Assert.Equal(state, target.States.Current); Assert.Null(target.LastDamageSourceId);
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Equal(next, sim.CaptureState(true).NextActorId);
        Assert.Null(sim.CaptureState().PuffRandomState);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 7)]
    [InlineData(true, 0)]
    [InlineData(true, 7)]
    public void ProtectedHitRetainsVisiblePuffWithRequestedTid(bool dormant, int damage)
    {
        var (sim, source, target) = Scene(); target.Dormant = dormant; target.Invulnerable = !dormant;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Attack(sim, source, damage, 1);
        var puff = sim.Actors.OfType<PuffActor>().Single(); Assert.Equal(77, puff.ThingId);
        Assert.Same(source, puff.DamageSource); Assert.Equal(1000, target.Health);
        Assert.Equal(hit.Z, puff.Z.ToDouble()); Assert.Equal(0, puff.States.Current);
    }

    [Fact]
    public void FlaggedActorHitReplaysDamageIdsAndRandomness()
    {
        var (sim, source, _) = Scene(); var before = SimSavegame.Write(sim, true);
        Attack(sim, source, 7, 1); sim.Tick(); var expected = SimSavegame.Write(sim, true); var hash = sim.Checksum;
        SimSavegame.Apply(sim, before); Attack(sim, source, 7, 1); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(hash, sim.Checksum);
    }

    private static void Attack(AuthoritySimulation sim, Actor source, int damage, int flags)
    {
        var stack = new List<int> { 0, 0, 0, damage, 0, 0, 256 << 16, flags, 77 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source }, [],
            new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 9, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) Scene()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        var target = sim.AddBot(100, 0); target.Brain = null; target.Health = 1000; target.NoPain = true;
        return (sim, sim.Players.Single(), target);
    }
}
