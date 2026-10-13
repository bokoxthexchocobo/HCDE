namespace HCDE.Client.Tests;

public class VmMusicFmStreamEventsTests
{
    [Fact]
    public void ShortEventMasksParametersAndPreservesDelta()
    {
        var device = new Device();
        Assert.Equal(new VmMusicFmStreamEvent(17, 3, null),
            VmMusicFmStreamEvents.DispatchNext(device, new uint[] { 17, 99, 0x00c181e5 }));
        Assert.Equal((5, 65, 1), device.Bend);
    }

    [Fact]
    public void TempoAndNopDoNotReachRenderer()
    {
        var device = new Device();
        Assert.Equal(new VmMusicFmStreamEvent(0, 3, 500000),
            VmMusicFmStreamEvents.DispatchNext(device, new uint[] { 0, 0, 0x0107a120 }));
        Assert.Equal(new VmMusicFmStreamEvent(500, 3, null),
            VmMusicFmStreamEvents.DispatchNext(device, new uint[] { 500, 0, 0x02000000 }));
        Assert.Equal(0, device.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    public void LongEventUsesExactLengthAndConsumesPaddedWords(int length)
    {
        var device = new Device();
        uint[] buffer = [3, 0, 0x80000000 | (uint)length, 0x01007ef0, 0xffffffF7, 99];
        Assert.Equal(new VmMusicFmStreamEvent(3, 3 + (length + 3) / 4, null),
            VmMusicFmStreamEvents.DispatchNext(device, buffer));
        Assert.Equal(new byte[] { 0xf0, 0x7e, 0, 1, 0xf7 }[..length], device.Payload);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void TruncatedRecordsFailBeforeDispatch(int available)
    {
        var device = new Device();
        uint[] buffer = [0, 0, 0x80000005, 0, 0];
        Assert.Throws<ArgumentException>(() => VmMusicFmStreamEvents.DispatchNext(device, buffer.AsSpan(0, available)));
        Assert.Equal(0, device.Calls);
    }

    [Fact]
    public void UnknownLongRecordStillAdvancesOverItsPadding()
    {
        var device = new Device();
        Assert.Equal(new VmMusicFmStreamEvent(7, 5, null),
            VmMusicFmStreamEvents.DispatchNext(device, new uint[] { 7, 0, 0x81000005, 1, 2 }));
        Assert.Equal(0, device.Calls);
    }

    [Fact]
    public void ConvertedStartupPacketsReachRendererInOrderWithoutPadding()
    {
        var device = new Device();
        var words = new uint[16];
        var count = VmMusicMidiControlBuffers.WriteInitial(words, true, new VmMusicMidiChannelVolume());
        var position = 0;
        while (position < count)
            position += VmMusicFmStreamEvents.DispatchNext(device, words.AsSpan(position, count - position)).WordsConsumed;
        Assert.Equal(count, position);
        Assert.Equal(3, device.Calls);
        Assert.Equal(new byte[] { 0xf0, 0x7f, 0x7f, 4, 1, 0x7f, 0x7f, 0xf7 }, device.Payload);
    }

    private sealed class Device : IVmMusicFmMidiDevice
    {
        public int Calls { get; private set; }
        public (int, int, int) Bend { get; private set; }
        public byte[]? Payload { get; private set; }
        public void NoteOn(int channel, int note, int velocity) => Calls++;
        public void NoteOff(int channel, int note) => Calls++;
        public void NoteAfterTouch(int channel, int note, int pressure) => Calls++;
        public void ControllerChange(int channel, int controller, int value) => Calls++;
        public void PatchChange(int channel, int program) => Calls++;
        public void ChannelAfterTouch(int channel, int pressure) => Calls++;
        public void PitchBend(int channel, int mostSignificant, int leastSignificant)
        {
            Calls++;
            Bend = (channel, mostSignificant, leastSignificant);
        }
        public void SystemExclusive(ReadOnlySpan<byte> data)
        {
            Calls++;
            Payload = data.ToArray();
        }
    }
}
