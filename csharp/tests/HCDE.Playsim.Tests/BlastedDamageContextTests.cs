using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedDamageContextTests
{
    [Theory]
    [InlineData(1, 1, 997, 999)]
    [InlineData(2, 3, 994, 997)]
    [InlineData(0, 1, 1000, 999)]
    [InlineData(1, 0, 997, 1000)]
    public void BothImpactHitsUseMoverDamageType(double targetFactor, double moverFactor, int targetHealth, int moverHealth)
    {
        var (sim, mover, target) = Room();
        mover.DamageType = "Fire"; target.DamageType = "Ice";
        target.SetDamageFactor("Fire", targetFactor); mover.SetDamageFactor("Fire", moverFactor);
        mover.SetDamageFactor("Ice", 0);
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(targetHealth, target.Health); Assert.Equal(moverHealth, mover.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShootableDeadMonsterStillReceivesBlastMomentumAndCausesReciprocalHit(bool solid)
    {
        var (sim, mover, target) = Room(); target.Health = 0;
        target.Shootable = true; target.Solid = solid; target.Pushable = true;
        mover.DamageType = "Fire"; mover.SetDamageFactor("Fire", 2);
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(4, target.VelocityX.ToDouble()); Assert.Equal(998, mover.Health);
        Assert.False(target.LastPushFromMovement); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void SaveRestorePreservesTypedImpactOutcome()
    {
        var (sim, mover, target) = Room(); mover.DamageType = "Fire";
        target.SetDamageFactor("Fire", 2); mover.SetDamageFactor("Fire", 3);
        var saved = SimSavegame.Write(sim, true);
        var (restored, loaded, hit) = Room(); SimSavegame.Apply(restored, saved);
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.False(ActorPhysics.TryMove(restored, loaded, 20, 0, out _));
        Assert.Equal(994, hit.Health); Assert.Equal(997, loaded.Health);
        Assert.Equal(target.Health, hit.Health); Assert.Equal(mover.Health, loaded.Health);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        mover.Brain = target.Brain = null; mover.Health = target.Health = 1000;
        mover.DoHarmSpecies = target.DoHarmSpecies = true; mover.PainChance = target.PainChance = 0;
        mover.Mass = 200; target.Mass = 300; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(4);
        return (sim, mover, target);
    }
}
