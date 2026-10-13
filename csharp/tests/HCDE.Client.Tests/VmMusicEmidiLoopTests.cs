namespace HCDE.Client.Tests;

public class VmMusicEmidiLoopTests
{
    [Fact]
    public void LiveMultitrackLocalLoopInterleavesAndRestarts()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(0, 0x0074b0), new(2, 0x403c90), new(3, 0x7f75b0)],
            [new(5, 0x403d91), new(0, 0x403e91)]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[15];
        Assert.Equal(15, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x403c90, 3, 0, 0x02000000,
            0, 0, 0x403d91, 0, 0, 0x403e91, 2, 0, 0x403c90 }, buffer);
        Assert.False(source.CheckDone());
        source.Restart();
        var restarted = new uint[15];
        Assert.Equal(15, source.MakeEvents(restarted, uint.MaxValue));
        Assert.Equal(buffer, restarted);
    }

    [Fact]
    public void LiveFormatTwoSequencesFiniteTracksWithLocalLoops()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [new(0, 0x0174b0), new(2, 0x403c90), new(0, 0x7f75b0)],
            [new(3, 0x403d91)]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[12];
        Assert.Equal(9, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x403c90, 2, 0, 0x403c90, 3, 0, 0x403d91 }, buffer[..9]);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void MultitrackGlobalLoopsStreamBoundedBatchesAndRestart()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(0, 0x0076b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [new(3, 0x403d91)]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[9];
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(9, source.MakeEvents(buffer, uint.MaxValue));
            Assert.Equal(new uint[] { 2, 0, 0x403c90, 1, 0, 0x403d91, 2, 0, 0x02000000 }, buffer);
            Assert.False(source.CheckDone());
        }
        source.Restart();
        Assert.Equal(9, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x403c90, 1, 0, 0x403d91, 2, 0, 0x02000000 }, buffer);
        source.StartPlayback(false, 1);
        Assert.Equal(9, source.MakeEvents(new uint[12], uint.MaxValue));
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void FiniteGlobalLoopsRestoreRemainingDelayAndReviveFinishedTracks()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(4, 0x0176b0), new(3, 0x403c90), new(5, 0x7f77b0), new(2, 0x403e90)],
            [new(1, 0x403d91), new(9, 0x403f91)]));
        source.StartPlayback(false, 0);
        uint[] buffer = new uint[30];
        Assert.Equal(27, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 1, 0, 0x403d91, 3, 0, 0x02000000, 3, 0, 0x403c90,
            3, 0, 0x403f91, 2, 0, 0x02000000, 3, 0, 0x403c90, 3, 0, 0x403f91,
            2, 0, 0x02000000, 2, 0, 0x403e90 }, buffer[..27]);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void TracksFinishedBeforeGlobalBeginStayFinishedAcrossRewinds()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(4, 0x0176b0), new(3, 0x403c90), new(5, 0x7f77b0)],
            [new(1, 0x403d91)]));
        source.StartPlayback(false, 0);
        uint[] buffer = new uint[30];
        var written = source.MakeEvents(buffer, uint.MaxValue);
        Assert.Equal(18, written);
        Assert.Equal(1, Enumerable.Range(0, written / 3).Count(i => buffer[i * 3 + 2] == 0x403d91));
        Assert.Equal(2, Enumerable.Range(0, written / 3).Count(i => buffer[i * 3 + 2] == 0x403c90));
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 3)]
    public void GlobalRewindsPreserveCapabilityDesignationAndProgramOverride(int technology, int expected)
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(0, 0x0076b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [new(0, 0x076eb1), new(0, 0x0970b1), new(0, 0x000ac1), new(3, 0x403d91)]));
        source.CheckCapabilities(technology);
        source.StartPlayback(false, 3);
        uint[] buffer = new uint[60];
        var written = source.MakeEvents(buffer, uint.MaxValue);
        var words = Enumerable.Range(0, written / 3).Select(i => buffer[i * 3 + 2]).ToArray();
        Assert.Equal(3, words.Count(word => word == 0x403c90));
        Assert.Equal(expected, words.Count(word => word == 0x403d91));
        Assert.Equal(expected, words.Count(word => word == 0x0009c1));
        Assert.DoesNotContain(0x000ac1u, words);
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void GlobalInfiniteLoopExportHonorsLimitAfterLivePlayback(int limit)
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(0, 0x0076b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [new(3, 0x403d91)]));
        source.StartPlayback(true, 0);
        source.MakeEvents(new uint[3], 0);
        var bytes = VmMusicSmfSource.Create(source, limit);
        var decoded = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(bytes).Tracks[0].Span);
        Assert.Equal(limit, decoded.Events.Count(item => item.Event == 0x403c90));
        Assert.Equal(limit, decoded.Events.Count(item => item.Event == 0x403d91));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void SilentGlobalLoopErrorPersistsAndFinitePlaybackRecovers()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(1,
            [new(0, 0x0076b0), new(0, 0x7f77b0)], []));
        source.StartPlayback(true, 0);
        var error = Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], 0));
        Assert.Same(error, Assert.Throws<InvalidDataException>(() => source.CheckDone()));
        source.StartPlayback(false, 1);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void LiveFormatTwoGlobalLoopKeepsLaterTracksPendingAndRestarts()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [new(0, 0x0076b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [new(1, 0x403d91)]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[6];
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
            Assert.Equal(new uint[] { 2, 0, 0x403c90, 3, 0, 0x02000000 }, buffer);
            Assert.False(source.CheckDone());
        }
        source.Restart();
        Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x403c90, 3, 0, 0x02000000 }, buffer);
        source.StartPlayback(false, 1);
        uint[] finite = new uint[12];
        Assert.Equal(9, source.MakeEvents(finite, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x403c90, 3, 0, 0x02000000, 1, 0, 0x403d91 }, finite[..9]);
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void FormatTwoGlobalExportRepeatsCurrentTrackAndPreservesLaterDelay(int limit)
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [new(0, 0x0076b0), new(20, 0x403c90), new(30, 0x7f77b0)],
            [new(1, 0x403d91)]));
        var bytes = VmMusicSmfSource.Create(source, limit);
        var decoded = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(bytes).Tracks[0].Span);
        Assert.Equal(limit, decoded.Events.Count(item => item.Event == 0x403c90));
        var later = Assert.Single(decoded.Events, item => item.Event == 0x403d91);
        // Export folds the consumed loop-end controller's delay into the next event.
        Assert.Equal(31u, later.Delay);
    }

    [Fact]
    public void FormatTwoGlobalLoopInLaterTrackDoesNotReplayEarlierOrEmptyTracks()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [], [new(1, 0x403d91)], [],
            [new(0, 0x0176b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [], [new(1, 0x403e92)]));
        source.StartPlayback(false, 0);
        uint[] buffer = new uint[21];
        Assert.Equal(18, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 1, 0, 0x403d91, 2, 0, 0x403c90, 3, 0, 0x02000000,
            2, 0, 0x403c90, 3, 0, 0x02000000, 1, 0, 0x403e92 }, buffer[..18]);
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 1)]
    public void FormatTwoFutureTrackRetainsInitBeatAfterLongGlobalLoop(int technology, int expected)
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [new(0, 0x0176b0), new(1000, 0x403c90), new(1000, 0x7f77b0)],
            [new(0, 0x076eb1), new(0, 0x0970b1), new(1, 0x403d91)]));
        source.CheckCapabilities(technology);
        source.StartPlayback(false, 0);
        uint[] buffer = new uint[24];
        var written = source.MakeEvents(buffer, uint.MaxValue);
        var words = Enumerable.Range(0, written / 3).Select(i => buffer[i * 3 + 2]).ToArray();
        Assert.Equal(2, words.Count(word => word == 0x403c90));
        Assert.Equal(expected, words.Count(word => word == 0x403d91));
        Assert.Equal(expected, words.Count(word => word == 0x0009c1));
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void FormatTwoExportAfterLiveGlobalLoopUsesRequestedLimit()
    {
        using var source = new VmMusicSmfPlaybackSource(Multitrack(2,
            [new(0, 0x0076b0), new(2, 0x403c90), new(3, 0x7f77b0)],
            [new(1, 0x403d91)]));
        source.StartPlayback(true, 0);
        source.MakeEvents(new uint[6], uint.MaxValue);
        var bytes = VmMusicSmfSource.Create(source, 3);
        var decoded = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(bytes).Tracks[0].Span);
        Assert.Equal(3, decoded.Events.Count(item => item.Event == 0x403c90));
        Assert.Single(decoded.Events, item => item.Event == 0x403d91);
        Assert.Equal(0, source.LoopLimit);
    }

    private static byte[] Multitrack(ushort format, params VmMusicSmfEvent[][] tracks)
    {
        using var stream = new MemoryStream();
        var header = VmMusicSmf.Serialize(480, 500000, [])[..14];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(8), format);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(10), (ushort)tracks.Length);
        stream.Write(header);
        foreach (var track in tracks) stream.Write(VmMusicSmf.Serialize(480, 500000, track).AsSpan(14));
        return stream.ToArray();
    }

    [Theory]
    [InlineData(116, 117)]
    [InlineData(118, 119)]
    public void LiveSingleTrackInfiniteLoopsProduceBoundedBatches(int begin, int end)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, (uint)(0xb0 | begin << 8)), new(2, 0x403c90), new(3, (uint)(0x7f00b0 | end << 8))]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[6];
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
            Assert.Equal(new uint[] { 2, 0, 0x403c90, 3, 0, 0x02000000 }, buffer);
            Assert.False(source.CheckDone());
        }
        source.Restart();
        Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
    }

    [Fact]
    public void FiniteExportAfterLiveEmidiUsesRequestedLoopLimit()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0074b0), new(2, 0x403c90), new(0, 0x7f75b0)]));
        source.StartPlayback(true, 0);
        source.MakeEvents(new uint[3], 0);
        Assert.Equal(VmMusicSmf.Serialize(480, 500000, Enumerable.Repeat(new VmMusicSmfEvent(2, 0x403c90), 3)),
            VmMusicSmfSource.Create(source, 3));
    }

    [Fact]
    public void ControlOnlyLiveLoopFaultPersistsUntilRestart()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0074b0), new(0, 0x7f75b0)]));
        source.StartPlayback(true, 0);
        var error = Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], 0));
        Assert.Same(error, Assert.Throws<InvalidDataException>(() => source.CheckDone()));
        source.StartPlayback(false, 1);
        Assert.True(source.CheckDone());
    }
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(5, 0, 5)]
    [InlineData(0, 1, 1)]
    [InlineData(5, 1, 1)]
    [InlineData(0, 30, 30)]
    [InlineData(5, 30, 5)]
    public void LoopCountClampingMatchesNativeXmidiConvention(int count, int limit, int expected)
    {
        Assert.Equal(expected, VmMusicEmidi.ClampLoopCount(count, limit));
    }

    [Theory]
    [InlineData(116)]
    [InlineData(117)]
    [InlineData(118)]
    [InlineData(119)]
    public void SinglePassConsumesEveryLoopControlWithoutLosingDelay(byte controller)
    {
        var input = new VmMusicSmfTrack(500000, [new(7, (uint)(0xb0 | (controller << 8) | (127 << 16)))]);
        Assert.Equal(new VmMusicSmfEvent[] { new(7, 0x02000000) }, VmMusicEmidi.Apply(input, 480, loopLimit: 1).Events);
    }

    [Fact]
    public void ExportSingleTrackGlobalLoopsWithDifferentLimits()
    {
        var data = VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0076b0), new(0, 0x403c90), new(5, 0x7f77b0), new(3, 0x403d90)]);
        using var source = new VmMusicSmfPlaybackSource(data);
        var expected = VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x403c90), new(5, 0x02000000), new(3, 0x403d90)]);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
        var repeated = VmMusicSmfSource.Create(source, 3);
        var decoded = VmMusicSmfTrackDecoder.Decode(VmMusicSmfDocument.Parse(repeated).Tracks[0].Span);
        Assert.Equal(3, decoded.Events.Count(x => x.Event == 0x403c90));
        Assert.Equal(0, source.LoopLimit);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void FileExportCommandUsesSinglePassThroughConcreteFactory()
    {
        var input = VmMusicSmf.Serialize(480, 500000, [new(0, 0x0076b0), new(0, 0x403c90), new(5, 0x7f77b0)]);
        var path = Path.Combine(Path.GetTempPath(), "hcde-loop-" + Guid.NewGuid().ToString("N") + ".mid");
        var factory = new VmMusicMidiSourceFactory();
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "song", path], _ => factory.Create(input, VmMusicMidiType.Midi),
                () => factory.LastError, _ => throw new Exception("unexpected report"));
            Assert.Equal(VmMusicSmf.Serialize(480, 500000, [new(0, 0x403c90), new(5, 0x02000000)]), File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }
}
