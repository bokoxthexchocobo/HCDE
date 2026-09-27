using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class PackedPushWalkerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SortedWordTableIncludesAlignmentPadding(int offset)
    {
        var countOffset = (offset + 7) & ~3;
        var bytes = new byte[countOffset + 16];
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), 256);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(countOffset), 1);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(countOffset + 12), 1);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, MapBehaviorFormat.AcsOld, (uint)offset,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(2, instructions.Count);
        Assert.Equal(countOffset + 12, instructions[1].Offset);
        Assert.Equal(countOffset + 8 - offset, instructions[0].OperandByteCount);
        Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.AsSpan(0, countOffset + 11),
            MapBehaviorFormat.AcsOld, (uint)offset, out _, out _, out _));
    }

    [Theory]
    [InlineData(0x52545053)]
    [InlineData(int.MaxValue)]
    public void MalformedOldDirectoryCountIsRejectedWithoutOverflow(int count)
    {
        var bytes = new byte[64]; BinaryPrimitives.WriteInt32LittleEndian(bytes, count);
        Assert.False(MapBehaviorDirectoryCodec.TryReadScripts(bytes, MapBehaviorFormat.AcsOld, 0, out _, out var error));
        Assert.Equal("behavior-script-directory-truncated", error);
    }

    [Theory]
    [InlineData(167, 1)]
    [InlineData(168, 2)]
    [InlineData(169, 3)]
    [InlineData(170, 4)]
    [InlineData(171, 5)]
    [InlineData(172, 6)]
    [InlineData(173, 1)]
    [InlineData(176, 2)]
    [InlineData(177, 3)]
    [InlineData(178, 4)]
    [InlineData(179, 5)]
    [InlineData(175, 0)]
    [InlineData(175, 255)]
    public void WordAndCompactWalkersUseExactPayloadLength(int opcode, int count)
    {
        foreach (var compact in new[] { false, true })
        {
            var width = compact ? 1 : 4;
            var payload = count + (opcode == 175 ? 1 : 0);
            var bytes = new byte[width + payload + width];
            if (compact) { bytes[0] = (byte)opcode; bytes[^1] = 1; }
            else { BinaryPrimitives.WriteInt32LittleEndian(bytes, opcode); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(width + payload), 1); }
            if (opcode == 175) bytes[width] = (byte)count;
            var format = compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld;
            Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, format, 0, out var instructions, out var terminated, out var error), error);
            Assert.True(terminated); Assert.Equal(2, instructions.Count);
            Assert.Equal(payload, instructions[0].OperandByteCount);
            Assert.Equal(width + payload, instructions[1].Offset);
            if (payload > 0)
                Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.AsSpan(0, width + payload - 1), format, 0, out _, out _, out _));
        }
    }
}
