using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportFlyingHeightTests
{
    [Theory]
    [InlineData(true, true, 256, 96)]
    [InlineData(true, true, 128, 72)]
    [InlineData(true, false, 256, 64)]
    [InlineData(false, true, 256, 64)]
    public void OrdinaryTeleportPreservesOnlyFlyingPlayerHeight(bool player, bool noGravity, short ceiling, int expected)
    {
        var geometry = GameplayFoundationTests.TwoRooms(64, ceiling).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 40 }],
        });
        Actor actor = player ? Assert.Single(sim.Players) : sim.AddBot(-40, 0);
        actor.NoGravity = noGravity; actor.Z = Fixed.FromInt(32); actor.OnGround = false;
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        Assert.Equal(1, actor.SectorIndex);
        Assert.Equal(expected, actor.Z.ToDouble());
        Assert.Equal(expected == 64, actor.OnGround);
        Assert.False(actor.OnMobj);
    }
}
