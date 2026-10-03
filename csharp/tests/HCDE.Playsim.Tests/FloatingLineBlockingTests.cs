using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloatingLineBlockingTests
{
    [Theory]
    [InlineData("FLOAT+6", 0x40000, false)]
    [InlineData("NOGRAVITY+6", 0x40000, true)]
    [InlineData("6", 0x40000, true)]
    [InlineData("FRIEND+FLOAT+6", 0x40002, false)]
    [InlineData("FRIEND+6", 0x40002, true)]
    [InlineData("FLOAT+6", 0, true)]
    public void FloaterBlockingDependsOnFloatRatherThanGravityOrFriendship(string bits, int flags, bool expected)
    {
        var sim = Room(bits, flags);
        Assert.Equal(expected, ActorPhysics.TryMove(sim, Assert.Single(sim.Actors), 40, 0, out _));
    }

    [Fact]
    public void ClearingFloatingFlagAllowsPreviouslyBlockedCrossing()
    {
        var sim = Room("FLOAT+6", 0x40000);
        var actor = Assert.Single(sim.Actors);
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.Equal(-40, actor.X.ToDouble());
        actor.Floating = false;
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    private static AuthoritySimulation Room(string bits, int flags)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1, Flags = flags,
            }).ToArray(),
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
    }
}
