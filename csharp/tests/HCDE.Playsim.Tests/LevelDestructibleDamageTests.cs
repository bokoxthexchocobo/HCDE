using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LevelDestructibleDamageTests
{
    [Fact]
    public void DamageLine_UpdatesPooledGroupHealth()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 0 }],
            Lines =
            [
                new LevelLine { Index = 0, Tag = 1, Health = 100, HealthGroup = 4, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { Index = 1, Tag = 2, Health = 100, HealthGroup = 4, SideFront = 1, SideBack = LevelLine.NoSide },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var lineA = sim.Level.Lines[0];
        var lineB = sim.Level.Lines[1];
        Assert.Equal(100, sim.HealthGroups[4]);

        LevelDestructibleDamage.DamageLine(sim, lineA, 30);
        Assert.Equal(70, lineA.Health);
        Assert.Equal(70, lineB.Health);
        Assert.Equal(70, sim.HealthGroups[4]);
    }

    [Fact]
    public void DamageSectorPart_UpdatesPooledFloorGroup()
    {
        var sector = new LevelSector
        {
            Index = 0,
            Tag = 9,
            HealthFloor = 80,
            HealthFloorGroup = 6,
            LightLevel = 128,
            CeilingHeight = 128,
        };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [sector],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        LevelDestructibleDamage.DamageSectorPart(sim, sector, AcsDestructibleHealth.SectorPartFloor, 25);
        Assert.Equal(55, sector.HealthFloor);
        Assert.Equal(55, sim.HealthGroups[6]);
    }
}
