using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetActorStateTests
{
    [Theory]
    [InlineData("Death.Fire", false, 2)]
    [InlineData("Death.Fire", true, -1)]
    [InlineData("Death.Extreme", false, 3)]
    [InlineData("Death.Extreme.More", false, 3)]
    [InlineData("Death", false, 2)]
    [InlineData("XDeath", true, 3)]
    [InlineData("xdeath.More", false, 3)]
    [InlineData("Burn", false, 2)]
    [InlineData("Ice", false, 2)]
    [InlineData("Disintegrate", false, 2)]
    [InlineData("..Death..Extreme..", true, 3)]
    [InlineData("...", false, -1)]
    public void NativeNamesChooseMostSpecificAvailableState(string name, bool exact, int expected)
    {
        var actor = new Actor { ExtremeDeathState = 3 };
        Assert.Equal(expected >= 0, AcsActorStates.TryFindNamedState(actor, name, exact, out var state));
        Assert.Equal(expected, state);
    }

    [Fact]
    public void BytecodeUsesPartialSpawnFallback()
    {
        var sim = Room(); var actor = sim.Actors.First();
        actor.States.Enter(actor, actor.GenericCrushState);
        Run(sim, [3, 7, 3, 0, 3, 0, 3, 0, 336, 5, 112, 1], "Spawn.Unknown", actor);
        Assert.Equal(1, sim.LightOf(0));
        Assert.Equal(actor.SpawnState, actor.States.Current);
    }

    [Theory]
    [InlineData("Null", 1)]
    [InlineData("GenericCrush", 1)]
    [InlineData("Death", 0)]
    [InlineData("Missing", 0)]
    public void ActivatorStateSelectionReturnsSuccess(string name, int expected)
    {
        var sim = Room(); var actor = sim.Actors.First();
        Run(sim, [3, 7, 3, 0, 3, 0, 3, 1, 336, 5, 112, 1], name, actor);
        Assert.Equal(expected, sim.LightOf(0));
        if (name == "Null") { Assert.Equal(actor.NullState, actor.States.Current); sim.Tick(); Assert.True(actor.Destroyed); }
        if (name == "GenericCrush") Assert.Equal(actor.GenericCrushState, actor.States.Current);
    }

    [Fact]
    public void TaggedSelectionCountsAllMatchingActors()
    {
        var sim = Room(); Run(sim, [3, 7, 3, 9, 3, 0, 3, 1, 336, 5, 112, 1], "GenericCrush", null);
        Assert.Equal(2, sim.LightOf(0));
        Assert.All(sim.Actors, actor => Assert.Equal(actor.GenericCrushState, actor.States.Current));
    }

    [Fact]
    public void MissingActivatorReturnsZero()
    {
        var sim = Room(); Run(sim, [3, 7, 3, 0, 3, 0, 3, 1, 336, 5, 112, 1], "Null", null);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void MissingArgumentsStopScriptBeforeFollowingAction()
    {
        var sim = Room(); Run(sim, [336, 3, 7, 3, 1, 112, 1], "Null", sim.Actors.First());
        Assert.Equal(128, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, int[] words, string name, Actor? activator)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = [name] });
        Assert.True(sim.Acs.TryExecute(1, [], activator)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 160, LightLevel = 128 }],
        Things = [new LevelThing { Type = 85, Id = 9 }, new LevelThing { Type = 30, Id = 9 }],
    });
}
