using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileNoClipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissileNoClipPassesWalls(bool noclip)
    {
        var (sim, missile, target) = Room(true); target.X = Fixed.FromInt(1000);
        missile.NoClip = noclip; missile.Tick();
        Assert.Equal(!noclip, missile.Destroyed);
        if (noclip) Assert.Equal(64, missile.X.ToDouble());
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissileNoClipPassesActorsWithoutImpactDamage(bool noclip)
    {
        var (sim, missile, target) = Room(false); missile.NoClip = noclip;
        missile.Tick();
        Assert.Equal(!noclip, missile.Destroyed); Assert.Equal(!noclip, target.Health < 1000);
        Assert.Equal(!noclip, sim.CaptureState().MissileDamageRandomState.HasValue);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NoClipMissileStopsAtPlanesWithoutExploding(bool noclip, bool ceiling)
    {
        var (_, missile, target) = Room(false); target.X = Fixed.FromInt(1000);
        missile.NoClip = noclip; missile.VelocityX = Fixed.FromInt(2);
        missile.Z = Fixed.FromInt(ceiling ? 116 : 8); missile.VelocityZ = Fixed.FromInt(ceiling ? 16 : -16);
        missile.Tick(); Assert.Equal(!noclip, missile.Destroyed);
        if (noclip)
        {
            Assert.Equal(ceiling ? 128 - missile.Height.ToDouble() : 0, missile.Z.ToDouble());
            Assert.Equal(0, missile.VelocityZ.Raw);
        }
    }

    [Fact]
    public void NoclipRipSkipsHitCallbacksDamageAndPush()
    {
        var (sim, missile, target) = Room(false); missile.NoClip = missile.Rip = true; target.Pushable = true;
        var calls = 0; missile.SpecialMissileHitFunction = (_, _) => { calls++; return -1; };
        var lifetime = missile.RemainingTics; missile.Tick();
        Assert.Equal(0, calls); Assert.Equal(1000, target.Health); Assert.Equal(0, target.VelocityX.Raw);
        Assert.Equal(lifetime - 1, missile.RemainingTics); Assert.False(missile.Destroyed);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void SavedNoclipMissileReplaysActorPassage()
    {
        var (sim, missile, target) = Room(false); missile.NoClip = true;
        var id = missile.Id; var saved = SimSavegame.Write(sim, true);
        missile.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == id)).Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(1000, target.Health);
    }

    private static (AuthoritySimulation, ProjectileActor, PlayerPawn) Room(bool wall)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level; var lines = geometry.Lines.ToArray();
        if (wall) lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var owner = sim.AddBot(-100, 0); owner.Brain = null; owner.NoPain = true;
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(-64); missile.Y = default; missile.Z = Fixed.FromInt(48);
        missile.VelocityX = Fixed.FromInt(128); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
