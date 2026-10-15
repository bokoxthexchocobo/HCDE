using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileRadiusSightTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, true)]
    [InlineData(ProjectileKind.CyberRocket, true)]
    [InlineData(ProjectileKind.Rocket, false)]
    [InlineData(ProjectileKind.CyberRocket, false)]
    public void RadiusSightUsesVictimEyeAndMissileHeight(ProjectileKind kind, bool visible)
    {
        var (sim, target, missile) = Room(kind);
        sim.Floors[1] = visible ? 20 : 0; sim.Ceilings[1] = visible ? 25 : 10;
        Assert.False(CombatTrace.HasLineOfSight(sim, missile, target));
        missile.ExplodeForAction();
        Assert.Equal(visible ? 980 : 1000, target.Health);
        Assert.True(missile.Destroyed);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.CyberRocket)]
    public void RadiusSightIgnoresMissileVisibilityWithoutRandomDraws(ProjectileKind kind)
    {
        var (sim, target, missile) = Room(kind);
        missile.MVisBlocked = true; missile.Invisible = true; missile.MinVisible = true;
        missile.RenderStyle = ActorRenderStyle.Translucent; missile.Alpha = 0;
        missile.ExplodeForAction();
        Assert.Equal(980, target.Health);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedPartialApertureExplosionReplaysExactly()
    {
        var (sim, _, missile) = Room(ProjectileKind.Rocket);
        sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        var id = missile.Id; var saved = SimSavegame.Write(sim, true);
        missile.ExplodeForAction(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == id)).ExplodeForAction();
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor, ProjectileActor) Room(ProjectileKind kind)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines });
        var owner = sim.AddBot(-1000, 0); owner.Brain = null;
        var target = sim.AddBot(-64, 0, 3001); target.Brain = null;
        target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = Fixed.FromInt(64); missile.Y = missile.Z = default;
        sim.DrainSoundRequests();
        return (sim, target, missile);
    }
}
