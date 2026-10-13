using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiManagedRendererTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlaybackRestartClearsPreviousPitchAdjustment(bool reopenDevice)
    {
        static VmMusicWildMidiManagedRenderer Create()
        {
            var bytes = new byte[847];
            "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
            bytes[198] = 1;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(247), 512);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(259), 22050);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(269), 440000);
            for (var i = 0; i < 512; i++) bytes[335 + i] = (byte)(i % 127);
            var patch = new VmMusicWildMidiInstrumentPatch(0);
            patch.Load(bytes);
            return new(44100, _ => patch, _ => 0);
        }
        static float[] Render(VmMusicWildMidiManagedRenderer renderer)
        {
            renderer.ShortEvent(0x90, 60, 100);
            var samples = new float[128];
            renderer.ComputeOutput(samples, 64);
            return samples;
        }
        using var fresh = Create();
        var expected = Render(fresh);
        Assert.Contains(expected, sample => sample != 0);
        using var reused = Create();
        using var device = new VmMusicWildMidiSessionDevice(reused);
        if (reopenDevice) device.Open();
        reused.ShortEvent(0xe0, 127, 127);
        reused.ShortEvent(0xb0, 99, 1);
        Assert.False(expected.SequenceEqual(Render(reused)));
        if (reopenDevice) { device.Close(); device.Open(); }
        else reused.ResetPlayback();
        Assert.Equal(0, reused.GetVoiceCount());
        Assert.Equal(expected, Render(reused));
    }

    [Fact]
    public void InterfaceSupportsEventsResetSilenceAndStatistics()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        VmMusicWildMidiSettings.Initialize(renderer, true, false);
        renderer.ShortEvent(0x90, 60, 100);
        renderer.LongEvent(new byte[] { 0xf0, 0x7e, 0x7f, 9, 1, 0xf7 });
        var output = new[] { 1f, 2f };
        renderer.ComputeOutput(output, 1);
        Assert.Equal(new[] { 0f, 0f }, output);
        Assert.Equal(0, renderer.GetVoiceCount());
    }

    [Fact]
    public void EnablingReverbSupportsSilentOutput()
    {
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        VmMusicWildMidiSettings.Initialize(renderer, true, true);
        var output = new float[200];
        renderer.ComputeOutput(output, 100);
        Assert.All(output, sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void DisposalGuardsAllRendererEntryPoints()
    {
        var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        renderer.Dispose();
        renderer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => renderer.ShortEvent(0x90, 60, 100));
        Assert.Throws<ObjectDisposedException>(() => renderer.LongEvent(Array.Empty<byte>()));
        Assert.Throws<ObjectDisposedException>(() => renderer.ComputeOutput(new float[2], 1));
        Assert.Throws<ObjectDisposedException>(() => renderer.LoadInstrument(0, 0, 0));
        Assert.Throws<ObjectDisposedException>(() => renderer.SetOption(2, 0));
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
    }
}
