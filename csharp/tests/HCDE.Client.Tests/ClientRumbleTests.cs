using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class ClientRumbleTests
{
    [Theory]
    [InlineData(0, false, 1)]
    [InlineData(1, false, 0)]
    [InlineData(1, true, 1)]
    public void UseRumbleFiltersConsoleAndCameraBody(int playerNum, bool viewRemote, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 100 }],
        });
        using var client = new ClientHost(sim); var requests = new List<SimulationRumbleRequest>();
        client.RumbleRequested += requests.Add;
        var player = sim.Players.Single(actor => actor.PlayerNum == playerNum);
        if (viewRemote) Assert.True(client.TrySetAudioListener(player.Id, out var error), error);
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 112, Arg0 = 7, Arg1 = 200, PlayerUse = true }, true, false));
        Assert.Equal(expected, requests.Count);
    }

    [Fact]
    public void ClientReceivesPainAndDeathPatterns()
    {
        var sim = Room(); using var client = new ClientHost(sim); var patterns = new List<string>();
        client.RumbleRequested += request => patterns.Add(request.Pattern);
        ActorDamage.Apply(sim.Players.Single(), 10); ActorDamage.Apply(sim.Players.Single(), 1000);
        Assert.Equal(new[] { "*pain", "*death" }, patterns);
    }

    [Fact]
    public void ClientForwardsLandingPatternWithoutAudioArchive()
    {
        var sim = Room(); using var client = new ClientHost(sim);
        var requests = new List<SimulationRumbleRequest>(); client.RumbleRequested += requests.Add;
        Land(sim);
        Assert.Equal(new SimulationRumbleRequest(sim.Players.Single().Id, "*land"), Assert.Single(requests));
    }

    [Fact]
    public void DisposedClientStopsReceivingSimulationRumble()
    {
        var sim = Room(); var client = new ClientHost(sim); var calls = 0;
        client.RumbleRequested += _ => calls++; Land(sim); Assert.Equal(1, calls);
        client.Dispose(); Land(sim); Assert.Equal(1, calls);
    }

    [Fact]
    public void DisabledWorldHapticsDoNotReachClient()
    {
        var sim = Room(); sim.WorldHapticsEnabled = false; using var client = new ClientHost(sim);
        var calls = 0; client.RumbleRequested += _ => calls++; Land(sim); Assert.Equal(0, calls);
    }

    private static void Land(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.Z = Fixed.FromInt(1);
        player.VelocityZ = Fixed.FromInt(-10); ActorPhysics.Step(sim, player);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
