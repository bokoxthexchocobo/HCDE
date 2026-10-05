using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DormantRaiseTimingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DormantRaiseAnimationFinishesWithoutWakingAi(bool archvile)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL + DORMANT\n"));
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        var duration = corpse.RaiseDuration;
        for (var tic = 1; tic <= duration; tic++)
        {
            corpse.Tick();
            Assert.Equal(duration - tic, corpse.Brain!.RaiseTics);
            Assert.True(corpse.Dormant);
            Assert.Null(corpse.Brain.TargetId);
        }
        ThingActivation.Execute(sim, corpse, 0, activate: true);
        for (var tic = 0; tic <= 8; tic++) corpse.Tick();
        Assert.False(corpse.Dormant);
        Assert.NotEqual(MonsterMode.Raise, corpse.Brain!.Mode);
    }
}
