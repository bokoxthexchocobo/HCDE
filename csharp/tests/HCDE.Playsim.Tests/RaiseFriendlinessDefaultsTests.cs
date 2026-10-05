using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseFriendlinessDefaultsTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void ClassFriendlinessIsRestoredBeforeArchvileAllegiance(bool classFriendly, bool archvile, bool raiserFriendly)
    {
        var sim = Room(classFriendly); var corpse = sim.Actors[1];
        Assert.Equal(classFriendly, corpse.Friendly);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Friendly = !classFriendly;
        Assert.True(archvile
            ? ArchvileActions.TryRaise(sim, new Actor { Friendly = raiserFriendly }, sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(archvile ? raiserFriendly : classFriendly, corpse.Friendly);
    }

    [Fact]
    public void FriendlyRevivalDefaultAffectsChecksum()
    {
        var first = Room(false); var second = Room(false);
        second.Actors[1].ResurrectionDefenseFlags = 8;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].Friendly, second.Actors[1].Friendly);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool friendly) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
        + (friendly ? " + FRIEND" : "") + "\n"));
}
