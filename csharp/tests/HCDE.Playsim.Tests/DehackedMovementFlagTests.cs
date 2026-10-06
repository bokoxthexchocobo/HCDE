using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMovementFlagTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1024, true, false)]
    [InlineData(16384, false, true)]
    [InlineData(17408, true, true)]
    public void ExplicitMovementFlagsAreIndependentOfGravity(int bits, bool dropOff, bool floating)
    {
        var actor = Assert.Single(Room(bits).Actors);
        Assert.Equal(dropOff, actor.AllowDropOff);
        Assert.Equal(floating, actor.Floating);
        Assert.False(actor.NoGravity);
    }

    [Theory]
    [InlineData(6, false)]
    [InlineData(1030, true)]
    [InlineData(16390, true)]
    public void MovementFlagsAllowCrossingOtherwiseRejectedLedge(int bits, bool expected)
    {
        var sim = Room(bits);
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(expected, ActorPhysics.TryMove(sim, actor, 1, 0, out _));
    }

    private static AuthoritySimulation Room(int bits)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-100, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
    }
}
