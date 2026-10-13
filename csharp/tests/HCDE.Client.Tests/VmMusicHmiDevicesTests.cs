using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicHmiDevicesTests
{
    [Theory]
    [InlineData(0xa000, 1, true)]
    [InlineData(0xa000, 4, false)]
    [InlineData(0xa000, 2, true)]
    [InlineData(0xa001, 1, true)]
    [InlineData(0xa001, 2, false)]
    [InlineData(0xa002, 4, true)]
    [InlineData(0xa002, 1, false)]
    [InlineData(0xa009, 4, true)]
    [InlineData(0xa008, 7, true)]
    [InlineData(0xa00a, 7, false)]
    [InlineData(0xa004, 1, false)]
    public void NativeDeviceMappingsAndAliasesArePreserved(ushort designation, int technology, bool expected) =>
        Assert.Equal(expected, VmMusicHmiDevices.IsEnabled(new ushort[] { designation }, technology));

    [Fact]
    public void StopsAtZeroAndAtEightEntriesAndFindsLaterMatches()
    {
        Assert.False(VmMusicHmiDevices.IsEnabled(new ushort[] { 0xa004, 0, 0xa000 }, 1));
        Assert.False(VmMusicHmiDevices.IsEnabled(Enumerable.Repeat((ushort)0xa004, 8).Append((ushort)0xa000).ToArray(), 1));
        Assert.True(VmMusicHmiDevices.IsEnabled(new ushort[] { 0xa004, 0xa002, 0xa000, 0 }, 1));
        Assert.False(VmMusicHmiDevices.IsEnabled(Array.Empty<ushort>(), 1));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 2)]
    [InlineData(2, 1)]
    public void SelectionPreservesOnlyEnabledTracksInDocumentOrder(int technology, int expectedFirst)
    {
        var bytes = new byte[0x100 + 3 * 0xaa];
        "HMI-MIDISONG061595\0"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xd4), 30);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xe4), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xe8), 0xf0);
        ushort[] designations = [0xa000, 0xa002, 0xa000];
        for (var i = 0; i < 3; i++)
        {
            var offset = 0x100 + i * 0xaa;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xf0 + i * 4), (uint)offset);
            "HMI-MIDITRACK"u8.CopyTo(bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 0x57), 0xa9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset + 0x99), designations[i]);
            bytes[offset + 0xa9] = (byte)(i + 1);
        }
        var tracks = VmMusicHmiDevices.SelectTracks(VmMusicHmiDocument.Parse(bytes), technology);
        Assert.Equal(expectedFirst, tracks[0].Events.Span[0]);
        Assert.Equal(technology == 4 ? new byte[] { 2 } : new byte[] { 1, 3 }, tracks.Select(track => track.Events.Span[0]));
    }
}
