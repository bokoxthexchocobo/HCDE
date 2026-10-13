using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiSessionDeviceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroDurationInfiniteLiveLoopFailsAndClosesSession(bool global)
    {
        var start = global ? 118 : 116;
        var end = global ? 119 : 117;
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, (uint)(0xb0 | start << 8)), new(0, 0x643c90), new(0, (uint)(0x7f00b0 | end << 8))]));
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer);
        using var session = new VmMusicMidiSession(source, device);
        Assert.Throws<InvalidDataException>(() => session.Start(600000, liveLoops: true));
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void FiniteZeroDurationLiveLoopCanComplete()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0174b0), new(0, 0x643c90), new(0, 0x7f75b0)]));
        source.StartPlayback(true, 0);
        source.MakeEvents(new uint[1024], 600000);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void RepeatingShortSongPrimesAndRefillsWithoutStopBuffer()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer);
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: true));
        for (var i = 0; i < 50; i++) Assert.True(device.ServiceStream(new float[2048]));
        Assert.Equal(VmMusicMidiEndState.Playing, session.Startup.Buffers.EndState);
        Assert.Equal(0, session.LastServiceError);
    }

    [Fact]
    public void ZeroDurationRepeatingSongFailsInsteadOfRefillingForever()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, [new(0, 0x643c90)]));
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer);
        using var session = new VmMusicMidiSession(source, device);
        Assert.Throws<InvalidDataException>(() => session.Start(600000, repeatSong: true));
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void RuntimeSettingsAndStatisticsAreWiredToOpenRenderer()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        using var device = new VmMusicWildMidiSessionDevice(renderer);
        device.Open();
        Assert.True(device.ChangeInteger("WILDMIDI.REVERB", 1));
        Assert.True(device.ChangeInteger("wildmidi.resampling", 1));
        Assert.False(device.ChangeInteger("unknown", int.MaxValue));
        Assert.Equal("  0 voices", device.GetStatistics());
        Assert.Throws<ArgumentOutOfRangeException>(() => device.ChangeInteger("wildmidi.reverb", -1));
        Assert.Throws<ArgumentNullException>(() => device.ChangeInteger(null!, 1));
        Assert.True(device.ChangeInteger("wildmidi.reverb", 0));
        device.Dispose();
        Assert.Throws<ObjectDisposedException>(() => device.ChangeInteger("wildmidi.reverb", 1));
        Assert.Throws<ObjectDisposedException>(() => device.GetStatistics());
        Assert.Equal(0, renderer.GetVoiceCount());
    }

    [Fact]
    public void ClosedDeviceCannotMutateBorrowedRendererSettings()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        using var device = new VmMusicWildMidiSessionDevice(renderer);
        Assert.Throws<InvalidOperationException>(() => device.ChangeInteger("wildmidi.reverb", 1));
        device.Open();
        device.Close();
        Assert.Throws<InvalidOperationException>(() => device.GetStatistics());
        device.Open();
        Assert.True(device.ChangeInteger("wildmidi.resampling", 0));
    }

    [Fact]
    public void SessionUsesActualFactoryRateAndRejectsMismatchedClock()
    {
        using var renderer = VmMusicWildMidiFactory.CreateConfigured(new(_ => null), 0, false, false);
        Assert.Equal(44100, renderer.SampleRate);
        using var device = new VmMusicWildMidiSessionDevice(renderer);
        Assert.Equal(VmMusicSoftSynthStreamInfo.Get(44100), device.GetStreamInfo());
        Assert.Equal(0, device.Open());
        Assert.Throws<ArgumentException>(() => new VmMusicWildMidiSessionDevice(renderer, 22050));
    }

    [Fact]
    public void ByteStreamRequiresCompleteStereoFramesAndClearsPausedOutput()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(22050, _ => null, _ => 0);
        using var device = new VmMusicWildMidiSessionDevice(renderer);
        device.Open();
        device.Prepare(0, new uint[] { 100, 0, 0x643c90 });
        device.StreamOut(0);
        device.StartStream();
        device.Pause(true);
        var output = Enumerable.Repeat((byte)255, 16).ToArray();
        Assert.True(device.ServiceStreamBytes(output));
        Assert.All(output, value => Assert.Equal((byte)0, value));
        Assert.Throws<ArgumentException>(() => device.ServiceStreamBytes(new byte[7]));
    }

    [Fact]
    public void SourceSessionPrimesBuffersAndServicesCompletions()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(480, 0x3c80), new(480, 0x643d90), new(480, 0x3d80)]));
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer, 44100);
        using (var session = new VmMusicMidiSession(source, device))
        {
            Assert.True(session.Start(100000));
            var output = new float[2000];
            for (var i = 0; i < 100 && device.Update(); i++) device.ServiceStream(output);
            Assert.False(device.Update());
            Assert.Equal(0, session.LastServiceError);
            Assert.Null(session.LastServiceException);
        }
        Assert.False(device.IsOpen);
        Assert.Equal(0, renderer.GetVoiceCount());
    }

    [Fact]
    public void PausedSessionProducesSilenceAndAdapterBorrowsRenderer()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer, 44100);
        device.Open();
        device.Prepare(0, new uint[] { 100, 0, 0x643c90 });
        device.StreamOut(0); device.StartStream(); device.Pause(true);
        var output = new[] { 1f, 1f };
        device.ServiceStream(output);
        Assert.Equal(new[] { 0f, 0f }, output);
        device.Dispose();
        Assert.Equal(0, renderer.GetVoiceCount());
        Assert.Throws<ObjectDisposedException>(() => device.Open());
    }
}
