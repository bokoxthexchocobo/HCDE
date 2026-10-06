using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileBlockProbeTests
{
    [Theory]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, true)]
    public void RipperProbePassesOnlyNonshootableCorpses(bool rip, bool shootable, bool dead, bool blocked)
    {
        var (sim, missile, target) = Setup(); missile.Rip = rip;
        target.Shootable = shootable; target.Health = dead ? 0 : 20;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, true)]
    public void SpectralPassageRespectsShootabilityAndAttackEligibility(bool shootable, bool spectralMissile,
        bool canHarmSpecies, bool blocked)
    {
        var (sim, missile, target) = Setup();
        target.Spectral = true; target.Shootable = shootable; target.DoHarmSpecies = canHarmSpecies;
        missile.Spectral = spectralMissile;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Fact]
    public void SpectralNonsolidShootableTargetStillAllowsPassage()
    {
        var (_, missile, target) = Setup(); target.Solid = false;
        target.Spectral = true; target.DoHarmSpecies = true;
        Assert.Null(ActorJumpActions.CheckBlock(missile, 2));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, true)]
    public void MissileProbeUsesSupportedContactFlags(int mode, bool blocked)
    {
        var (sim, missile, target) = Setup();
        target.NonShootable = mode == 1; target.Ghost = mode == 2; missile.ThruGhost = mode == 2;
        missile.MThruSpecies = mode == 3; target.Solid = mode != 4;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(16, false, 16, true)]
    [InlineData(16, false, 17, false)]
    [InlineData(-16, false, 17, true)]
    [InlineData(-16, true, 17, false)]
    public void PassHeightUsesNativeInclusiveTopBoundary(int height, bool clip, int z, bool blocked)
    {
        var (_, missile, target) = Setup(clip); target.ProjectilePassHeight = Fixed.FromInt(height);
        missile.Z = Fixed.FromInt(z);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OwnerBlocksOnlyWithHitOwner(bool hitOwner)
    {
        var (_, missile, _) = Setup(); missile.X = missile.Owner.X; missile.HitOwner = hitOwner;
        Assert.Equal(hitOwner ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(bool clip = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(0, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.X = missile.Y = default;
        missile.Z = Fixed.FromInt(20);
        return (sim, missile, target);
    }
}
