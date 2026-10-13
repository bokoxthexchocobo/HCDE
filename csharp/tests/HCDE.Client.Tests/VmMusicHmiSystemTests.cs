using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicHmiSystemTests
{
    [Theory]
    [InlineData(0xf1, 1)]
    [InlineData(0xf2, 2)]
    [InlineData(0xf3, 1)]
    [InlineData(0xf6, 0)]
    [InlineData(0xf8, 0)]
    [InlineData(0xfa, 0)]
    [InlineData(0xfb, 0)]
    [InlineData(0xfc, 0)]
    public void BothFormatsPreserveSystemPacketsTimingAndRunningStatus(byte status, int count)
    {
        foreach (var hmi in new[] { true, false })
        {
            var zero = hmi ? (byte)0 : (byte)128;
            var seven = hmi ? (byte)7 : (byte)135;
            var packet = new byte[count + 1];
            packet[0] = status;
            for (var i = 1; i < packet.Length; i++) packet[i] = (byte)i;
            var bytes = new byte[] { zero, 0xc0, 4, seven }.Concat(packet).Concat(new byte[] { zero, 9 }).ToArray();
            var track = hmi ? VmMusicHmiEvents.Decode(bytes) : VmMusicHmpEvents.Decode(bytes);
            Assert.Equal(3, track.Events.Count);
            Assert.Equal(packet, track.Events[1].Data.ToArray());
            Assert.Equal(7u, track.Events[1].Delay);
            Assert.Equal(new VmMusicSmfEvent(0, 0x09c0), track.Events[2]);
            Assert.Equal(new ushort[] { 4, 9 }, VmMusicMidiPrecache.Scan(track.Events));
            var exported = VmMusicSmf.Serialize(120, track.InitialTempo, track.Events);
            var roundtrip = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(exported).Tracks[0].Span);
            Assert.Equal(packet, roundtrip.Events[1].Data.ToArray());
        }
    }

    [Theory]
    [InlineData(0xf4)]
    [InlineData(0xf5)]
    [InlineData(0xf9)]
    [InlineData(0xfd)]
    public void UndefinedStatusesFailInBothFormats(byte status)
    {
        Assert.Throws<InvalidDataException>(() => VmMusicHmiEvents.Decode([0, status]));
        Assert.Throws<InvalidDataException>(() => VmMusicHmpEvents.Decode([128, status]));
    }

    [Theory]
    [InlineData(new byte[] { 0xf1 })]
    [InlineData(new byte[] { 0xf2, 1 })]
    [InlineData(new byte[] { 0xf3, 128 })]
    [InlineData(new byte[] { 0xf2, 1, 128 })]
    public void MalformedPayloadsFailInBothFormats(byte[] packet)
    {
        Assert.Throws<InvalidDataException>(() => VmMusicHmiEvents.Decode(new byte[] { 0 }.Concat(packet).ToArray()));
        Assert.Throws<InvalidDataException>(() => VmMusicHmpEvents.Decode(new byte[] { 128 }.Concat(packet).ToArray()));
    }

    [Fact]
    public void FactorySourceExportsSystemMessagesAndPrecacheRewinds()
    {
        byte[] events = [128, 0xc0, 7, 130, 0xf2, 1, 2, 128, 8];
        var bytes = new byte[0x314 + events.Length];
        "HMIMIDIP"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x30), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x38), 120);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x30c), (uint)events.Length + 12);
        events.CopyTo(bytes, 0x314);
        var factory = new VmMusicMidiSourceFactory();
        using var source = Assert.IsType<VmMusicHmiSource>(factory.Create(bytes, VmMusicMidiType.Hmi));
        Assert.Equal(new ushort[] { 7, 8 }, VmMusicMidiPrecache.Collect(source));
        Assert.False(source.CheckDone());
        Assert.Equal(VmMusicSmf.Serialize(120, 1000000, VmMusicHmpEvents.Decode(events).Events), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void FeRemainsSpecialRecordInsteadOfActiveSensing()
    {
        Assert.Empty(VmMusicHmiEvents.Decode([0, 0xfe, 0x12, 1, 2]).Events);
        Assert.Empty(VmMusicHmpEvents.Decode([128, 0xfe, 0x12, 1, 2]).Events);
        Assert.Throws<InvalidDataException>(() => VmMusicHmiEvents.Decode([0, 0xfe]));
    }
}
