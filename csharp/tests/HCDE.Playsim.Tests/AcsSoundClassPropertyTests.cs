using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSoundClassPropertyTests
{
    [Theory]
    [InlineData("custom", "custom")]
    [InlineData("", "player")]
    [InlineData("None", "None")]
    public void OverrideAndDefaultsPersistAndOldSaveClears(string value, string expected)
    {
        var sim = Room(); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        Assert.Equal("player", AcsActorStringProperties.Get(sim, player, 0, 47));
        AcsActorStringProperties.Set(sim, player, 0, 47, value);
        Assert.Equal(expected, AcsActorStringProperties.Get(sim, player, 0, 47));
        var saved = SimSavegame.Write(sim, true); sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        player.SoundClass = "changed"; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.Equal(expected, AcsActorStringProperties.Get(sim, player, 0, 47));
        Assert.Equal(checksum, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Null(player.SoundClass);
    }

    [Fact]
    public void NonPlayerAssignmentIsIgnoredAndMissingActorReturnsNoString()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a is not PlayerPawn);
        AcsActorStringProperties.Set(sim, actor, 0, 47, "custom");
        Assert.Equal("player", AcsActorStringProperties.Get(sim, actor, 0, 47));
        Assert.Null(AcsActorStringProperties.Get(sim, null, 99, 47));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckResolvesModuleAndGlobalNames(bool pooled)
    {
        var sim = Room(); var player = sim.Players.Single(); var pool = new AcsGlobalStrings();
        player.SoundClass = "Custom";
        var stack = new List<int> { 0, 47, pooled ? pool.Add("custom") : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = player },
            ["custom"], pool, 22, 3, out var result)); Assert.Equal(1, result);
    }

    [Fact]
    public void VmCopiesReturnedGlobalSoundClassToAnotherPlayer()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, Id = 8, X = 100 },
                new LevelThing { Type = 3004, Id = 7, X = 200 }]
        });
        var first = sim.Players.First();
        int[] words = [3, 0, 3, 47, 3, 0, (int)AcsPcode.SetActorProperty,
            3, 8, 3, 47, 3, 0, 3, 47, (int)AcsPcode.GetActorProperty, (int)AcsPcode.SetActorProperty, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = ["custom"] });
        Assert.True(sim.Acs.TryExecute(1, [], first)); sim.Acs.Tick(sim);
        Assert.Equal("custom", first.SoundClass);
        Assert.Equal("custom", sim.Players.Last().SoundClass);
        Assert.Equal("player", AcsActorStringProperties.Get(sim, null, 7, 47));
    }

    [Fact]
    public void MalformedStringLengthRejects()
    {
        var sim = Room(); sim.Players.Single().SoundClass = "custom";
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(260, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 12), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, Id = 7, X = 100 }, new LevelThing { Type = 1 }]
    });
}
