using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class FlightCommandTests
{
    [Fact]
    public void NetworkLandInputRunsWithItsQueuedSimulationTic()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
        });
        var player = sim.Players.Single(); player.NoGravity = true; player.Z = Fixed.FromInt(64); sim.Tick();
        var sink = new SimulationCommandSink(sim);
        Assert.True(sink.ApplyCommand(0, 0, 1, new UserCmd(0, 0, 0, 0, 0, 0, short.MinValue), ReadOnlyMemory<byte>.Empty));
        Assert.True(player.NoGravity);
        sim.Tick(); Assert.False(player.NoGravity);
        Assert.Equal(-1, player.VelocityZ.ToDouble());
    }
}
