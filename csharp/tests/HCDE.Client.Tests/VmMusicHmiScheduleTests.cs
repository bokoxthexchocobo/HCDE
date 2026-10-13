using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicHmiScheduleTests
{
    private static VmMusicHmiDocument Document(params (ushort Device, byte[] Events)[] tracks)
    {
        var bytes = new byte[0x100 + tracks.Sum(x => 0xa9 + x.Events.Length)];
        "HMI-MIDISONG061595\0"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xd4), 30);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xe4), (ushort)tracks.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xe8), 0xec);
        var offset = 0x100;
        for (var i = 0; i < tracks.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xec + i * 4), (uint)offset);
            "HMI-MIDITRACK"u8.CopyTo(bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 0x57), 0xa9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset + 0x99), tracks[i].Device);
            tracks[i].Events.CopyTo(bytes, offset + 0xa9);
            offset += 0xa9 + tracks[i].Events.Length;
        }
        return VmMusicHmiDocument.Parse(bytes);
    }

    [Fact]
    public void CurrentRealTrackContinuesBeforeZeroDurationOffThenOffWinsOtherTrackTie()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa000, [0, 0x90, 60, 100, 0, 0, 0xc0, 1]),
            (0xa000, [0, 0xc1, 2])));
        Assert.Equal(new uint[] { 0x643c90, 0x01c0, 0x3c90, 0x02c1 }, track.Events.Select(x => x.Event));
    }

    [Fact]
    public void OffWinsFutureRealTieAndGlobalTrackOrderStaysStable()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa000, [0, 0x90, 60, 100, 5, 10, 0xc0, 1]),
            (0xa000, [5, 0xc1, 2, 0, 0xc1, 3])));
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(5, 0x3c90), new(0, 0x02c1),
            new(0, 0x03c1), new(5, 0x01c0) }, track.Events);
    }

    [Fact]
    public void ZeroDelayIgnoredMetaKeepsCurrentTrackPrecedence()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa000, [0, 0x90, 60, 100, 0,
            0, 0xff, 1, 0, 0, 0xc0, 1])));
        Assert.Equal(new uint[] { 0x643c90, 0x01c0, 0x3c90 }, track.Events.Select(x => x.Event));
    }

    [Fact]
    public void DisabledTracksEmitNoNotesButLastLeadingTempoStillWins()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa000, [0, 0xc0, 1]),
            (0xa002, [0, 0xff, 81, 3, 7, 0xa1, 0x20, 0, 0xc1, 2])));
        Assert.Equal(500000u, track.InitialTempo);
        Assert.True(track.HasInitialTempo);
        Assert.Equal(0x01c0u, Assert.Single(track.Events).Event);
    }

    [Fact]
    public void DisabledFirstTrackDoesNotLeakZeroDelayEvent()
    {
        var document = Document((0xa002, [0, 0xc0, 1]), (0xa000, [0, 0xc1, 2]));
        Assert.Equal(0x02c1u, Assert.Single(VmMusicHmiSchedule.Decode(document).Events).Event);
        Assert.Equal(0x01c0u, Assert.Single(VmMusicHmiSchedule.Decode(document, 4).Events).Event);
    }

    [Fact]
    public void OffsDrainAfterAllTracksEndAndExportRetainsTiming()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa000, [0, 0x90, 60, 100, 8, 0, 0xff, 47, 0]),
            (0xa000, [2, 0x91, 61, 100, 3, 0, 0xff, 47, 0])));
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(2, 0x643d91),
            new(3, 0x3d91), new(3, 0x3c90) }, track.Events);
        Assert.NotEmpty(VmMusicSmf.Serialize(120, track.InitialTempo, track.Events));
    }

    [Fact]
    public void SingleTrackDocumentAndStandaloneDecodeAgreeOnTies()
    {
        byte[] bytes = [0, 0x90, 60, 100, 0, 0, 0xff, 1, 0, 0, 0xc0, 7, 0, 0xff, 47, 0];
        var standalone = VmMusicHmiEvents.Decode(bytes);
        var scheduled = VmMusicHmiSchedule.Decode(Document((0xa000, bytes)));
        Assert.Equal(standalone.InitialTempo, scheduled.InitialTempo);
        Assert.Equal(standalone.Events, scheduled.Events);
    }

    [Fact]
    public void GlobalQueuePreservesNativeHeapTieOrderAcrossTracks()
    {
        var track = VmMusicHmiSchedule.Decode(Document(
            (0xa000, [0, 0x90, 60, 100, 10, 0, 0x90, 61, 100, 10]),
            (0xa000, [0, 0x91, 62, 100, 10, 0, 0x91, 63, 100, 10])));
        Assert.Equal(new uint[] { 0x3c90, 0x3f91, 0x3e91, 0x3d90 }, track.Events.Skip(4).Select(x => x.Event));
    }

    [Fact]
    public void NoEnabledTracksProducesEmptySongWithDefaultTempo()
    {
        var track = VmMusicHmiSchedule.Decode(Document((0xa004, [0, 0xc0, 1])));
        Assert.Empty(track.Events);
        Assert.Equal(4000000u, track.InitialTempo);
    }
}
