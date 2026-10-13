using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiDeviceRenderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DryRenderingMixesBoundVoiceAndAppliesGainOnce(bool enhanced)
    {
        using var resources = new VmMusicWildMidiDeviceResources();
        resources.Notes.NoteOn(0, 60, 100, false, (channel, note, velocity) => new(new()
        {
            NoteId = 60, Velocity = velocity, SampleIncrement = 1024,
            EnvelopeTargets = new int[7], EnvelopeRates = new int[7], MixVolume = new(1024, 512)
        }, new(new short[] { 1000, 2000, 0 }, new(2048, 0, 0, 0))));
        var voice = Assert.Single(resources.Notes.Voices);
        voice.EnvelopeLevel = 1024 << 12;
        var output = new[] { -1f, -1f, -1f, -1f, 99f };
        resources.RenderDry(output, 2, enhanced);
        const float gain = 1.3f / 32768f;
        Assert.Equal(new[] { 1000 * gain, 500 * gain, 2000 * gain, 1000 * gain, 99f }, output);
        Assert.Empty(resources.Notes.Voices);
    }

    [Fact]
    public void EmptyDeviceRendersSilenceAndZeroFramesPreservesOutput()
    {
        using var resources = new VmMusicWildMidiDeviceResources();
        var output = new[] { 3f, 4f, 5f };
        resources.RenderDry(output, 0);
        Assert.Equal(new[] { 3f, 4f, 5f }, output);
        resources.RenderDry(output, 1);
        Assert.Equal(new[] { 0f, 0f, 5f }, output);
    }

    [Fact]
    public void InvalidBuffersAndDisposedDeviceFailBeforeRendering()
    {
        var resources = new VmMusicWildMidiDeviceResources();
        Assert.Throws<ArgumentException>(() => resources.RenderDry(new float[1], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => resources.RenderDry(new float[2], -1));
        resources.Dispose();
        Assert.Throws<ObjectDisposedException>(() => resources.RenderDry(new float[2], 1));
    }
}
