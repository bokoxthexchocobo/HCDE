using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicSmfDocumentTests
{
    private static byte[] Document(ushort format, ushort division, params byte[][] tracks)
    {
        var bytes = new byte[14 + tracks.Sum(x => 8 + x.Length)];
        "MThd"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), 6);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), format);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), (ushort)tracks.Length);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(12), division);
        var offset = 14;
        foreach (var track in tracks)
        {
            "MTrk"u8.CopyTo(bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset + 4), (uint)track.Length);
            track.CopyTo(bytes, offset + 8);
            offset += 8 + track.Length;
        }
        return bytes;
    }

    [Fact]
    public void ParsesActualSerializerOutputAndOwnsTrackSnapshot()
    {
        var bytes = VmMusicSmf.Serialize(480, 500000, [new(0, 0x403c90)]);
        var expected = bytes[22..];
        var document = VmMusicSmfDocument.Parse(bytes);
        Array.Clear(bytes);
        Assert.Equal(0, document.Format);
        Assert.Equal(480, document.Division);
        Assert.Equal(expected, Assert.Single(document.Tracks).ToArray());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void MultitrackFormatsPreserveTrackOrderAndRawDivision(ushort format)
    {
        var document = VmMusicSmfDocument.Parse(Document(format, 0xe728, [1, 2], [], [3]));
        Assert.Equal(format, document.Format);
        Assert.Equal(0xe728, document.Division);
        Assert.Equal(3, document.Tracks.Count);
        Assert.Equal(new byte[] { 1, 2 }, document.Tracks[0].ToArray());
        Assert.Empty(document.Tracks[1].ToArray());
        Assert.Equal(new byte[] { 3 }, document.Tracks[2].ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void FormatZeroLoadsOneTrackRegardlessOfDeclaredCount(ushort count)
    {
        var bytes = Document(0, 140, [1], [2]);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), count);
        Assert.Equal(new byte[] { 1 }, Assert.Single(VmMusicSmfDocument.Parse(bytes).Tracks).ToArray());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(7, 7)]
    [InlineData(9, 3)]
    [InlineData(12, 0)]
    [InlineData(13, 0)]
    [InlineData(14, 0)]
    [InlineData(18, 255)]
    public void MalformedHeaderAndChunkFieldsAreRejected(int index, byte value)
    {
        var bytes = Document(0, 480, [1]);
        if (index == 12) bytes[13] = 0;
        if (index == 13) bytes[12] = 0;
        bytes[index] = value;
        Assert.Throws<InvalidDataException>(() => VmMusicSmfDocument.Parse(bytes));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(21)]
    [InlineData(22)]
    public void TruncatedHeaderOrTrackIsRejected(int length)
    {
        var bytes = Document(0, 480, [1]);
        Assert.Throws<InvalidDataException>(() => VmMusicSmfDocument.Parse(bytes.AsSpan(0, length)));
    }

    [Fact]
    public void MissingDeclaredTracksAndZeroMultitrackCountAreRejected()
    {
        var bytes = Document(1, 480, [1]);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), 2);
        Assert.Throws<InvalidDataException>(() => VmMusicSmfDocument.Parse(bytes));
        Assert.Throws<InvalidDataException>(() => VmMusicSmfDocument.Parse(Document(1, 480)));
    }
}
