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
        var (_, missile, _) = Setup(); missile.X = Assert.IsAssignableFrom<Actor>(missile.Owner).X; missile.HitOwner = hitOwner;
        Assert.Equal(hitOwner ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
    }

    [Theory]
    [InlineData(16, false, 17, false)]
    [InlineData(16, false, 16, true)]
    [InlineData(-16, false, 17, true)]
    [InlineData(-16, true, 17, false)]
    [InlineData(0, false, 57, false)]
    [InlineData(0, false, 56, true)]
    public void NoPassMobjRetainsMissilePassHeightRules(int height, bool clip, int z, bool blocked)
    {
        var (sim, missile, target) = Setup(clip, noPassMobj: true);
        target.ProjectilePassHeight = Fixed.FromInt(height); missile.Z = Fixed.FromInt(z);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(true, false, false, false, false, true)]
    [InlineData(false, true, false, false, false, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, true, false, false, false)]
    [InlineData(true, false, false, true, false, false)]
    [InlineData(true, false, false, true, true, true)]
    public void DontOverlapPrecedesMissileHeightClearance(bool canPass, bool bridge, bool legacy,
        bool rip, bool dontRip, bool blocked)
    {
        var (sim, missile, target) = Setup(noPassMobj: legacy);
        missile.CanPass = canPass; missile.DontOverlap = target.DontOverlap = true;
        missile.Rip = rip; target.DontRip = dontRip; target.ActsLikeBridge = bridge;
        missile.Z = Fixed.FromInt(64);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, before);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
    }

    [Theory]
    [InlineData(true, false, false, false, false, true)]
    [InlineData(false, true, false, false, false, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, true, false, false, false)]
    [InlineData(true, false, false, true, false, false)]
    [InlineData(true, false, false, true, true, true)]
    public void DontOverlapStopsTravelWithoutDirectDamage(bool canPass, bool bridge, bool legacy,
        bool rip, bool dontRip, bool blocked)
    {
        var (sim, missile, target) = Setup(noPassMobj: legacy);
        missile.CanPass = canPass; missile.DontOverlap = target.DontOverlap = true;
        missile.Rip = rip; target.DontRip = dontRip; target.ActsLikeBridge = bridge;
        missile.Z = Fixed.FromInt(64); missile.VelocityZ = default;
        var health = target.Health;
        sim.Tick();
        Assert.Equal(blocked, missile.Destroyed);
        Assert.Equal(health, target.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DontOverlapTravelBlocksBeforeMissilePassageChecks(int mode)
    {
        var (sim, missile, target) = Setup();
        if (mode == 3)
        {
            target.X = Fixed.FromInt(200);
            target = Assert.IsAssignableFrom<Actor>(missile.Owner);
            target.X = default;
        }
        missile.CanPass = true; missile.DontOverlap = target.DontOverlap = true;
        target.NonShootable = mode == 0;
        target.Ghost = missile.ThruGhost = mode == 1;
        target.Spectral = mode == 2;
        var health = target.Health;
        sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(health, target.Health);
    }

    [Theory]
    [InlineData(true, false, false, false, false, 56, false)]
    [InlineData(true, false, false, false, false, 55, true)]
    [InlineData(false, true, false, false, false, 56, false)]
    [InlineData(false, false, false, false, false, 56, true)]
    [InlineData(true, false, true, false, false, 56, true)]
    [InlineData(true, false, false, true, false, 56, true)]
    [InlineData(true, false, false, true, true, 56, false)]
    public void GenericHeightPassagePrecedesExtendedMissilePassHeight(bool canPass, bool bridge, bool legacy,
        bool rip, bool dontRip, int z, bool contact)
    {
        var (sim, missile, target) = Setup(noPassMobj: legacy);
        missile.CanPass = canPass; target.ActsLikeBridge = bridge;
        missile.Rip = rip; target.DontRip = dontRip;
        target.ProjectilePassHeight = Fixed.FromInt(96);
        target.DoHarmSpecies = true;
        missile.Z = Fixed.FromInt(z); missile.VelocityZ = default;
        var health = target.Health;
        var before = SimSavegame.Write(sim);
        Assert.Equal(contact ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
        sim.Tick();
        Assert.Equal(contact, target.Health < health);
        Assert.Equal(contact && !rip, missile.Destroyed);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(bool clip = false, bool noPassMobj = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            compat: (clip ? CompatSurface.MissileClip : CompatSurface.None)
                | (noPassMobj ? CompatSurface.NoPassMobj : CompatSurface.None));
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(0, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.X = missile.Y = default;
        missile.Z = Fixed.FromInt(20);
        return (sim, missile, target);
    }
}
