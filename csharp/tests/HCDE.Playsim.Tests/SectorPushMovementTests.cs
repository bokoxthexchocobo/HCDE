using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorPushMovementTests
{
    [Fact]
    public void SectorCurrentAppliesHorizontalPushWhileGrounded()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, PushForceX = 2 }],
            Lines = [new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single();
        player.OnGround = true;
        ActorPhysics.Step(sim, player);
        Assert.True(player.VelocityX.ToDouble() > 0);
    }
}
