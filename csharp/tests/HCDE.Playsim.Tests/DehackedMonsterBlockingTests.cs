using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMonsterBlockingTests
{
    [Theory]
    [InlineData("6", 2, false)]
    [InlineData("FRIEND+6", 2, true)]
    [InlineData("STEALTH+6", 2, false)]
    [InlineData("FRIEND+6", 3, false)]
    public void FriendRemapPassesOnlyMonsterBlockingLines(string bits, int flags, bool expected)
    {
        var sim = Room(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"), flags);
        Assert.Equal(expected, ActorPhysics.TryMove(sim, Assert.Single(sim.Actors), 40, 0, out _));
    }

    [Fact]
    public void RemovingFriendBitPreservesNativeExtendedFlagSideEffect()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = FRIEND+6\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = 6\n", first);
        var actor = Assert.Single(Room(second, 2).Actors);
        Assert.False(actor.Friendly);
        Assert.True(actor.NoBlockMonsters);
        Assert.True(second.Actors.Single(record => record.Index == 2).NoBlockMonsters);
    }

    [Fact]
    public void CrossingExemptionParticipatesInSimulationChecksum()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = 6\n");
        var first = Room(patch, 0); var second = Room(patch, 0);
        Assert.Single(first.Actors).NoBlockMonsters = true;
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch, int flags)
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
        }, dehacked: patch);
    }
}
