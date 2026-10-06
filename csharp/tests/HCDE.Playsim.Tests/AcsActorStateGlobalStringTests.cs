using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorStateGlobalStringTests
{
    [Theory]
    [InlineData("Spawn", false, 1)]
    [InlineData("Spawn.Custom", false, 1)]
    [InlineData("Spawn.Custom", true, 0)]
    [InlineData("Missing", false, 0)]
    public void GlobalLabelQueriesPreserveExactAndFallbackBehavior(string label, bool exact, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        var globals = new AcsGlobalStrings(); var stack = new List<int> { 0, globals.Add(label), exact ? 1 : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], globals, AcsCallFunctions.CheckActorState, 3, out var result));
        Assert.Equal(expected, result); Assert.Empty(stack);
    }
}
