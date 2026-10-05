using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDroppedDefaultsTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void RevivalRestoresPatchedDroppedDefault(bool dropped, bool archvile, bool blocked)
    {
        var sim = Room(dropped); var corpse = sim.Actors[1];
        Assert.Equal(dropped, corpse.Dropped);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Dropped = !dropped;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked ? !dropped : dropped, corpse.Dropped);
    }

    [Fact]
    public void DroppedRevivalDefaultAffectsChecksum()
    {
        var first = Room(false); var second = Room(false);
        second.Actors[1].ResurrectionCollisionFlags = 19;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].Dropped, second.Actors[1].Dropped);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool dropped) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
        + (dropped ? " + DROPPED" : "") + "\n"));
}
