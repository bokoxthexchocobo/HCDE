using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicSmfMergeTests
{
    [Fact]
    public void LiveMultitrackPlaybackMatchesFiniteMergeAndTempoBudget()
    {
        var one = VmMusicSmf.Serialize(480, 1000000, [new(10, 0x403c90)])[22..];
        var two = VmMusicSmf.Serialize(480, 250000, [new(5, 0x403d91), new(0, 0x403e91)])[22..];
        using var stream = new MemoryStream();
        var header = VmMusicSmf.Serialize(480, 500000, [])[..14];
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(10), 2);
        stream.Write(header);
        foreach (var track in new[] { one, two })
        {
            stream.Write("MTrk"u8);
            var size = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(size, (uint)track.Length);
            stream.Write(size);
            stream.Write(track);
        }
        using var live = new VmMusicSmfPlaybackSource(stream.ToArray());
        using var finite = new VmMusicSmfPlaybackSource(stream.ToArray());
        live.StartPlayback(true, 0);
        finite.StartPlayback(false, 1);
        Assert.Equal(250000u, live.InitialTempo);
        uint[] actual = new uint[12], expected = new uint[12];
        Assert.Equal(3, live.MakeEvents(actual, 2000));
        Assert.Equal(3, finite.MakeEvents(expected, 2000));
        Assert.Equal(expected, actual);
        Assert.Equal(6, live.MakeEvents(actual, uint.MaxValue));
        Assert.Equal(6, finite.MakeEvents(expected, uint.MaxValue));
        Assert.Equal(expected, actual);
        Assert.True(live.CheckDone());
    }

    [Fact]
    public void MergeUsesAbsoluteTicksAndCurrentTrackPrecedenceAtTies()
    {
        var first = new VmMusicSmfTrack(500000, [new(10, 0x403c90), new(0, 0x403d90)]);
        var second = new VmMusicSmfTrack(500000, [new(5, 0x403e91), new(5, 0x403f91), new(0, 0x404091)]);
        var merged = VmMusicSmfMerge.Merge([first, second]);
        Assert.Equal(new uint[] { 5, 5, 0, 0, 0 }, merged.Events.Select(x => x.Delay));
        Assert.Equal(new uint[] { 0x403e91, 0x403c90, 0x403d90, 0x403f91, 0x404091 }, merged.Events.Select(x => x.Event));
    }

    [Fact]
    public void CurrentTrackRetainsPriorityWhenItsNextEventIsAtCurrentTick()
    {
        var merged = VmMusicSmfMerge.Merge([
            new(500000, [new(10, 0x403c90)]),
            new(500000, [new(5, 0x403d91), new(0, 0x403e91), new(5, 0x403f91)])]);
        Assert.Equal(new uint[] { 0x403d91, 0x403e91, 0x403c90, 0x403f91 }, merged.Events.Select(x => x.Event));
    }

    [Fact]
    public void LastExplicitInitialTempoWinsWhileDefaultTracksDoNotOverrideIt()
    {
        var merged = VmMusicSmfMerge.Merge([
            new(1000000, [], true), new(250000, [], true), new(500000, [])]);
        Assert.Equal(250000u, merged.InitialTempo);
        Assert.True(merged.HasInitialTempo);
        Assert.Empty(merged.Events);
        Assert.Equal(500000u, VmMusicSmfMerge.Merge([]).InitialTempo);
    }

    [Fact]
    public void LongestTrackEndDelayIsPreservedAndInputsRemainUnchanged()
    {
        var events = new VmMusicSmfEvent[] { new(20, 0x02000000) };
        var merged = VmMusicSmfMerge.Merge([new(500000, events), new(500000, [new(5, 0x403c90)])]);
        Assert.Equal(new uint[] { 5, 15 }, merged.Events.Select(x => x.Delay));
        Assert.Equal(20u, events[0].Delay);
    }

    [Fact]
    public void FormatOneFactoryExportMergesTracksAndTempoChanges()
    {
        var one = VmMusicSmf.Serialize(480, 1000000, [new(0, 0x403c90), new(10, 0x403d90)])[22..];
        var two = VmMusicSmf.Serialize(480, 500000, [new(5, 0x403e91), new(2, 0x0103d090)])[22..];
        var bytes = new byte[14 + 8 + one.Length + 8 + two.Length];
        VmMusicSmf.Serialize(480, 500000, []).AsSpan(0, 14).CopyTo(bytes);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), 2);
        var offset = 14;
        foreach (var track in new[] { one, two })
        {
            "MTrk"u8.CopyTo(bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset + 4), (uint)track.Length);
            track.CopyTo(bytes, offset + 8); offset += 8 + track.Length;
        }
        var factory = new VmMusicMidiSourceFactory();
        using var source = factory.Create(bytes, VmMusicMidiType.Midi);
        Assert.IsType<VmMusicSmfPlaybackSource>(source);
        Assert.Equal(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x403c90), new(5, 0x403e91), new(2, 0x0103d090), new(3, 0x403d90)]), VmMusicSmfSource.Create(source!, 1));
    }
}
