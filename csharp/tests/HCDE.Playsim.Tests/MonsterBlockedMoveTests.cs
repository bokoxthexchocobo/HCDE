using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterBlockedMoveTests
{
    [Fact]
    public void CanUseWallsOpensDoorWhenBlocked()
    {
        var sim = DoorHall();
        var monster = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var door = sim.Level.Lines.Single(l => l.Special == LineSpecials.DoorRaise);
        monster.CanUseWalls = true;
        MonsterBlockedMoveActions.TryClearBlockedMove(sim, monster, door);
        Assert.NotEmpty(sim.Motions);
    }

    private static AuthoritySimulation DoorHall()
    {
        var sides = new[] { new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 } };
        var lines = new[]
        {
            new LevelLine { X1 = 64, Y1 = -128, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Special = LineSpecials.DoorRaise },
        };
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.Unknown,
            Sectors =
            [
                new LevelSector { CeilingHeight = 64, FloorHeight = 0 },
                new LevelSector { CeilingHeight = 128, FloorHeight = 0 },
            ],
            Sides = sides,
            Lines = lines,
            Things = [new LevelThing { Type = 3001, X = 32, Y = 0 }],
        });
    }
}
