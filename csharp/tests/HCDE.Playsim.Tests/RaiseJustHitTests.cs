using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseJustHitTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseClearsCombatHitFlagBeforeAnyBrainTick(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL + DORMANT\n"));
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.JustHit = true;
        if (blocked) sim.Ceilings[0] = 10;

        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.JustHit);
        Assert.True(corpse.Dormant);
        if (!blocked)
        {
            corpse.Tick();
            Assert.False(corpse.JustHit);
        }
    }
}
