using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedReactionTests
{
    [Theory]
    [InlineData(17)]
    [InlineData(0)]
    [InlineData(-5)]
    public void ExplicitReactionTimeSurvivesBrainAttachment(int reaction)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nReaction time = {reaction}\n");
        var actor = Spawn(patch);
        Assert.True(patch.Actors.Single(record => record.Index == 2).ReactionTimePatched);
        Assert.Equal(reaction, actor.ReactionTime);
        Assert.Equal(reaction, actor.Brain!.ReactionTics);
    }

    [Fact]
    public void UnrelatedPatchKeepsExistingManagedReactionDefault()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 88\n");
        Assert.False(patch.Actors.Single(record => record.Index == 2).ReactionTimePatched);
        Assert.Equal(10, Spawn(patch).ReactionTime);
    }

    [Fact]
    public void ChainedPatchPreservesReactionAssignmentAndBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nReaction time = 17\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        var third = DehackedPatch.Apply("Thing 2\nReaction time = 0\n", second);
        Assert.Equal(17, Spawn(first).ReactionTime);
        Assert.Equal(17, Spawn(second).ReactionTime);
        Assert.Equal(0, Spawn(third).ReactionTime);
    }

    [Fact]
    public void PatchedReactionControlsManagedAcquisitionDelay()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 400 }, new LevelThing { Type = 3004 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nReaction time = 2\n"));
        var actor = sim.Actors.Single(candidate => candidate.DoomEdNum == 3004);
        actor.Brain!.Tick(sim, actor); actor.Brain.Tick(sim, actor);
        Assert.Equal(0, actor.ReactionTime);
        Assert.Null(actor.Brain.TargetId);
        actor.Brain.Tick(sim, actor);
        Assert.Equal(sim.Players.Single().Id, actor.Brain.TargetId);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
