using HCDE.MapLoader;
using HCDE.Gamedata;

namespace HCDE.Playsim.Tests;

public class MapsModsTests
{
    private static AuthoritySimulation Room(LevelLine line, MapDataFormat format = MapDataFormat.DoomBinary, string ns = "") =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = format, Namespace = ns,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [line], Things = [new LevelThing { Type = 1 }],
        });

    [Fact]
    public void DoomSwitchExitRequiresUseAndClearsOnce()
    {
        var line = new LevelLine { X1 = 32, X2 = 32, Y1 = -64, Y2 = 64, SideBack = -1, Special = 11 };
        var sim = Room(line);
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.True(sim.Exited);
        Assert.Equal(0, line.Special);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZdoomExitHonorsActivationAndRepeat(bool repeat)
    {
        var line = new LevelLine { Special = 243, PlayerCross = true, Repeat = repeat };
        var sim = Room(line, MapDataFormat.UdmfText, "ZDoom");
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.Equal(repeat ? 243 : 0, line.Special);
    }

    [Fact]
    public void ZdoomDoorNumberDoesNotBecomeDoomExit()
    {
        var line = new LevelLine { Special = 11, PlayerUse = true, SideBack = -1 };
        var sim = Room(line, MapDataFormat.UdmfText, "ZDoom");
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.False(sim.Exited);
        Assert.Equal(11, line.Special);
    }

    [Fact]
    public void DoomManualDoorTargetsBackSectorAndKeepsRepeatSpecial()
    {
        var line = new LevelLine { Special = 1, SideFront = 0, SideBack = 1 };
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 0 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [line], Things = [new LevelThing { Type = 1 }],
        });
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        for (var tic = 0; tic < 62; tic++) LineSpecials.TickMotions(sim);
        Assert.Equal(124, sim.CeilingOf(1));
        Assert.Equal(128, sim.CeilingOf(0));
        Assert.Equal(1, line.Special);
    }

    [Fact]
    public void UseCannotReachThroughSolidWall()
    {
        var exit = new LevelLine { X1 = 48, X2 = 48, Y1 = -64, Y2 = 64, SideBack = -1, Special = 11 };
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -64, Y2 = 64, SideBack = -1 }, exit],
            Things = [new LevelThing { Type = 1 }],
        });
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.False(sim.Exited);
        Assert.Equal(11, exit.Special);
    }

    [Theory]
    [InlineData(270, 270)]
    [InlineData(-90, 270)]
    [InlineData(450, 90)]
    public void MapAnglesUseFullUnsignedCircle(double input, double expected) =>
        Assert.Equal(expected, BamAngle.FromDegrees(input).ToDegrees());

    [Fact]
    public void SkillAndModeFilterEnemiesAndInvasionMarkers()
    {
        var level = new PlayLevel { Things = [
            new LevelThing { Type = 1 },
            new LevelThing { Type = 3004, SkillMask = 1 },
            new LevelThing { Type = 3004, Coop = false },
            new LevelThing { Type = 9001, Coop = false },
            new LevelThing { Type = 9001, SkillMask = 4 },
        ] };
        var sim = AuthoritySimulation.Start(level, spawnOptions: new SpawnOptions(2, SpawnGameMode.Cooperative));
        Assert.Equal(2, sim.Actors.Count);
        Assert.DoesNotContain(sim.Actors, actor => actor.DoomEdNum == 3004);
        Assert.Single(sim.Actors, actor => actor.DoomEdNum == 9001);
    }

    [Fact]
    public void SpawnKeepsFractionalPositionAndHeightAndAppliesDehackedMapNumber()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 77\n");
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004, X = 1.25, Y = 2.5, Z = 3.75, Id = 42 }],
        }, dehacked: patch);
        var actor = Assert.Single(sim.Actors);
        Assert.Equal((1.25, 2.5, 3.75, 42, 77), (actor.X.ToDouble(), actor.Y.ToDouble(), actor.Z.ToDouble(), actor.ThingId, actor.Health));
    }
}
