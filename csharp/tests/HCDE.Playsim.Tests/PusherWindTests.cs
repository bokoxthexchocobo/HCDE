using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PusherWindTests
{
    [Fact]
    public void SectorWindThinkerAddsVelocityOnGround()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = LineSpecialFeasibleGaps.SectorSetWind, Arg0 = 7, Arg1 = 64, Arg2 = 0, PlayerUse = true }, true));
        var player = sim.Players.Single();
        player.OnGround = true;
        var vx = player.VelocityX.ToDouble();
        sim.Tick();
        Assert.True(player.VelocityX.ToDouble() > vx);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 0, Y = 0 }],
    });
}
