using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicHmiSourceTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HmiAndHmpAcceptLiveModeAndPreserveFiniteOutputAndRestart(bool hmp, bool live)
    {
        using var source = new VmMusicHmiSource(hmp ? Hmp() : Hmi());
        source.StartPlayback(live, 3);
        Assert.Equal(3, source.LoopLimit);
        uint[] first = new uint[6], restarted = new uint[6];
        Assert.Equal(6, source.MakeEvents(first, uint.MaxValue));
        Assert.True(source.CheckDone());
        Assert.Equal(0, source.MakeEvents(restarted, uint.MaxValue));
        source.Restart();
        Assert.Equal(6, source.MakeEvents(restarted, uint.MaxValue));
        Assert.Equal(first, restarted);
    }

    private static byte[] Hmi()
    {
        var bytes = new byte[0x26a];
        "HMI-MIDISONG061595\0"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xd4), 30);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xe4), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xe8), 0xf0);
        byte[][] events = [[0, 0x90, 60, 100, 4, 0, 0xff, 47, 0], [0, 0xc1, 7]];
        var offset = 0x100;
        for (var i = 0; i < 2; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xf0 + i * 4), (uint)offset);
            "HMI-MIDITRACK"u8.CopyTo(bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 0x57), 0xa9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset + 0x99), (ushort)(i == 0 ? 0xa000 : 0xa002));
            events[i].CopyTo(bytes, offset + 0xa9);
            offset += 0xa9 + events[i].Length;
        }
        Array.Resize(ref bytes, offset);
        return bytes;
    }

    private static byte[] Hmp()
    {
        var bytes = new byte[0x31a];
        "HMIMIDIP"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x30), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x38), 120);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x30c), 18);
        new byte[] { 0x80, 0xc0, 7, 0x80, 0xc0, 8 }.CopyTo(bytes, 0x314);
        return bytes;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FactoryCreatesBothFormatsAndOwnsSnapshot(bool hmp)
    {
        var factory = new VmMusicMidiSourceFactory();
        var bytes = hmp ? Hmp() : Hmi();
        using var source = Assert.IsType<VmMusicHmiSource>(factory.Create(bytes, VmMusicMidiType.Hmi));
        Array.Clear(bytes);
        var expected = hmp ? VmMusicSmf.Serialize(120, 1000000, [new(0, 0x07c0), new(0, 0x08c0)])
            : VmMusicSmf.Serialize(120, 4000000, [new(0, 0x643c90), new(4, 0x3c90)]);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
        source.Restart();
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void DeviceSelectionChangesAtRestartAndExportSelectsMidiPort()
    {
        using var source = new VmMusicHmiSource(Hmi());
        source.CheckCapabilities(4);
        source.Restart();
        uint[] buffer = new uint[3];
        Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(0x07c1u, buffer[2]);
        Assert.True(source.CheckDone());
        Assert.Equal(VmMusicSmf.Serialize(120, 4000000, [new(0, 0x643c90), new(4, 0x3c90)]),
            VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void PrecacheRewindsSelectedDeviceAndFindsPrograms()
    {
        using var source = new VmMusicHmiSource(Hmi());
        source.CheckCapabilities(4);
        Assert.Equal(new ushort[] { 7 }, VmMusicMidiPrecache.Collect(source));
        Assert.False(source.CheckDone());
        Assert.Equal(1, source.LoopLimit);
    }

    [Fact]
    public void SmallBatchesDoNotLoseAutomaticNoteOffs()
    {
        using var source = new VmMusicHmiSource(Hmi());
        uint[] buffer = new uint[3];
        Assert.Equal(3, source.MakeEvents(buffer, 0));
        Assert.Equal(0x643c90u, buffer[2]);
        Assert.Equal(3, source.MakeEvents(buffer, 0));
        Assert.Equal(4u, buffer[0]);
        Assert.Equal(0x3c90u, buffer[2]);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void MalformedEventsReportFactoryErrorAndSuccessClearsIt()
    {
        var factory = new VmMusicMidiSourceFactory();
        var bytes = Hmp(); bytes[^1] = 128;
        Assert.Null(factory.Create(bytes, VmMusicMidiType.Hmi));
        Assert.NotEmpty(factory.LastError);
        using var source = factory.Create(Hmp(), VmMusicMidiType.Hmi);
        Assert.NotNull(source);
        Assert.Equal("", factory.LastError);
    }

    [Fact]
    public void InvalidLoopLimitAndDisposedMethodsFailExplicitly()
    {
        var source = new VmMusicHmiSource(Hmi());
        Assert.Throws<ArgumentOutOfRangeException>(() => source.StartPlayback(true, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.StartPlayback(false, -1));
        source.Dispose();
        Assert.Throws<ObjectDisposedException>(() => source.StartPlayback(true, 0));
        Assert.Throws<ObjectDisposedException>(() => source.Restart());
        Assert.Throws<ObjectDisposedException>(() => source.CheckCapabilities(1));
        Assert.Throws<ObjectDisposedException>(() => source.CheckDone());
        Assert.Throws<ObjectDisposedException>(() => source.MakeEvents(new uint[3], 0));
    }
}
