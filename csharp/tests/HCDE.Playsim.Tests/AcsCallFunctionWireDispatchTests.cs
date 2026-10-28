namespace HCDE.Playsim.Tests;

public class AcsCallFunctionWireDispatchTests
{
    [Fact]
    public void CheckClassWireIdDispatchesToManagedHandler()
    {
        var stack = new List<int> { 0, 1 };
        Assert.True(AcsCallFunctions.TryInvoke(
            AuthoritySimulation.Start(new HCDE.MapLoader.PlayLevel { Things = [new HCDE.MapLoader.LevelThing { Type = 1 }] }),
            stack,
            new AcsActivatorBinding(),
            [],
            new AcsGlobalStrings(),
            AcsCallFunctionWireIds.NameToWire["CheckClass"],
            2,
            out var wireResult));
        Assert.True(AcsCallFunctions.TryInvoke(
            AuthoritySimulation.Start(new HCDE.MapLoader.PlayLevel { Things = [new HCDE.MapLoader.LevelThing { Type = 1 }] }),
            new List<int> { 0, 1 },
            new AcsActivatorBinding(),
            [],
            new AcsGlobalStrings(),
            AcsCallFunctions.CheckClass,
            2,
            out var slotResult));
        Assert.Equal(wireResult, slotResult);
    }
}
