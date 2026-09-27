using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public class MapBehaviorMapArrayCodecTests
{
    [Fact]
    public void MiniPreservesOrderOverlapsSignedValuesAndOwnsItsData()
    {
        var data = Chunk("MINI", 126, -1, int.MinValue).Concat(Chunk("MINI", 127, 19)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out _)); Array.Clear(data);
        Assert.Equal(new[] { 126, 127 }, result!.MapInitializers.Select(i => i.FirstVariable));
        Assert.Equal(new[] { -1, int.MinValue }, result.MapInitializers[0].Values);
        Assert.Equal(19, Assert.Single(result.MapInitializers[1].Values));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(128)]
    [InlineData(int.MaxValue)]
    public void MiniRejectsValuesOutsideMapStorage(int first)
    {
        Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("MINI", first, 19), out var result, out _));
        Assert.Null(result);
    }

    [Fact]
    public void MiniRangeMayEndAtLastSlotButCannotCrossIt()
    {
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("MINI", 127, 19), out _, out _));
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("MINI", 128), out _, out _));
        Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("MINI", 127, 19, 20), out var result, out _));
        Assert.Null(result);
    }

    [Fact]
    public void DeclarationsAndInitializersRetainNativeOrderAndSignedValues()
    {
        var data = Chunk("AINI", 7, -1, int.MinValue).Concat(Chunk("ARAY", 7, 3, 127, 0))
            .Concat(Chunk("AINI", 7, 19)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out var error), error);
        Assert.Equal(new[] { 7, 127 }, result!.Declarations.Select(d => d.MapVariable));
        Assert.Equal(new uint[] { 3, 0 }, result.Declarations.Select(d => d.Length));
        Assert.Equal(new[] { -1, int.MinValue }, result.Initializers[0].Values);
        Assert.Equal(19, Assert.Single(result.Initializers[1].Values));
    }

    [Fact]
    public void LengthMetadataDoesNotAllocateDeclaredStorage()
    {
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("ARAY", 0, -1), out var result, out _));
        Assert.Equal(uint.MaxValue, Assert.Single(result!.Declarations).Length);
    }

    [Fact]
    public void FirstDeclarationChunkWinsAndDuplicateBindingsArePreserved()
    {
        var data = Chunk("ARAY", 7, 1, 7, 2).Concat(Chunk("ARAY", 8, 3)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out _));
        Assert.Equal(new[] { 7, 7 }, result!.Declarations.Select(d => d.MapVariable));
    }

    [Fact]
    public void UnknownChunksAndEmptyRegionAreAccepted()
    {
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(Chunk("TEST", 1), out var result, out _));
        Assert.Empty(result!.Declarations); Assert.Empty(result.Initializers);
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks([], out result, out _));
        Assert.Empty(result!.Declarations);
    }

    [Fact]
    public void InitializerValuesAreOwnedAndNotClippedBeforeBinding()
    {
        var data = Chunk("ARAY", 0, 1).Concat(Chunk("AINI", 0, 19, 20)).ToArray();
        Assert.True(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out _));
        Array.Clear(data);
        Assert.Equal(new[] { 19, 20 }, Assert.Single(result!.Initializers).Values);
    }

    [Theory]
    [InlineData("ARAY", -1)]
    [InlineData("ARAY", 128)]
    [InlineData("AINI", -1)]
    [InlineData("AINI", 128)]
    public void InvalidMapVariableRejectsWithoutPartialOutput(string chunk, int variable)
    {
        Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(Chunk(chunk, variable, 1), out var result, out var error));
        Assert.Null(result); Assert.Equal("behavior-array-map-variable-out-of-range", error);
    }

    [Theory]
    [InlineData("ARAY", 4)]
    [InlineData("ARAY", 7)]
    [InlineData("AINI", 0)]
    [InlineData("AINI", 3)]
    [InlineData("AINI", 5)]
    [InlineData("MINI", 0)]
    [InlineData("MINI", 3)]
    [InlineData("MINI", 5)]
    public void MalformedPayloadLengthRejects(string id, int length)
    {
        var data = new byte[8 + length]; Encoding.ASCII.GetBytes(id).CopyTo(data, 0);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), length);
        Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out _)); Assert.Null(result);
    }

    [Fact]
    public void EveryTruncationAndOversizedChunkRejects()
    {
        var data = Chunk("ARAY", 0, 2);
        for (var size = 1; size < data.Length; size++)
        {
            Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(data.AsSpan(0, size), out var result, out _));
            Assert.Null(result);
        }
        foreach (var size in new[] { -1, int.MaxValue })
        {
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), size);
            Assert.False(MapBehaviorMapArrayCodec.TryReadChunks(data, out var result, out _)); Assert.Null(result);
        }
    }

    private static byte[] Chunk(string id, params int[] words)
    {
        var data = new byte[8 + words.Length * 4]; Encoding.ASCII.GetBytes(id).CopyTo(data, 0);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), words.Length * 4);
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8 + i * 4), words[i]);
        return data;
    }
}
