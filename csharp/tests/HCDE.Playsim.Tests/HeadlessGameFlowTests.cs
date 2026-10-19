using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class HeadlessGameFlowTests
{
    [Fact]
    public void TicCommand_QueuesOnAuthoritySimulation()
    {
        using var session = Session(SpawnGameMode.Cooperative);
        session.InitNew("MAP01");
        var sim = session.Simulation!;
        var player = sim.Players.Single();
        HeadlessGameFlow.ApplyTicCommand(sim, player.PlayerNum,
            HeadlessGameFlow.BuildMovementCommand(0x6400, 0, yaw: 128));
        sim.Tick();
        Assert.True(player.HasMovementInput);
    }

    [Fact]
    public void ReloadRequest_RestartsMapThroughSession()
    {
        using var session = Session(SpawnGameMode.Single);
        session.InitNew("MAP01", 9);
        var sim = session.Simulation!;
        var player = sim.Players.Single();
        player.Health = 0;
        sim.Tick();
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.True(sim.QueueCommand(player.PlayerNum, new PlayerCommand { Use = true }));
        sim.Tick();
        Assert.True(sim.ReloadRequested);
        var ticker = new HeadlessGameTicker(session);
        ticker.Tick();
        Assert.NotNull(session.Simulation);
        Assert.True(session.Simulation.Players.Single().Health > 0);
        Assert.False(session.Simulation.ReloadRequested);
    }

    [Fact]
    public void DeathmatchSpawn_UsesDmStartAfterRespawn()
    {
        using var session = Session(SpawnGameMode.Deathmatch);
        session.InitNew("MAP01");
        var sim = session.Simulation!;
        var player = sim.Players.Single();
        player.Health = 0;
        sim.ForceRespawn = true;
        for (var i = 0; i < GameTicClock.TicRate + 5; i++) sim.Tick();
        Assert.True(player.Health > 0);
    }

    private static HeadlessGameSession Session(SpawnGameMode mode) => new(new HeadlessGameSessionConfig
    {
        IwadBytes = TestWadBuilder.BuildMinimalMapWad("MAP01"),
        RngSeed = 2,
        IntermissionTics = 0,
        SpawnOptions = new SpawnOptions(3, mode),
    });
}
