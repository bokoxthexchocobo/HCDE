using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseSeekabilityFlagsTests
{
    [Theory]
    [InlineData(3004, 0, false)]
    [InlineData(3004, 1, false)]
    [InlineData(3004, 2, false)]
    [InlineData(58, 0, false)]
    [InlineData(58, 1, false)]
    [InlineData(58, 2, false)]
    [InlineData(3004, 0, true)]
    [InlineData(3004, 1, true)]
    [InlineData(3004, 2, true)]
    [InlineData(58, 0, true)]
    [InlineData(58, 1, true)]
    [InlineData(58, 2, true)]
    public void ResurrectionResetsRuntimeSeekFlagsOnlyOnSuccessAndReplaysAfterSave(int type, int path, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type, X = 20 }],
        });
        var corpse = sim.Actors.Single(a => a.DoomEdNum == type);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.CantSeek = corpse.DontSeekInvisible = corpse.MinVisible = true;
        corpse.DontThrust = true;
        corpse.Shadow = type != 58;
        if (blocked) sim.Ceilings[0] = 10;
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            var raised = path switch
            {
                0 => ActorRaiseActions.RaiseSelf(corpse),
                1 => ThingRaise.Execute(sim, 17, corpse, 0, 0) == true,
                _ => ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single()),
            };
            Assert.Equal(!blocked, raised);
            Assert.Equal(blocked, corpse.CantSeek);
            Assert.Equal(blocked, corpse.DontSeekInvisible);
            Assert.Equal(blocked, corpse.MinVisible);
            Assert.Equal(blocked, corpse.DontThrust);
            Assert.Equal(blocked ? type != 58 : type == 58, corpse.Shadow);
            if (!blocked)
            {
                Assert.True(new Actor().CanSeek(corpse));
                Assert.Equal(type != 58, new Actor { DontSeekInvisible = true }.CanSeek(corpse));
            }
            SimSavegame.Apply(sim, bytes);
            corpse = sim.Actors.Single(a => a.DoomEdNum == type);
            Assert.True(corpse.CantSeek); Assert.True(corpse.DontSeekInvisible); Assert.True(corpse.MinVisible);
            Assert.True(corpse.DontThrust);
        }
    }
}
