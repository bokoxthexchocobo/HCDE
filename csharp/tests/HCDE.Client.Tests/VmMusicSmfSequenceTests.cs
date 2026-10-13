using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicSmfSequenceTests
{
    [Fact]
    public void TracksPlayInSequenceWithEndAndStartDelaysPreserved()
    {
        var sequence = VmMusicSmfMerge.Sequence([
            new(1000000, [new(20, 0x403c90), new(7, 0x02000000)], true),
            new(500000, []),
            new(250000, [new(3, 0x403d91), new(0, 0x403e91)], true)]);
        Assert.Equal(250000u, sequence.InitialTempo);
        Assert.Equal(new uint[] { 20, 7, 3, 0 }, sequence.Events.Select(x => x.Delay));
        Assert.Equal(new uint[] { 0x403c90, 0x02000000, 0x403d91, 0x403e91 }, sequence.Events.Select(x => x.Event));
    }

    [Fact]
    public void FutureTrackDelayDoesNotUnderflowWhenEarlierTrackRunsLonger()
    {
        var sequence = VmMusicSmfMerge.Sequence([
            new(500000, [new(1000, 0x403c90)]), new(500000, [new(1, 0x403d91)])]);
        Assert.Equal(new uint[] { 1000, 1 }, sequence.Events.Select(x => x.Delay));
        Assert.Equal(500000u, sequence.InitialTempo);
        Assert.False(sequence.HasInitialTempo);
    }

    [Fact]
    public void EmptySequenceAndNullTracksAreHandledExplicitly()
    {
        Assert.Empty(VmMusicSmfMerge.Sequence([]).Events);
        Assert.Throws<ArgumentNullException>(() => VmMusicSmfMerge.Sequence(new VmMusicSmfTrack[] { null! }));
    }

    [Fact]
    public void FormatTwoResourceExportWritesSequentialTracksToDisk()
    {
        var first = VmMusicSmf.Serialize(480, 1000000, [new(20, 0x403c90), new(5, 0x02000000)])[22..];
        var second = VmMusicSmf.Serialize(480, 500000, [new(1, 0x403d91), new(2, 0x0103d090)])[22..];
        var data = new byte[14 + 16 + first.Length + second.Length];
        VmMusicSmf.Serialize(480, 500000, []).AsSpan(0, 14).CopyTo(data);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(8), 2);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(10), 2);
        var offset = 14;
        foreach (var track in new[] { first, second })
        {
            "MTrk"u8.CopyTo(data.AsSpan(offset));
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset + 4), (uint)track.Length);
            track.CopyTo(data, offset + 8); offset += 8 + track.Length;
        }
        var path = Path.Combine(Path.GetTempPath(), "hcde-sequence-" + Guid.NewGuid().ToString("N") + ".mid");
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "song", path], name =>
                VmMusicMidiSource.OpenManaged(name, "", _ => 0, _ => -1, _ => data,
                    _ => throw new Exception("unexpected report")), () => "unused", _ => throw new Exception("unexpected report"));
            Assert.Equal(VmMusicSmf.Serialize(480, 500000,
                [new(20, 0x403c90), new(5, 0x02000000), new(1, 0x403d91), new(2, 0x0103d090)]), File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }
}
