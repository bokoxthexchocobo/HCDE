using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class MapDormantSpawnTests
{
    [Fact]
    public void ImportedDormantMonsterHoldsStateUntilActivated()
    {
        const string text = "namespace = \"ZDoom\"; thing { type = 3004; skill3 = true; single = true; dormant = true; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01")); var actor = Assert.Single(sim.Actors);
        Assert.True(actor.Dormant); Assert.Equal(-1, actor.States.RemainingTics);
        var frame = actor.States.Current; var reaction = actor.ReactionTime;
        sim.Tick(); Assert.Equal(frame, actor.States.Current); Assert.Equal(reaction, actor.ReactionTime);
        Assert.True(ThingActivation.Execute(sim, actor, 0, true));
        Assert.False(actor.Dormant); Assert.Equal(1, actor.States.RemainingTics);
    }
    [Fact]
    public void MapDormantDoesNotDeactivatePlayer()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, Dormant = true }] });
        Assert.False(Assert.Single(sim.Players).Dormant);
    }
}