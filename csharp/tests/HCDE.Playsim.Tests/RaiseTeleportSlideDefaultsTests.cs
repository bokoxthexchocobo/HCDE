using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseTeleportSlideDefaultsTests
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
    public void RaiseRestoresPatchedTeleportAndSlidingFlags(bool noTeleport, bool canSlide, bool archvile)
    {
        var sim = Room(noTeleport, canSlide);
        var corpse = sim.Actors[1];
        Assert.Equal(noTeleport, corpse.NoTeleport);
        Assert.Equal(canSlide, corpse.CanSlide);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoTeleport = !noTeleport;
        corpse.CanSlide = !canSlide;

        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(noTeleport, corpse.NoTeleport);
        Assert.Equal(canSlide, corpse.CanSlide);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void DifferentDefaultsAffectChecksumWithMatchingCurrentFlags(int flags)
    {
        var first = Room(false, false);
        var second = Room(false, false);
        second.Actors[1].ResurrectionMovementFlags = flags;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].NoTeleport, second.Actors[1].NoTeleport);
        Assert.Equal(first.Actors[1].CanSlide, second.Actors[1].CanSlide);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool noTeleport, bool canSlide) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
        + (noTeleport ? " + NOTELEPORT" : "") + (canSlide ? " + CANSLIDE" : "") + "\n"));
}
