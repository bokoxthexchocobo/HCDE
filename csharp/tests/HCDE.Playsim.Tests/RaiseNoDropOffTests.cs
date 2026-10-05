using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseNoDropOffTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalClearsTemporaryLedgeProhibition(bool archvile, bool blocked)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-100, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL + DROPOFF\n"));
        var corpse = Assert.Single(sim.Actors);
        corpse.NoDropOff = true;
        Assert.False(ActorPhysics.TryMove(sim, corpse, 40, 0, out _));
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        if (blocked) sim.Ceilings[corpse.SectorIndex] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor { X = Fixed.FromInt(-60) }, new Actor { X = Fixed.FromInt(500) })
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoDropOff);
        if (!blocked) Assert.True(ActorPhysics.TryMove(sim, corpse, 40, 0, out _));
    }
}
