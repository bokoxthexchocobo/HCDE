using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class MapBehaviorJumpTableTests
{
    [Fact]
    public void FirstChunkWinsAndOffsetsRemainUnsigned()
    {
        byte[] chunks = [.. Chunk("JUNK", 9), .. Chunk("JUMP", 24, uint.MaxValue), .. Chunk("JUMP", 99)];
        Assert.True(MapBehaviorJumpTableCodec.TryReadChunks(chunks, out var targets, out var error), error);
        Assert.Equal(new uint[] { 24, uint.MaxValue }, targets);
        chunks[20] = 0;
        Assert.Equal(24u, targets![0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOrEmptyFirstTableProducesEmptyResult(bool present)
    {
        byte[] chunks = present ? [.. Chunk("JUMP"), .. Chunk("JUMP", 24)] : Chunk("JUNK", 24);
        Assert.True(MapBehaviorJumpTableCodec.TryReadChunks(chunks, out var targets, out _));
        Assert.Empty(targets!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void RelocationPreservesUnalignedTargets(int offset)
    {
        Assert.True(MapBehaviorJumpTableCodec.TryRelocate(new uint[] { (uint)(24 + offset) }, 24, 8,
            out var points, out var error), error);
        Assert.Equal(new[] { offset }, points);
    }

    [Theory]
    [InlineData(23u)]
    [InlineData(29u)]
    [InlineData(uint.MaxValue)]
    public void OutsideOrPartialOpcodeTargetFailsAtomically(uint invalid)
    {
        Assert.False(MapBehaviorJumpTableCodec.TryRelocate(new uint[] { 24, invalid }, 24, 8, out var points, out _));
        Assert.Null(points);
    }

    [Fact]
    public void LastCompleteWordIsAccepted()
    {
        Assert.True(MapBehaviorJumpTableCodec.TryRelocate(new uint[] { 28 }, 24, 8, out var points, out _));
        Assert.Equal(new[] { 4 }, points);
    }

    [Fact]
    public void EveryNonemptyTruncatedChunkIsRejected()
    {
        var bytes = Chunk("JUMP", 24, 28);
        for (var length = 1; length < bytes.Length; length++)
        {
            Assert.False(MapBehaviorJumpTableCodec.TryReadChunks(bytes.AsSpan(0, length), out var targets, out _));
            Assert.Null(targets);
        }
    }

    [Fact]
    public void MalformedTailDoesNotExposeEarlierTable()
    {
        Assert.False(MapBehaviorJumpTableCodec.TryReadChunks([.. Chunk("JUMP", 24), 1], out var targets, out _));
        Assert.Null(targets);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void InvalidPayloadSizeIsRejected(int size)
    {
        var bytes = Chunk("JUMP", 24); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), size);
        Assert.False(MapBehaviorJumpTableCodec.TryReadChunks(bytes, out var targets, out _)); Assert.Null(targets);
    }

    private static byte[] Chunk(string id, params uint[] values)
    {
        var bytes = new byte[8 + values.Length * 4];
        System.Text.Encoding.ASCII.GetBytes(id).CopyTo(bytes, 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), values.Length * 4);
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8 + i * 4), values[i]);
        return bytes;
    }
}
