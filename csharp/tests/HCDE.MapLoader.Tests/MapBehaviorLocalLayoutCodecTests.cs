using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class MapBehaviorLocalLayoutCodecTests
{
    [Fact]
    public void CountsApplyBeforeArraysAndSignedScriptIdsArePreserved()
    {
        var data = Sary(-1, 2, 3).Concat(Svct(-1, 30)).ToArray();
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks(data, out var layouts, out var error), error);
        Assert.Equal(30, layouts![-1].LocalVariableCount); Assert.Equal(new[] { 2, 3 }, layouts[-1].ArraySizes);
        Array.Clear(data); Assert.Equal(2, layouts[-1].ArraySizes[0]);
    }

    [Fact]
    public void DefaultLocalsAndRepeatedArrayChunksMatchNativeOffsets()
    {
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks(Sary(7, 2, 3).Concat(Sary(7, 4)).ToArray(), out var layouts, out _));
        Assert.Equal(25, layouts![7].LocalVariableCount); Assert.Equal(4, Assert.Single(layouts[7].ArraySizes));
    }

    [Fact]
    public void FirstSvctWinsAndCountsAreUnsigned()
    {
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks(Svct(7, ushort.MaxValue).Concat(Svct(7, 1)).ToArray(), out var layouts, out _));
        Assert.Equal(65535, layouts![7].LocalVariableCount); Assert.Empty(layouts[7].ArraySizes);
    }

    [Fact]
    public void EmptyAndUnknownChunksHaveNoLayouts()
    {
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks([], out var layouts, out _)); Assert.Empty(layouts!);
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks(Chunk("TEST"u8, []), out layouts, out _)); Assert.Empty(layouts!);
    }

    [Theory]
    [InlineData("SVCT", 1)]
    [InlineData("SVCT", 3)]
    [InlineData("SARY", 1)]
    [InlineData("SARY", 2)]
    [InlineData("SARY", 5)]
    [InlineData("SARY", 7)]
    public void InvalidPayloadsRejectWithoutPartialLayouts(string id, int size)
    {
        Assert.False(MapBehaviorLocalLayoutCodec.TryReadChunks(Chunk(System.Text.Encoding.ASCII.GetBytes(id), new byte[size]), out var layouts, out _));
        Assert.Null(layouts);
    }

    [Fact]
    public void UnsupportedSizesAndTotalOverflowReject()
    {
        Assert.False(MapBehaviorLocalLayoutCodec.TryReadChunks(Sary(7, -1), out var layouts, out _)); Assert.Null(layouts);
        Assert.False(MapBehaviorLocalLayoutCodec.TryReadChunks(Sary(7, int.MaxValue), out layouts, out _)); Assert.Null(layouts);
    }

    [Fact]
    public void TruncatedAndOversizedChunksReject()
    {
        var data = Sary(7, 1);
        for (var length = 1; length < data.Length; length++)
            Assert.False(MapBehaviorLocalLayoutCodec.TryReadChunks(data.AsSpan(0, length), out _, out _));
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), int.MaxValue);
        Assert.False(MapBehaviorLocalLayoutCodec.TryReadChunks(data, out _, out _));
    }

    private static byte[] Svct(short script, ushort count)
    {
        var payload = new byte[4]; BinaryPrimitives.WriteInt16LittleEndian(payload, script);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(2), count); return Chunk("SVCT"u8, payload);
    }
    private static byte[] Sary(short script, params int[] sizes)
    {
        var payload = new byte[2 + sizes.Length * 4]; BinaryPrimitives.WriteInt16LittleEndian(payload, script);
        for (var i = 0; i < sizes.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(2 + i * 4), sizes[i]);
        return Chunk("SARY"u8, payload);
    }
    private static byte[] Chunk(ReadOnlySpan<byte> id, byte[] payload)
    {
        var bytes = new byte[8 + payload.Length]; id.CopyTo(bytes); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), payload.Length);
        payload.CopyTo(bytes, 8); return bytes;
    }
}
