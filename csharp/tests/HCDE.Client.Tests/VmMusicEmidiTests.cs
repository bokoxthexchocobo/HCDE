namespace HCDE.Client.Tests;

public class VmMusicEmidiTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(4, true)]
    [InlineData(7, false)]
    public void SourceCapabilitiesReachDesignationFilteringOnRestart(int technology, bool included)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [Control(0, 110, 7), new(5, 0x403c92), new(0, 0x0103d090)]));
        source.CheckCapabilities(technology);
        source.Restart();
        uint[] buffer = new uint[9];
        Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(5u, buffer[0]);
        Assert.Equal(included ? 0x403c92u : 0x02000000u, buffer[2]);
        Assert.Equal(0x0103d090u, buffer[5]);
    }

    [Fact]
    public void SourcePrecacheUsesFmDesignationsWhileExportSelectsGeneralMidi()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [Control(0, 110, 7), new(0, 0x05c2), new(2, 0x403c92)]));
        source.CheckCapabilities(4);
        Assert.Equal(new ushort[] { 5 }, VmMusicMidiPrecache.Collect(source));
        Assert.Equal(VmMusicSmf.Serialize(480, 500000, [new(2, 0x02000000)]), VmMusicSmfSource.Create(source, 1));
        Assert.Empty(VmMusicMidiPrecache.Collect(source));
    }
    private static VmMusicSmfEvent Control(uint delay, int number, int value) => new(delay, (uint)(0xb2 | (number << 8) | (value << 16)));

    [Fact]
    public void ProgramAndVolumeOverridesSuppressOrdinaryEventsPerTrack()
    {
        var result = VmMusicEmidi.Apply(new(500000, [Control(0, 112, 5), new(3, 0x06c2),
            Control(0, 113, 70), Control(4, 7, 100), Control(0, 39, 10), Control(500, 112, 9), Control(0, 113, 80)]), 480);
        Assert.Equal(new uint[] { 0x05c2, 0x02000000, 0x4607b2, 0x02000000, 0x09c2, 0x5007b2 }, result.Events.Select(x => x.Event));
        Assert.Equal(new uint[] { 0, 3, 0, 4, 500, 0 }, result.Events.Select(x => x.Delay));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, true)]
    public void TrackDesignationSelectsGmOrAdlibWithoutFilteringTempo(int technology, bool note)
    {
        var result = VmMusicEmidi.Apply(new(500000, [Control(0, 110, 7), new(5, 0x403c92), new(0, 0x0103d090)]), 480, technology);
        Assert.Equal(note ? 0x403c92u : 0x02000000u, result.Events[0].Event);
        Assert.Equal(5u, result.Events[0].Delay);
        Assert.Equal(0x0103d090u, result.Events[1].Event);
    }

    [Fact]
    public void ExclusionDefaultsToAllAndCanBeReincluded()
    {
        var result = VmMusicEmidi.Apply(new(500000,
            [Control(0, 111, 0), new(2, 0x403c92), Control(0, 110, 0), new(0, 0x403d92)]), 480);
        Assert.Equal(new uint[] { 0x02000000, 0x403d92 }, result.Events.Select(x => x.Event));
    }

    [Fact]
    public void InitBeatBoundaryPassesUnactivatedSpecialControllersThrough()
    {
        var input = new VmMusicSmfEvent[] { Control(480, 110, 7), Control(0, 112, 5), Control(0, 113, 70) };
        Assert.Equal(input, VmMusicEmidi.Apply(new(500000, input), 480).Events);
    }

    [Fact]
    public void DesignationAllAndInvalidValuesFollowNativeRules()
    {
        var result = VmMusicEmidi.Apply(new(500000,
            [Control(0, 110, 50), new(0, 0x403c92), Control(0, 110, 127), new(0, 0x403d92)]), 480);
        Assert.Equal(new uint[] { 0x403c92, 0x403d92 }, result.Events.Select(x => x.Event));
    }

    [Fact]
    public void SourceExportAppliesOverridesAndRestartIsDeterministic()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [Control(0, 112, 5), new(0, 0x06c2), Control(0, 113, 70), Control(2, 7, 100)]));
        var expected = VmMusicSmf.Serialize(480, 500000, [new(0, 0x05c2), Control(0, 7, 70), new(2, 0x02000000)]);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
    }
}
