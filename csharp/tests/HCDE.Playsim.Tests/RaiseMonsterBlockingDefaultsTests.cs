using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseMonsterBlockingDefaultsTests
{
    [Theory]
    [InlineData(false, false, 2, false)]
    [InlineData(true, false, 2, true)]
    [InlineData(false, true, 2, false)]
    [InlineData(true, true, 2, true)]
    [InlineData(true, false, 3, false)]
    [InlineData(true, true, 3, false)]
    public void RevivalRestoresClassMonsterBlockingExemption(bool exemption, bool archvile, int lineFlags, bool crosses)
    {
        var sim = Room(exemption, lineFlags);
        var corpse = Assert.Single(sim.Actors);
        Assert.Equal(exemption, corpse.NoBlockMonsters);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoBlockMonsters = !exemption;
        Assert.True(archvile
            ? ArchvileActions.TryRaise(sim, new Actor { X = Fixed.FromInt(-60) }, new Actor { X = Fixed.FromInt(500) })
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(exemption, corpse.NoBlockMonsters);
        Assert.Equal(crosses, ActorPhysics.TryMove(sim, corpse, 40, 0, out _));
    }

    [Fact]
    public void ExemptionRevivalDefaultAffectsChecksum()
    {
        var first = Room(false, 0); var second = Room(false, 0);
        second.Actors[0].ResurrectionCollisionFlags = 11;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[0].NoBlockMonsters, second.Actors[0].NoBlockMonsters);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool exemption, int lineFlags)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var patch = DehackedPatch.Apply("Thing 2\nBits = " + (exemption ? "FRIEND+6" : "6") + "\n");
        if (exemption) patch = DehackedPatch.Apply("Thing 2\nBits = 6\n", patch);
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1, Flags = lineFlags,
            }).ToArray(),
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: patch);
    }
}
