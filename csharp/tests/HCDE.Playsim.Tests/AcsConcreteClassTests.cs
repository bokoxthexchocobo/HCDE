using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsConcreteClassTests
{
    [Theory]
    [InlineData("DoomPlayer", 1)]
    [InlineData("doomplayer", 1)]
    [InlineData("PlayerPawn", 0)]
    [InlineData("Actor", 0)]
    public void PlayerCheckUsesConcreteClass(string name, int expected)
    {
        var sim = Room(); Assert.Equal(expected, Check(sim, sim.Players.Single(), name));
    }

    [Theory]
    [InlineData(3001, "DoomImp", "Zombieman")]
    [InlineData(58, "Spectre", "Demon")]
    [InlineData(2025, "RadSuit", "Clip")]
    public void RetainedDefinitionClassWinsOverChangedEditorNumber(int definition, string name, string rejected)
    {
        var sim = Room(); var actor = new Actor { DoomEdNum = 9999, DefinitionDoomEdNum = definition, Simulation = sim };
        Assert.Equal(1, Check(sim, actor, name)); Assert.Equal(0, Check(sim, actor, rejected));
        var globals = new AcsGlobalStrings(); var stack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], globals,
            AcsCallFunctions.GetActorClass, 1, out var result));
        Assert.Equal(name, globals.Get(result)); Assert.Empty(stack);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrUnknownActorDoesNotMatchNone(bool unknown)
    {
        var sim = Room(); var actor = unknown ? new Actor { DoomEdNum = 9999 } : null;
        Assert.Equal(0, Check(sim, actor, "None"));
    }

    private static int Check(AuthoritySimulation sim, Actor? actor, string name)
    {
        var stack = new List<int> { 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [name], new AcsGlobalStrings(),
            AcsCallFunctions.CheckActorClass, 2, out var result));
        Assert.Empty(stack); return result;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
