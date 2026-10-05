using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GibHealthCallbackOrderingTests
{
    [Theory]
    [InlineData(-5, 2)]
    [InlineData(-50, 1)]
    public void DamageDeathQueriesBeforeSpecialAndUsesLaterThreshold(int laterThreshold, int expectedState)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var actor = new QueryActor(laterThreshold) { Simulation = sim, Health = 20, Special = 19,
            DeathState = 1, ExtremeDeathState = 2 };
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1), new ActorFrame(-1, 2)], 0);
        ActorDamage.Apply(actor, 30, sim.Players.Single());
        Assert.Equal(new[] { 19, 0 }, actor.SpecialAtQuery);
        Assert.Equal(expectedState, actor.States.Current);
    }

    private sealed class QueryActor(int laterThreshold) : Actor
    {
        public List<int> SpecialAtQuery { get; } = [];
        public override int GetGibHealth()
        {
            SpecialAtQuery.Add(Special);
            return SpecialAtQuery.Count == 1 ? -100 : laterThreshold;
        }
    }
}
