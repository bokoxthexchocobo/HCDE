using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseMonsterClassificationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalRestoresPatchedMonsterClassification(bool monster, bool archvile)
    {
        var sim = Room(monster); var corpse = sim.Actors[1];
        Assert.Equal(monster, corpse.IsMonster);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.IsMonster = !monster;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(monster, corpse.IsMonster);
        Assert.Equal(monster ? 1 : 0, sim.Massacre());
        Assert.Equal(monster, corpse.IsDead);
    }

    private static AuthoritySimulation Room(bool monster) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE"
        + (monster ? " + COUNTKILL" : "") + "\n"));
}
