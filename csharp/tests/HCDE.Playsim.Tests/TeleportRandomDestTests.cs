using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportRandomDestTests
{
    [Fact]
    public void TidTeleportUsesDeterministicIndexAcrossMatchingDests()
    {
        var a = Room(1);
        var b = Room(1);
        Assert.True(LineSpecials.ActivateMapLine(a, a.Players.Single(), new LevelLine { Special = LineSpecials.Teleport, Arg0 = 5, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(b, b.Players.Single(), new LevelLine { Special = LineSpecials.Teleport, Arg0 = 5, PlayerUse = true }, true));
        Assert.Equal(a.Players.Single().X.Raw, b.Players.Single().X.Raw);
    }

    private static AuthoritySimulation Room(int seed)
    {
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7 }, new LevelSector { Tag = 7 }],
            Things =
            [
                new LevelThing { Type = 1 },
                new LevelThing { Type = 14, Id = 5, X = -64, Y = 0 },
                new LevelThing { Type = 14, Id = 5, X = 64, Y = 0 },
            ],
        }, rngSeed: seed);
    }
}
