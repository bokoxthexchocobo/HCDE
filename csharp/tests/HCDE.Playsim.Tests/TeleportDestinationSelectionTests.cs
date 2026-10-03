using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportDestinationSelectionTests
{
    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void TidAndSectorTagBothRestrictDestination(MapDataFormat format)
    {
        var sim = Room(format);
        var destinations = sim.Actors.Where(a => a.DoomEdNum == LineSpecials.TeleportDestType).ToArray();
        destinations[0].ThingId = 7; destinations[0].SectorIndex = 0;
        destinations[1].ThingId = 7; destinations[1].SectorIndex = 1;
        var line = new LevelLine { Special = 70, Arg0 = 7, Arg1 = 20, PlayerCross = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, Assert.Single(sim.Players), line, false, false));
        Assert.Equal(200, Assert.Single(sim.Players).X.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroTidUsesLowestTaggedSectorRatherThanActorOrder(bool extended)
    {
        var sim = Room(extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary);
        var destinations = sim.Actors.Where(a => a.DoomEdNum == LineSpecials.TeleportDestType).ToArray();
        destinations[0].SectorIndex = 1; destinations[1].SectorIndex = 0;
        var line = new LevelLine { Special = extended ? 70 : 39, Tag = 10, Arg1 = 10, PlayerCross = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, Assert.Single(sim.Players), line, false, false));
        Assert.Equal(200, Assert.Single(sim.Players).X.ToDouble());
    }

    [Fact]
    public void UnmatchedSectorTagFailsWithoutConsumingLine()
    {
        var sim = Room(MapDataFormat.HexenBinary);
        var line = new LevelLine { Special = 70, Arg1 = 99, PlayerCross = true };
        var player = Assert.Single(sim.Players);
        var x = player.X;
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, false, false));
        Assert.Equal(x, player.X); Assert.Equal(70, line.Special);
    }

    private static AuthoritySimulation Room(MapDataFormat format) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = format, Namespace = "ZDoom",
        Sectors = [new LevelSector { Tag = 10, CeilingHeight = 128 },
            new LevelSector { Tag = 20, AdditionalTags = [10], CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 },
            new LevelThing { Type = LineSpecials.TeleportDestType, X = 100 },
            new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}