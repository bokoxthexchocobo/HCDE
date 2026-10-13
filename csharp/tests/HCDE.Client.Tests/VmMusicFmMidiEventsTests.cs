namespace HCDE.Client.Tests;

public class VmMusicFmMidiEventsTests
{
    [Theory]
    [InlineData(0x9f, 60, 100, "on:15:60:100")]
    [InlineData(0x90, 60, 0, "on:0:60:0")]
    [InlineData(0x8e, 60, 127, "off:14:60")]
    [InlineData(0xad, 61, 99, "key:13:61:99")]
    [InlineData(0xbc, 7, 80, "control:12:7:80")]
    [InlineData(0xcb, 42, 127, "patch:11:42")]
    [InlineData(0xda, 71, 127, "pressure:10:71")]
    [InlineData(0xe9, 1, 64, "bend:9:64:1")]
    public void DispatchPreservesNativeChannelAndParameterMapping(int status, int first, int second, string expected)
    {
        var device = new Device();
        VmMusicFmMidiEvents.HandleEvent(device, status, first, second);
        Assert.Equal(expected, Assert.Single(device.Calls));
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x70)]
    [InlineData(0xf0)]
    [InlineData(0xff)]
    public void UnsupportedShortCommandsDoNotReachRenderer(int status)
    {
        var device = new Device();
        VmMusicFmMidiEvents.HandleEvent(device, status, 1, 2);
        Assert.Empty(device.Calls);
    }

    [Fact]
    public void SystemExclusiveForwardsExactBoundedBytesIncludingFraming()
    {
        var device = new Device();
        byte[] packet = [99, 0xf0, 0x7e, 0, 0xf7, 88];
        VmMusicFmMidiEvents.HandleLongEvent(device, packet.AsSpan(1, 4));
        Assert.Equal(new byte[] { 0xf0, 0x7e, 0, 0xf7 }, device.LongData);
        Assert.Equal(new byte[] { 99, 0xf0, 0x7e, 0, 0xf7, 88 }, packet);
        VmMusicFmMidiEvents.HandleLongEvent(device, []);
        Assert.Empty(device.LongData!);
        Assert.Equal(2, device.LongCalls);
    }

    [Fact]
    public void RendererFailuresPropagateForShortAndLongEvents()
    {
        var failure = new InvalidOperationException("renderer failed");
        var device = new Device { Failure = failure };
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => VmMusicFmMidiEvents.HandleEvent(device, 0x90, 60, 100)));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => VmMusicFmMidiEvents.HandleLongEvent(device, new byte[] { 0xf0, 0xf7 })));
    }

    private sealed class Device : IVmMusicFmMidiDevice
    {
        public List<string> Calls { get; } = [];
        public byte[]? LongData { get; private set; }
        public int LongCalls { get; private set; }
        public Exception? Failure { get; init; }
        private void Record(string call)
        {
            if (Failure is not null) throw Failure;
            Calls.Add(call);
        }
        public void NoteOn(int channel, int note, int velocity) => Record($"on:{channel}:{note}:{velocity}");
        public void NoteOff(int channel, int note) => Record($"off:{channel}:{note}");
        public void NoteAfterTouch(int channel, int note, int pressure) => Record($"key:{channel}:{note}:{pressure}");
        public void ControllerChange(int channel, int controller, int value) => Record($"control:{channel}:{controller}:{value}");
        public void PatchChange(int channel, int program) => Record($"patch:{channel}:{program}");
        public void ChannelAfterTouch(int channel, int pressure) => Record($"pressure:{channel}:{pressure}");
        public void PitchBend(int channel, int mostSignificant, int leastSignificant) => Record($"bend:{channel}:{mostSignificant}:{leastSignificant}");
        public void SystemExclusive(ReadOnlySpan<byte> data)
        {
            if (Failure is not null) throw Failure;
            LongData = data.ToArray();
            LongCalls++;
        }
    }
}
