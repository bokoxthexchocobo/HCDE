using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorZBlockerTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void ProjectileExclusionsDoNotApplyDamage(int mode, bool blocked)
    {
        var (sim, missile, target) = Setup();
        target.NonShootable = mode == 1;
        target.Ghost = mode == 2; missile.ThruGhost = mode == 2;
        target.Spectral = mode is 3 or 4; missile.Spectral = mode == 4;
        missile.Rip = mode is 5 or 6; target.DontRip = mode == 6;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? target : null, ActorPhysics.FindZBlocker(sim, missile));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(16, false, 16, true)]
    [InlineData(16, false, 17, false)]
    [InlineData(-16, false, 17, true)]
    [InlineData(-16, true, 17, false)]
    public void ProjectilePassHeightPreservesBoundaryAndCompatibility(int height, bool clip, int z, bool blocked)
    {
        var (sim, missile, target) = Setup(clip);
        target.ProjectilePassHeight = Fixed.FromInt(height); missile.Z = Fixed.FromInt(z);
        Assert.Equal(blocked ? target : null, ActorPhysics.FindZBlocker(sim, missile));
    }

    [Fact]
    public void FullScanSelectsHighestWhileQuickScanStopsAtFirst()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var actor = sim.AddBot(0, 0); var lower = sim.AddBot(0, 0); var higher = sim.AddBot(0, 0);
        higher.Z = Fixed.FromInt(10);
        Assert.Same(lower, ActorPhysics.FindZBlocker(sim, actor));
        Assert.Same(higher, ActorPhysics.FindZBlocker(sim, actor, quick: false));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OwnerCollisionRequiresHitOwner(bool hitOwner)
    {
        var (sim, missile, target) = Setup(); target.X = Fixed.FromInt(200);
        missile.X = missile.Owner.X; missile.HitOwner = hitOwner;
        Assert.Equal(hitOwner ? missile.Owner : null, ActorPhysics.FindZBlocker(sim, missile));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void RipperBossAndLevelRestrictionsRemainBlockers(int mode, bool blocked)
    {
        var (sim, missile, target) = Setup(); missile.Rip = true; missile.RipperLevel = 2;
        target.Boss = mode == 1; missile.NoBossRip = mode == 1;
        target.RipLevelMin = mode == 2 ? 3 : 0; target.RipLevelMax = mode == 3 ? 1 : 0;
        Assert.Equal(blocked ? target : null, ActorPhysics.FindZBlocker(sim, missile));
    }

    [Fact]
    public void OwnerSpeciesPassageSurvivesSaveRestore()
    {
        var (sim, missile, target) = Setup(); missile.MThruSpecies = true;
        Assert.Null(ActorPhysics.FindZBlocker(sim, missile));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, victim) = Setup(); restored.RestoreState(state);
        Assert.True(loaded.MThruSpecies);
        Assert.Null(ActorPhysics.FindZBlocker(restored, loaded));
        victim.Species = "Different";
        Assert.Same(victim, ActorPhysics.FindZBlocker(restored, loaded));
        Assert.Equal(20, target.Health);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(bool clip = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] },
            compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(0, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        return (sim, missile, target);
    }
}
