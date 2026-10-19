using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PSavegRoundTripTests
{
    [Fact]
    public void SectorPlanesAndTerrainsRoundTrip()
    {
        var original = Room();
        original.Floors[0] = 24;
        original.Ceilings[0] = 200;
        original.Level.Sectors[0].FloorTerrain = "FLOOR1";
        original.Level.Sectors[0].CeilingTerrain = "CEIL1";
        var bytes = SimSavegame.Write(original, includeBotIdentities: true);
        Assert.Equal(364, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var restored = Room();
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(24, restored.Floors[0]);
        Assert.Equal(200, restored.Ceilings[0]);
        Assert.Equal("FLOOR1", restored.Level.Sectors[0].FloorTerrain);
        Assert.Equal("CEIL1", restored.Level.Sectors[0].CeilingTerrain);
    }

    [Fact]
    public void LineActionsRoundTrip()
    {
        var original = Room();
        new SimLineAction(11, 42, 31).Apply(original.Level.Lines[0]);
        var bytes = SimSavegame.Write(original, includeBotIdentities: true);
        var restored = Room();
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(new SimLineAction(11, 42, 31), SimLineAction.Capture(restored.Level.Lines[0]));
    }

    [Fact]
    public void PlayerAmmoAndThinkerOrderRoundTrip()
    {
        var original = Room();
        var player = original.Players.Single();
        player.Inventory.Shells = 33;
        original.Thinkers.Remove(player);
        original.Thinkers.Add(player, 1);
        var bytes = SimSavegame.Write(original, includeBotIdentities: true);
        var restored = Room();
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(33, restored.Players.Single().Inventory.Shells);
        Assert.Equal(1, restored.Players.Single().StatNum);
    }

    [Fact]
    public void AcsGlobalsMapWorldAndSuspendedFiberRoundTrip()
    {
        var original = Room();
        original.Acs.Add(CloneProgram(1));
        Assert.True(original.Acs.Enqueue(1, [0], original.Players.Single()));
        original.Tick();
        Assert.Equal(1, original.Acs.SuspendedCount);
        var bytes = SimSavegame.Write(original, includeBotIdentities: true);
        Assert.Equal(366, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out _, out var readError), readError);
        var restored = Room();
        restored.Acs.Add(CloneProgram(1));
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(original.Checksum, restored.Checksum);
        Assert.Equal(1, restored.Acs.SuspendedCount);
        Assert.NotNull(restored.CaptureState(includeBotIdentities: true).Acs);
    }

    [Fact]
    public void LegacyArchiveWithoutAcsTrailerPreservesZeroedVariables()
    {
        var original = Room();
        original.Acs.Add(CloneProgram(1));
        Assert.True(original.Acs.ExecuteAlways(1, ReadOnlySpan<int>.Empty));
        original.Tick();
        var state = original.CaptureState(includeBotIdentities: true);
        state.Acs = null;
        var legacy = SimSavegame.Write(state);
        Assert.True(BinaryPrimitives.ReadUInt16LittleEndian(legacy.AsSpan(4)) < 366);
        var restored = Room();
        restored.Acs.Add(CloneProgram(1));
        SimSavegame.Apply(restored, legacy);
        Assert.Null(restored.CaptureState(includeBotIdentities: true).Acs);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "SAVE01",
        Lines = [new LevelLine { Tag = 7, Special = 0, V1 = 0, V2 = 1, SideFront = 0 }],
        Sectors = [new LevelSector { Tag = 9, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 64, Y = 64 }],
        Vertices = [new LevelVertex(), new LevelVertex { X = 128 }],
        Sides = [new LevelSide()],
    });

    private static AcsProgram CloneProgram(int number) => new()
    {
        Number = number,
        LocalVariableCount = 4,
        Code = Words(
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.AssignGlobalVar, 0,
            (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.AssignMapVar, 1,
            (int)AcsPcode.PushNumber, 7,
            (int)AcsPcode.AssignWorldVar, 2,
            (int)AcsPcode.Suspend,
            (int)AcsPcode.Terminate),
    };

    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
}
