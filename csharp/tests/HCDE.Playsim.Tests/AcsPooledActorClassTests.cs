using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsPooledActorClassTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3001)]
    [InlineData(2025)]
    public void GetterResultCanBePassedDirectlyToClassCheck(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Last(); var strings = new AcsGlobalStrings();
        var stack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], strings,
            AcsCallFunctions.GetActorClass, 1, out var classId));
        Assert.True(AcsStringIds.IsGlobalPool(classId));
        stack.AddRange([0, classId]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], strings,
            AcsCallFunctions.CheckActorClass, 2, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
    }

    [Theory]
    [InlineData("doomplayer", 1)]
    [InlineData("DoomImp", 0)]
    [InlineData("", 0)]
    public void PooledNamesUseExactCaseInsensitiveClassEquality(string name, int expected)
    {
        var sim = Room(1); var strings = new AcsGlobalStrings();
        var stack = new List<int> { 77, 0, strings.Add(name) };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() }, ["MisleadingModuleValue"], strings,
            AcsCallFunctions.CheckActorClass, 2, out var result));
        Assert.Equal(expected, result); Assert.Equal(new[] { 77 }, stack);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    [InlineData(AcsStringIds.StringPoolMarker + 99)]
    public void InvalidStringIdsFailClassMatch(int id)
    {
        var sim = Room(1); var stack = new List<int> { 0, id };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() }, [], new AcsGlobalStrings(),
            AcsCallFunctions.CheckActorClass, 2, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = type == 1 ? [new LevelThing { Type = 1 }] : [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 80 }]
    });
}
