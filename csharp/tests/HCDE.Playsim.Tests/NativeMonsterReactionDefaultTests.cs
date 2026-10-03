using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class NativeMonsterReactionDefaultTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    [InlineData(3006)]
    [InlineData(64)]
    [InlineData(16)]
    public void ClassFactoryMapAndDynamicSpawnUseEightTics(int type)
    {
        Assert.Equal(8, MonsterBrain.ForType(type)!.ReactionTics);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        Assert.Equal(8, Assert.Single(sim.Actors).ReactionTime);
        Assert.Equal(8, sim.AddBot(500, 0, type).ReactionTime);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultCounterCountsDownAndExplicitPatchWins(bool patched)
    {
        var patch = DehackedPatch.Apply(patched ? "Thing 2\nReaction time = 3\n" : "Thing 2\nHit points = 88\n");
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1, X = 800 }] }, dehacked: patch);
        var actor = sim.AddBot(0, 0);
        var initial = patched ? 3 : 8;
        Assert.Equal(initial, actor.ReactionTime);
        for (var tic = 1; tic <= initial; tic++)
        {
            actor.Brain!.Tick(sim, actor);
            Assert.Equal(initial - tic, actor.ReactionTime); Assert.Null(actor.Brain.TargetId);
        }
        actor.Brain!.Tick(sim, actor); Assert.Equal(sim.Players.Single().Id, actor.Brain.TargetId);
    }
}