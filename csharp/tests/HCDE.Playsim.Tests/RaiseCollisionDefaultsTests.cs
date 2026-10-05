using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseCollisionDefaultsTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(6, false)]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(6, true)]
    public void RevivalRestoresPatchedClassCollisionFlags(int bits, bool archvileRaise)
    {
        var sim = Room(bits);
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Solid = (bits & 2) == 0;
        corpse.Shootable = (bits & 4) == 0;

        Assert.True(archvileRaise
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal((bits & 2) != 0, corpse.Solid);
        Assert.Equal((bits & 4) != 0, corpse.Shootable);
    }

    [Fact]
    public void DifferentRevivalDefaultsAffectChecksumWhenCurrentFlagsMatch()
    {
        var first = Room(6);
        var second = Room(6);
        Assert.Equal(first.Checksum, second.Checksum);
        second.Actors[1].ResurrectionCollisionFlags = 0;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].Solid, second.Actors[1].Solid);
        Assert.Equal(first.Actors[1].Shootable, second.Actors[1].Shootable);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(int bits) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
}
