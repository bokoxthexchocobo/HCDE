using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbOutputTests
{
    [Fact]
    public void OutputPipelineAppliesReverbBeforeFloatGainAndKeepsTail()
    {
        using var resources = new VmMusicWildMidiDeviceResources();
        resources.Notes.Voices.Add(new VmMusicWildMidiNoteReleaseState
        {
            Active = true, EnvelopeLevel = 4194303, MixVolume = new(1024, 0),
            Sample = new(new short[] { 4096, 4096, 0, 0 }, new(2048, 0, 1024, 1024))
        });
        var coefficients = new int[8, 6, 5];
        coefficients[0, 0, 0] = 1024;
        var reverb = new VmMusicWildMidiReverbDelay(1, 1, new int[8], new int[8], new int[4], new int[4], coefficients);
        var dry = new float[2];
        resources.RenderDry(dry, 1);
        var wet = new float[] { 0, 0, 99 };
        resources.Render(wet, 1, false, reverb);
        Assert.True(wet[0] > dry[0]);
        Assert.True(wet[1] > 0);
        Assert.Equal(99f, wet[2]);
        resources.Notes.Clear();
        var tail = new float[2];
        resources.Render(tail, 1, false, reverb);
        Assert.True(tail[1] > 0);
        reverb.Reset();
        resources.Render(tail, 1, false, reverb);
        Assert.Equal(new[] { 0f, 0f }, tail);
    }

    [Fact]
    public void RendererAcceptsConfiguredRoomAndOptionTransitions()
    {
        var settings = new VmMusicWildMidiReverbSettings();
        settings.Apply("reverb_room_width 1");
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0, settings);
        renderer.SetOption(4, 4);
        renderer.SetOption(4, 0);
        renderer.SetOption(7, 7);
        renderer.ComputeOutput(new float[2], 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => renderer.SetOption(8, 0));
    }
}
