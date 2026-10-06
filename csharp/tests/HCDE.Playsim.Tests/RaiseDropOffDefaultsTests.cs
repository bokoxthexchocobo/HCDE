using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDropOffDefaultsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RaisedMonsterCrossesLedgeOnlyWhenClassAllowsDropOff(bool dropOff, bool archvile)
    {
        var sim = Room(dropOff);
        var corpse = Assert.Single(sim.Actors);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.AllowDropOff = !dropOff;
        Assert.True(archvile
            ? ArchvileActions.TryRaise(sim, new Actor { X = Fixed.FromInt(-60) }, new Actor { X = Fixed.FromInt(500) })
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(dropOff, corpse.AllowDropOff);
        Assert.Equal(dropOff, ActorPhysics.TryMove(sim, corpse, 1, 0, out _));
    }

    [Fact]
    public void DropOffRevivalDefaultAffectsChecksum()
    {
        var first = Room(false); var second = Room(false);
        second.Actors[0].ResurrectionMovementFlags = 16;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[0].AllowDropOff, second.Actors[0].AllowDropOff);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool dropOff)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-100, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
            + (dropOff ? " + DROPOFF" : "") + "\n"));
    }
}
