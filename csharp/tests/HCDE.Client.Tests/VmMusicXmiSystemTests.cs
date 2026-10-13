namespace HCDE.Client.Tests;

public class VmMusicXmiSystemTests
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
    [InlineData(0xfe, 0)]
    public void SystemMessagesConsumeExactlyTheirDataAndExportAsEscapes(byte status, int count)
    {
        var packet = new byte[count + 1];
        packet[0] = status;
        for (var i = 1; i < packet.Length; i++) packet[i] = (byte)i;
        var bytes = new byte[] { 7 }.Concat(packet).Concat(new byte[] { 0xc0, 9 }).ToArray();
        var track = VmMusicXmiEvents.Decode(bytes);
        Assert.Equal(2, track.Events.Count);
        Assert.Equal(7u, track.Events[0].Delay);
        Assert.Equal(packet, track.Events[0].Data.ToArray());
        Assert.Equal(0x80000000u | (uint)packet.Length, track.Events[0].Event);
        Assert.Equal(new VmMusicSmfEvent(0, 0x09c0), track.Events[1]);
        var exported = VmMusicSmf.Serialize(60, 500000, track.Events);
        var roundtrip = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(exported).Tracks[0].Span);
        Assert.Equal(packet, roundtrip.Events[0].Data.ToArray());
        Assert.Equal(new ushort[] { 9 }, VmMusicMidiPrecache.Scan(track.Events));
    }

    [Theory]
    [InlineData(0xf4)]
    [InlineData(0xf5)]
    [InlineData(0xf9)]
    [InlineData(0xfd)]
    public void UndefinedSystemStatusesAreRejected(byte status) =>
        Assert.Throws<InvalidDataException>(() => VmMusicXmiEvents.Decode([status]));

    [Theory]
    [InlineData(new byte[] { 0xf1 })]
    [InlineData(new byte[] { 0xf2, 1 })]
    [InlineData(new byte[] { 0xf3, 128 })]
    [InlineData(new byte[] { 0xf2, 1, 128 })]
    public void InvalidSystemPayloadsAreRejected(byte[] bytes) =>
        Assert.Throws<InvalidDataException>(() => VmMusicXmiEvents.Decode(bytes));
}
