using HCDE.Gamedata;
using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class HeadlessLevelFlowTests
{
    [Fact]
    public void InitNew_LoadsMapAndTicks()
    {
        using var session = Session();
        session.InitNew("MAP01", 7);
        Assert.NotNull(session.Simulation);
        Assert.Equal("MAP01", session.CurrentMapName);
        session.Simulation!.Tick();
        Assert.Single(session.Simulation.Players);
    }

    [Fact]
    public void ExitWithMapInfo_TransitionsToNextMapAfterIntermission()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapInfoParser.TryParse(
            "map MAP01 { next = MAP01 }",
            out var info, out var error), error);
        using var session = new HeadlessGameSession(new HeadlessGameSessionConfig
        {
            IwadBytes = wad,
            MapInfo = info,
            RngSeed = 1,
            IntermissionTics = 2,
        });
        session.InitNew("MAP01");
        session.Simulation!.MarkExited(false);
        Assert.Equal("MAP01", session.PendingNextMap);
        Assert.Equal(HeadlessGameState.Intermission, session.State);
        session.Tick();
        session.Tick();
        Assert.Equal("MAP01", session.CurrentMapName);
        Assert.False(session.Simulation!.Exited);
    }

    [Fact]
    public void CoopTravel_PreservesPlayerHealthAcrossLoad()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapInfoParser.TryParse("map MAP01 { next = MAP01 }",
            out var info, out var error), error);
        using var session = new HeadlessGameSession(new HeadlessGameSessionConfig
        {
            IwadBytes = wad,
            MapInfo = info,
            RngSeed = 3,
            IntermissionTics = 0,
            SpawnOptions = new SpawnOptions(3, SpawnGameMode.Cooperative),
        });
        session.InitNew("MAP01");
        var player = session.Simulation!.Players.Single();
        player.Health = 77;
        session.Simulation.MarkExited(false);
        session.Tick();
        Assert.Equal("MAP01", session.CurrentMapName);
        Assert.Equal(77, session.Simulation.Players.Single().Health);
    }

    [Fact]
    public void ShouldDoIntermission_SkipsHubHubWhenClusterDisallows()
    {
        Assert.True(MapInfoParser.TryParse(
            "cluster 1 { hub } map MAP01 { cluster = 1 next = MAP02 } map MAP02 { cluster = 1 }",
            out var info, out var parseError), parseError);
        Assert.False(HeadlessMapExit.ShouldDoIntermission(info, "MAP01", "MAP02", deathmatch: false));
    }

    private static HeadlessGameSession Session() => new(new HeadlessGameSessionConfig
    {
        IwadBytes = TestWadBuilder.BuildMinimalMapWad("MAP01"),
        RngSeed = 1,
        IntermissionTics = 0,
    });
}
