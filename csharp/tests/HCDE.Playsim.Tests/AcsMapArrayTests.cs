using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMapArrayTests
{
    [Fact]
    public void MiniLoadsBeforeDeclarationsAndArrayInitializersRegardlessOfChunkOrder()
    {
        var bytes = Chunk("AINI"u8, 6, 19).Concat(Chunk("ARAY"u8, 7, 1, 8, 1))
            .Concat(Chunk("MINI"u8, 6, 0, 99)).Concat(Chunk("MINI"u8, 6, 1)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(bytes, out var metadata, out var error), error);
        var sim = Room(); Load(sim, metadata!);
        Run(sim, 1, 3, 7, 29, 7, 3, 0, 207, 8, 14, 5, 112, 1);
        Assert.Equal(19, sim.LightOf(0)); // ARAY overwrites slot 7 with 0; MINI aliases slot 6 to array 1.
    }

    [Fact]
    public void ReloadClearsOldMapStateButPreservesWorldAndGlobalScopes()
    {
        var sim = Room(); Load(sim, new([new(7, 1)], [new(7, new[] { 35 })]));
        Run(sim, 1, 3, 10, 26, 127, 3, 20, 27, 127, 3, 30, 181, 63, 1);
        Load(sim, new([], []));
        Run(sim, 2, 3, 7, 29, 127, 30, 127, 14, 182, 63, 14, 3, 0, 207, 7, 14, 5, 112, 1);
        Assert.Equal(50, sim.LightOf(0));
    }

    [Fact]
    public void MiniOverlapsAndLastSlotAreAppliedWithOwnedValues()
    {
        var sim = Room(); var values = new[] { 10, 20 };
        Load(sim, new([], []) { MapInitializers = [new(126, values), new(127, new[] { 30 })] });
        values[0] = 99;
        Run(sim, 1, 3, 7, 29, 126, 29, 127, 14, 5, 112, 1); Assert.Equal(40, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(int.MaxValue)]
    public void InvalidMiniLoadLeavesExistingMapStateIntact(int first)
    {
        var sim = Room(); Load(sim, new([new(7, 1)], [new(7, new[] { 19 })]));
        var checksum = sim.Acs.Checksum;
        Assert.False(sim.Acs.TryLoadMapArrays(new([], []) { MapInitializers = [new(first, new[] { 1, 2 })] }, out _));
        Assert.Equal(checksum, sim.Acs.Checksum);
        Run(sim, 1, 3, 7, 3, 0, 207, 7, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(208, 7, 3, 3)]
    [InlineData(209, 7, 3, 10)]
    [InlineData(210, 7, 3, 4)]
    [InlineData(211, -7, 3, -21)]
    [InlineData(212, -7, 3, -2)]
    [InlineData(213, -7, 3, -1)]
    [InlineData(295, 6, 3, 2)]
    [InlineData(302, 6, 3, 5)]
    [InlineData(309, 6, 3, 7)]
    [InlineData(316, 3, 4, 48)]
    [InlineData(323, -9, 1, -5)]
    public void ArrayOperationsFollowMapVariableBinding(int opcode, int initial, int operand, int expected)
    {
        var sim = Room(); Load(sim, new([new(127, 2)], [new(127, new[] { 0, initial })]));
        Run(sim, 1, 3, 7, 3, 1, 3, operand, opcode, 127, 3, 1, 207, 127, 5, 112, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void IncrementDecrementAndAliasingPreserveStackAndShareState()
    {
        var sim = Room(); Load(sim, new([new(127, 2)], []));
        Run(sim, 1, 29, 127, 26, 6, 3, 7, 3, 1, 214, 6, 3, 1, 214, 127,
            3, 1, 215, 6, 3, 1, 207, 127, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void InvalidElementReadsZeroAndWritesAreIgnored(int index)
    {
        var sim = Room(); Load(sim, new([new(7, 2)], [new(7, new[] { 19 })]));
        Run(sim, 1, 3, index, 3, 35, 208, 7, 3, 7, 3, index, 207, 7, 5, 112, 1);
        Assert.Equal(0, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, 0, 207, 7, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void InvalidArrayBindingReadsZeroAndIgnoresWrites(int id)
    {
        var sim = Room(); Load(sim, new([new(7, 1)], []));
        Run(sim, 1, 3, id, 26, 7, 3, 0, 3, 35, 208, 7, 3, 7, 3, 0, 207, 7, 5, 112, 1);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(212)]
    [InlineData(213)]
    public void ZeroDivisorStopsBeforeChangingArray(int opcode)
    {
        var sim = Room(); Load(sim, new([new(7, 1)], [new(7, new[] { 19 })]));
        Run(sim, 1, 3, 0, 3, 0, opcode, 7, 10, 112, 7, 35, 1); Assert.Equal(128, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, 0, 207, 7, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(208, 7, true)]
    [InlineData(207, 7, false)]
    [InlineData(208, 128, true)]
    public void MalformedOperandsStopBeforeFollowingMutation(int opcode, int variable, bool push)
    {
        var sim = Room(); Load(sim, new([new(7, 1)], []));
        Run(sim, 1, push ? [3, 0, opcode, variable, 10, 112, 7, 35, 1] : [opcode, variable, 10, 112, 7, 35, 1]);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void NativeMetadataDecodesAndLoadsWithOrderedClippedInitializers()
    {
        var bytes = Chunk("ARAY"u8, 7, 1, 7, 3).Concat(Chunk("AINI"u8, 7, 10, 20, 30, 40))
            .Concat(Chunk("AINI"u8, 7, 19)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(bytes, out var metadata, out var error), error);
        var sim = Room(); Load(sim, metadata!); Array.Clear(bytes);
        Run(sim, 1, 3, 7, 29, 7, 3, 0, 207, 7, 14, 3, 1, 207, 7, 14, 3, 2, 207, 7, 14, 5, 112, 1);
        Assert.Equal(70, sim.LightOf(0)); // Binding 1 + 19 + 20 + 30.
    }

    [Fact]
    public void LoadCopiesInputAndUsesInitializedVariableAliases()
    {
        var sim = Room();
        var values = new[] { 19 }; Load(sim, new([new(7, 1), new(8, 1)], [new(6, values)])
        { MapInitializers = [new(6, new[] { 1 })] }); values[0] = 35;
        Run(sim, 2, 3, 7, 3, 0, 207, 8, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void RejectedLoadsPreserveValuesBindingsAndChecksum()
    {
        var sim = Room(); Load(sim, new([new(7, 1)], [new(7, new[] { 19 })])); var checksum = sim.Acs.Checksum;
        MapBehaviorMapArrayMetadata[] invalid = [new([new(7, uint.MaxValue)], []),
            new([new(7, AcsVm.MaximumMapArrayElements), new(8, 1)], []), new([new(128, 1)], []),
            new([new(7, 1)], [new(-1, new[] { 1 })]),
            new(Enumerable.Repeat(new MapBehaviorMapArrayDeclaration(7, 0), AcsVm.MaximumMapArrays + 1).ToArray(), [])];
        foreach (var metadata in invalid)
        {
            Assert.False(sim.Acs.TryLoadMapArrays(metadata, out var error)); Assert.NotNull(error);
            Assert.Equal(checksum, sim.Acs.Checksum);
        }
        Run(sim, 1, 3, 7, 3, 0, 207, 7, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void ActiveScriptPreventsReload()
    {
        var sim = Room(); Add(sim, 1, 56, 5, 1); Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim);
        var checksum = sim.Acs.Checksum;
        Assert.False(sim.Acs.TryLoadMapArrays(new([], []), out _)); Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Fact]
    public void ArrayShapeAndValuesParticipateInChecksum()
    {
        var a = Room(); var b = Room(); Load(a, new([new(7, 1)], [])); Load(b, new([new(7, 2)], []));
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
        Load(b, new([new(7, 1)], [])); Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
        Load(b, new([new(7, 1)], [new(7, new[] { 1 })])); Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    private static void Load(AuthoritySimulation sim, MapBehaviorMapArrayMetadata metadata) =>
        Assert.True(sim.Acs.TryLoadMapArrays(metadata, out var error), error);
    private static void Run(AuthoritySimulation sim, int number, params int[] words)
    {
        Add(sim, number, words); Assert.True(sim.Acs.Enqueue(number)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }
    private static void Add(AuthoritySimulation sim, int number, params int[] words) => sim.Acs.Add(new AcsProgram { Number = number, Code = Words(words) });
    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
    private static byte[] Chunk(ReadOnlySpan<byte> id, params int[] words)
    {
        var data = new byte[8 + words.Length * 4]; id.CopyTo(data); BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), words.Length * 4);
        Words(words).CopyTo(data, 8); return data;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
