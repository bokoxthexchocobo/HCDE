using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseMovementDefaultsTests
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
    public void RaiseRestoresClassMovementDefaults(bool noGravity, bool floating, bool archvile)
    {
        var sim = Room(noGravity, floating);
        var corpse = sim.Actors[1];
        Assert.Equal(noGravity, corpse.NoGravity);
        Assert.Equal(floating, corpse.Floating);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoGravity = !noGravity;
        corpse.Floating = !floating;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(noGravity, corpse.NoGravity);
        Assert.Equal(floating, corpse.Floating);
    }

    [Fact]
    public void MovementDefaultsAffectChecksumWithMatchingCurrentFlags()
    {
        var first = Room(false, false);
        var second = Room(false, false);
        second.Actors[1].ResurrectionMovementFlags = 3;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].NoGravity, second.Actors[1].NoGravity);
        Assert.Equal(first.Actors[1].Floating, second.Actors[1].Floating);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool noGravity, bool floating) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
        + (noGravity ? " + NOGRAVITY" : "") + (floating ? " + FLOAT" : "") + "\n"));
}
