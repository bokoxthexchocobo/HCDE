using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingProjectileLineSpecialsTests
{
    [Fact]
    public void ProjectileAimedSpawnsMissileTowardTargetTid()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.ThingId = 7;
        var player = sim.Players.Single();
        player.ThingId = 8;
        var type = new ProjectileActor(imp, ProjectileKind.ImpBall).DoomEdNum;
        var line = Trigger(178, spotTid: 7, type, speed: 32, destTid: 8);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.Contains(sim.Actors, a => a is ProjectileActor { Kind: ProjectileKind.ImpBall });
    }

    [Fact]
    public void ProjectileInterceptSpawnsSeekerMissile()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.ThingId = 7;
        var player = sim.Players.Single();
        player.ThingId = 8;
        player.VelocityX = Fixed.FromDouble(4);
        var type = new ProjectileActor(imp, ProjectileKind.ImpBall).DoomEdNum;
        var line = Trigger(175, spotTid: 7, type, speed: 32, destTid: 8);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        var missile = Assert.IsType<ProjectileActor>(sim.Actors.First(a => a is ProjectileActor));
        Assert.Equal(ProjectileKind.ImpBall, missile.Kind);
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128, FloorHeight = 0 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0 },
            ],
        });
    }

    private static LevelLine Trigger(int special, int spotTid, int type, int speed, int destTid) => new()
    {
        Special = special,
        PlayerUse = true,
        Arg0 = spotTid,
        Arg1 = type,
        Arg2 = speed,
        Arg3 = destTid,
        Arg4 = 0,
    };
}
