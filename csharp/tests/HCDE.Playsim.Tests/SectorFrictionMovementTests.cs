using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorFrictionMovementTests
{
    [Fact]
    public void SectorFrictionSpecialSlowsGroundMovement()
    {
        var sim = Room(frictionAmount: 50);
        var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        var icyX = player.X.ToDouble();
        sim = Room(frictionAmount: 100);
        player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.True(player.X.ToDouble() > icyX);
    }

    private static AuthoritySimulation Room(int frictionAmount)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256 }],
            Lines = [new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var line = new LevelLine
        {
            Special = 219,
            Tag = 7,
            Arg0 = 7,
            Arg1 = frictionAmount,
            PlayerUse = true,
            Repeat = true,
            SideFront = 0,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        return sim;
    }
}
