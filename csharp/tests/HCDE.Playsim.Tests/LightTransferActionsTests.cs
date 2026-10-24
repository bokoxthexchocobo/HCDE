using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightTransferActionsTests
{
    [Fact]
    public void TransferFollowsSourceSectorLight()
    {
        var sim = Room();
        LightTransferActions.Add(sim, 0, 8, copyFloor: true);
        sim.Lights[0] = 40;
        sim.Tick();
        Assert.Equal(40, sim.LightOf(1));
        sim.Lights[0] = 200;
        sim.Tick();
        Assert.Equal(200, sim.LightOf(1));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 0, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, LightLevel = 96 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
