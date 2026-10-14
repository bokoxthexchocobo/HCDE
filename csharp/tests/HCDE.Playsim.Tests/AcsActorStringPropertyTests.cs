using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorStringPropertyTests
{
    [Theory]
    [InlineData(20)]
    [InlineData(46)]
    public void BytecodeCanCopyReturnedGlobalStringToTaggedActors(int property)
    {
        var sim = Room(); var player = sim.Players.Single();
        int[] words = [3, 0, 3, property, 3, 0, (int)AcsPcode.SetActorProperty,
            3, 7, 3, property, 3, 0, 3, property, (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.SetActorProperty, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = ["CustomType"] });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.Equal("CustomType", AcsActorStringProperties.Get(sim, player, 0, property));
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), actor =>
            Assert.Equal("CustomType", AcsActorStringProperties.Get(sim, actor, 0, property)));
    }

    [Theory]
    [InlineData(20, false)]
    [InlineData(20, true)]
    [InlineData(46, false)]
    [InlineData(46, true)]
    public void CheckActorPropertyResolvesModuleAndGlobalStringsIgnoringCase(int property, bool pooled)
    {
        var sim = Room(); var player = sim.Players.Single(); var pool = new AcsGlobalStrings();
        AcsActorStringProperties.Set(sim, player, 0, property, "CustomType");
        var value = pooled ? pool.Add("customtype") : 0;
        var stack = new List<int> { 0, property, value };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = player },
            ["customtype"], pool, 22, 3, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
        stack.AddRange([99, property, value]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = player },
            ["customtype"], pool, 22, 3, out result)); Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(20, "CustomType", "CustomType")]
    [InlineData(20, "None", "DoomPlayer")]
    [InlineData(20, "", "DoomPlayer")]
    [InlineData(46, "Fire", "Fire")]
    [InlineData(46, "None", "None")]
    [InlineData(46, "", "None")]
    public void NamedAndDefaultValuesSurviveSaveAndOldSnapshotReset(int property, string value, string expected)
    {
        var sim = Room(); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        AcsActorStringProperties.Set(sim, player, 0, property, value);
        Assert.Equal(expected, AcsActorStringProperties.Get(sim, player, 0, property));
        var saved = SimSavegame.Write(sim, true);
        AcsActorStringProperties.Set(sim, player, 0, property, "Changed");
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, AcsActorStringProperties.Get(sim, player, 0, property));
        SimSavegame.Apply(sim, old);
        Assert.Equal(property == 20 ? "DoomPlayer" : "None", AcsActorStringProperties.Get(sim, player, 0, property));
    }

    [Fact]
    public void SpeciesQueryRetainsInheritedMonsterSpeciesAndIgnoresDestroyedActors()
    {
        var sim = Room(); var spectre = sim.Actors.Single(a => a.DoomEdNum == 58);
        Assert.Equal("Demon", AcsActorStringProperties.Get(sim, spectre, 0, 20));
        spectre.Destroy();
        AcsActorStringProperties.Set(sim, spectre, 0, 20, "Changed");
        Assert.Null(AcsActorStringProperties.Get(sim, spectre, 0, 20)); Assert.Null(spectre.Species);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, Id = 7, X = 100 },
            new LevelThing { Type = 58, Id = 7, X = 200 }]
    });
}
