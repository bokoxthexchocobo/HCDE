using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorChangeFlagsLoadedMapTests
{
    [Fact]
    public void SectorChangeFlagsSetsNoAttackRuntimeBit()
    {
        var sim = Room();
        Assert.True(SectorChangeFlagsSpecial.Execute(sim, SectorChangeFlagsSpecial.Special, 7, 2048, 0));
        var sector = sim.Level.Sectors.Single(s => s.MatchesTag(7));
        Assert.Equal(2048, sector.RuntimeFlags);
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.SectorIndex = sim.Level.Sectors.ToList().FindIndex(s => s.MatchesTag(7));
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        Assert.False(MonsterBrain.CheckNativeMeleeRange(sim, imp, player));
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors =
            [
                new LevelSector { Tag = 7, CeilingHeight = 128, FloorHeight = 0 },
                new LevelSector { Tag = 8, CeilingHeight = 128, FloorHeight = 0 },
            ],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40, Y = 0 }, new LevelThing { Type = 3001, X = -40, Y = 32 }],
        });
    }
}
