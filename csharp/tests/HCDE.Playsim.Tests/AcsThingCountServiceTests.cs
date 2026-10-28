using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsThingCountServiceTests
{
    private static AuthoritySimulation SimWithImp()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 64, Y = 64 }],
        };
        return AuthoritySimulation.Start(level);
    }

    [Fact]
    public void CountsLivingMapActorsByEditorNumber()
    {
        var sim = SimWithImp();
        Assert.Equal(1, AcsThingCountService.Count(sim, 3001, tid: 0, sectorTag: -1));
    }

    [Fact]
    public void ReplacementPassAddsSecondEditorNumber()
    {
        var sim = SimWithImp();
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 3001, X = 64, Y = 64 },
                new LevelThing { Type = 3004, X = 96, Y = 64 },
            ],
        };
        var dual = AuthoritySimulation.Start(level);
        AcsThingCountReplacementPolicy.RegisterReplacement(3001, 3004);
        var count = AcsThingCountService.Count(dual, 3001, 0, -1, AcsThingCountReplacementPolicy.ResolveEditorNumber);
        Assert.Equal(2, count);
        AcsThingCountReplacementPolicy.RegisterReplacement(3001, 3001);
    }

    [Fact]
    public void TidFilterLimitsMatches()
    {
        var sim = SimWithImp();
        sim.Actors.First(a => a.DoomEdNum == 3001).ThingId = 42;
        Assert.Equal(1, AcsThingCountService.Count(sim, 3001, 42, -1));
        Assert.Equal(0, AcsThingCountService.Count(sim, 3001, 99, -1));
    }

    [Fact]
    public void CountByClassNameUsesCatalog()
    {
        var sim = SimWithImp();
        Assert.Equal(1, AcsThingCountService.CountByClassName(sim, "DoomImp", 0, -1));
        Assert.Equal(0, AcsThingCountService.CountByClassName(sim, "NotAClass", 0, -1));
    }
}

public class AcsPlayerPopulationTests
{
    [Fact]
    public void CountsPlayersSkippingReservedSlots()
    {
        var level = new PlayLevel { MapName = "MAP01", Things = [new LevelThing { Type = 1, X = 0, Y = 0 }] };
        var sim = AuthoritySimulation.Start(level);
        Assert.Equal(1, AcsPlayerPopulation.CountInGame(sim));
        Assert.Equal(0, AcsPlayerPopulation.CountInGame(sim, slot => slot == 0));
    }
}
