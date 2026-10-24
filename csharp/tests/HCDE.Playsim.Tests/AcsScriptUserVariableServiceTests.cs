using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScriptUserVariableServiceTests
{
    [Fact]
    public void CallFunc_SetAndGetUserVariable_RoundTripsIndexedSlot()
    {
        var sim = Room();
        var stack = new List<int> { 0, 0, 1, 7 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() },
            ["mycounter"], new AcsGlobalStrings(), AcsCallFunctions.SetUserVariable, 4, out _));
        stack = [0, 0, 1];
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() },
            ["mycounter"], new AcsGlobalStrings(), AcsCallFunctions.GetUserVariable, 3, out var value));
        Assert.Equal(7, value);
    }

    [Fact]
    public void IsValidIndex_RejectsOutOfRangeWithoutResize()
    {
        var sim = Room();
        var actor = sim.Players.Single();
        actor.SetScriptUserVar("arr", 0, 1);
        Assert.True(AcsScriptUserVariableService.IsValidIndex(actor, "arr", 0));
        Assert.False(AcsScriptUserVariableService.IsValidIndex(actor, "arr", 3));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
