using HCDE.MapLoader;
using HCDE.Gamedata;

namespace HCDE.Playsim.Tests;

public class RaiseAmbushDefaultsTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void RevivalRestoresClassAmbushInsteadOfMapOverride(bool classAmbush, bool mapAmbush, bool archvile)
    {
        var sim = Room(classAmbush, mapAmbush);
        var corpse = sim.Actors[1];
        Assert.Equal(classAmbush || mapAmbush, corpse.Ambush);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Ambush = !classAmbush;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(classAmbush, corpse.Ambush);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockedRevivalPreservesCurrentAmbush(bool archvile)
    {
        var sim = Room(false, true);
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        sim.Ceilings[0] = 10;
        Assert.False(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.True(corpse.Ambush);
    }

    private static AuthoritySimulation Room(bool classAmbush, bool mapAmbush) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Ambush = mapAmbush }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL" + (classAmbush ? " + AMBUSH" : "") + "\n"));
}
