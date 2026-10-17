using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class ClientTeleportRumbleTests
{
    [Theory]
    [InlineData(0, false, true)]
    [InlineData(1, false, false)]
    [InlineData(1, true, true)]
    public void DeliveryRequiresConsoleBodyOrViewedPlayerBody(int teleported, bool viewRemote, bool delivered)
    {
        var sim = Room(); using var host = new ClientHost(sim); var requests = new List<SimulationRumbleRequest>();
        host.RumbleRequested += requests.Add;
        var remote = sim.Players.Single(player => player.PlayerNum == 1);
        if (viewRemote) Assert.True(host.TrySetAudioListener(remote.Id, out var error), error);
        var actor = sim.Players.Single(player => player.PlayerNum == teleported);
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        Assert.Equal(delivered ? 1 : 0, requests.Count);
    }

    [Fact]
    public void ConsoleBodyStillEligibleWhileViewingAnotherPlayer()
    {
        var sim = Room(); using var host = new ClientHost(sim); var requests = new List<SimulationRumbleRequest>();
        host.RumbleRequested += requests.Add;
        Assert.True(host.TrySetAudioListener(sim.Players.Single(player => player.PlayerNum == 1).Id, out var error), error);
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(player => player.PlayerNum == 0), LineSpecials.Teleport, 0));
        Assert.Single(requests);
    }

    [Fact]
    public void ChangedConsolePlayerControlsDelivery()
    {
        var sim = Room(); using var host = new ClientHost(sim); var requests = new List<SimulationRumbleRequest>();
        host.RumbleRequested += requests.Add;
        Assert.True(host.TrySetAudioConsolePlayer(1, out var error), error);
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(player => player.PlayerNum == 0), LineSpecials.Teleport, 0));
        Assert.Empty(requests);
        sim.Players.Single(player => player.PlayerNum == 0).X = Fixed.FromInt(400);
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(player => player.PlayerNum == 1), LineSpecials.Teleport, 0));
        Assert.Equal("misc/teleport", Assert.Single(requests).Pattern);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 100 }, new LevelThing { Type = 14, X = 200 }],
    });
}
