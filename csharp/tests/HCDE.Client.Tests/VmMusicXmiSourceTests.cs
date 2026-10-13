using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client.Tests;

public class VmMusicXmiSourceTests
{
    [Fact]
    public void LiveDecoderFailureRemainsFailureUntilRestart()
    {
        using var source = new VmMusicXmiSource(Document([0xb0, 116, 0, 0xb0, 117, 127]));
        source.StartPlayback(true, 0);
        var original = Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], 0));
        Assert.Same(original, Assert.Throws<InvalidDataException>(() => source.CheckDone()));
        Assert.Same(original, Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], 0)));
        source.Restart();
        Assert.NotSame(original, Assert.Throws<InvalidDataException>(() => source.CheckDone()));
        source.StartPlayback(false, 0);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void LiveSysexCapacityFailureCanRetryWithoutConsumingPendingPacket()
    {
        using var source = new VmMusicXmiSource(Document([2, 0xf0, 3, 1, 2, 0xf7, 3, 0xc0, 7]));
        source.StartPlayback(true, 0);
        Assert.False(source.CheckDone());
        Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], 0));
        uint[] packet = new uint[4];
        Assert.Equal(4, source.MakeEvents(packet, 0));
        Assert.Equal(new uint[] { 2, 0, 0x80000004, 0xf70201f0 }, packet);
        uint[] following = new uint[3];
        Assert.Equal(3, source.MakeEvents(following, 0));
        Assert.Equal(new uint[] { 3, 0, 0x07c0 }, following);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void LiveSysexSuppressionAndTimeBudgetPreserveFollowingDelays()
    {
        using var source = new VmMusicXmiSource(Document([2, 0xf0, 3, 1, 2, 0xf7, 3, 0xc0, 7]));
        source.StartPlayback(true, 0);
        source.SkipSysex = true;
        uint[] packet = new uint[12];
        Assert.Equal(3, source.MakeEvents(packet, 0));
        Assert.Equal(new uint[] { 2, 0, 0x02000000 }, packet.Take(3));
        Assert.Equal(3, source.MakeEvents(packet, 0));
        Assert.Equal(new uint[] { 3, 0, 0x07c0 }, packet.Take(3));
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void LiveSubsongSelectionClearsPendingEventAndRestartsSelectedSong()
    {
        using var source = new VmMusicXmiSource(Document([0xc0, 1], [0xc0, 2]));
        source.StartPlayback(true, 0);
        Assert.False(source.CheckDone());
        Assert.True(source.SetSubsong(1));
        uint[] buffer = new uint[3];
        Assert.Equal(3, source.MakeEvents(buffer, 0));
        Assert.Equal(0x02c0u, buffer[2]);
        Assert.True(source.CheckDone());
    }
    [Fact]
    public void LiveInfiniteLoopProducesBoundedBatchesAndRestartResetsCursor()
    {
        using var source = new VmMusicXmiSource(Document([0xb0, 116, 0, 2, 0xc0, 5, 3, 0xb0, 117, 127]));
        source.StartPlayback(true, 0);
        uint[] buffer = new uint[6];
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
            Assert.Equal(new uint[] { 2, 0, 0x05c0, 3, 0, 0x02000000 }, buffer);
            Assert.False(source.CheckDone());
        }
        source.Restart();
        Assert.Equal(3, source.MakeEvents(new uint[3], 0));
    }

    [Fact]
    public void LiveFiniteLoopsMatchEagerExportIncludingPendingNoteOffs()
    {
        byte[] song = [0xb0, 116, 2, 0x90, 60, 100, 8, 3, 0xb0, 117, 127];
        using var source = new VmMusicXmiSource(Document(song));
        source.StartPlayback(true, 0);
        var actual = new List<VmMusicSmfEvent>();
        uint[] buffer = new uint[3];
        while (!source.CheckDone())
        {
            Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
            actual.Add(new(buffer[0], buffer[2]));
        }
        Assert.Equal(VmMusicXmiEvents.Decode(song).Events, actual);
    }

    [Fact]
    public void LiveControlOnlyInfiniteLoopFailsInsteadOfHanging()
    {
        using var source = new VmMusicXmiSource(Document([0xb0, 116, 0, 0xb0, 117, 127]));
        source.StartPlayback(true, 0);
        Assert.Throws<InvalidDataException>(() => source.MakeEvents(new uint[3], uint.MaxValue));
    }

    [Fact]
    public void ExportAfterLiveLoopIsFiniteAndCanReturnToLivePlayback()
    {
        using var source = new VmMusicXmiSource(Document([0xb0, 116, 0, 1, 0xc0, 7, 0xb0, 117, 127]));
        source.StartPlayback(true, 0);
        source.MakeEvents(new uint[3], 0);
        Assert.Equal(VmMusicSmf.Serialize(60, 500000, Enumerable.Repeat(new VmMusicSmfEvent(1, 0x07c0), 3)),
            VmMusicSmfSource.Create(source, 3));
        source.StartPlayback(true, 0);
        Assert.Equal(3, source.MakeEvents(new uint[3], 0));
        Assert.False(source.CheckDone());
    }
    private static byte[] Chunk(string id, byte[] payload)
    {
        var bytes = new byte[8 + payload.Length + (payload.Length & 1)];
        Encoding.ASCII.GetBytes(id).CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), (uint)payload.Length);
        payload.CopyTo(bytes, 8);
        return bytes;
    }
    private static byte[] Document(params byte[][] songs) => Chunk("CAT ",
        "XMID"u8.ToArray().Concat(songs.SelectMany(song => Chunk("FORM",
            "XMID"u8.ToArray().Concat(Chunk("EVNT", song)).ToArray()))).ToArray());

    [Fact]
    public void FactoryOwnsSnapshotAndExportsNotesWithDurations()
    {
        var bytes = Document([0x90, 60, 100, 4, 0xff, 47, 0]);
        var factory = new VmMusicMidiSourceFactory();
        using var source = Assert.IsType<VmMusicXmiSource>(factory.Create(bytes, VmMusicMidiType.Xmi));
        Array.Clear(bytes);
        Assert.Equal("", factory.LastError);
        Assert.Equal(VmMusicSmf.Serialize(60, 500000, [new(0, 0x643c90), new(4, 0x3c90)]),
            VmMusicSmfSource.Create(source, 1));
        Assert.True(source.CheckDone());
        source.Restart();
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void SongSelectionAndInvalidSelectionPreserveExpectedState()
    {
        using var source = new VmMusicXmiSource(Document([0xc0, 1], [0xc0, 2]));
        Assert.Equal(2, source.SongCount);
        Assert.True(source.SetSubsong(1));
        Assert.False(source.SetSubsong(-1));
        Assert.False(source.SetSubsong(2));
        Assert.Equal(1, source.CurrentSong);
        Assert.Equal(VmMusicSmf.Serialize(60, 500000, [new(0, 0x02c0)]), VmMusicSmfSource.Create(source, 1));
        Assert.True(source.SetSubsong(0));
        Assert.Equal(VmMusicSmf.Serialize(60, 500000, [new(0, 0x01c0)]), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void ChangingExportLimitRecompilesLoopsAndPrecacheRewinds()
    {
        using var source = new VmMusicXmiSource(Document([0xb0, 116, 0, 0xc0, 7, 0xb0, 117, 127]));
        foreach (var limit in new[] { 1, 3, 2 })
            Assert.Equal(VmMusicSmf.Serialize(60, 500000, Enumerable.Repeat(new VmMusicSmfEvent(0, 0x07c0), limit)),
                VmMusicSmfSource.Create(source, limit));
        Assert.Equal(new ushort[] { 7 }, VmMusicMidiPrecache.Collect(source));
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void SmallBatchesPreserveSysexAndFollowingEvents()
    {
        using var source = new VmMusicXmiSource(Document([0xc0, 1, 0xf0, 2, 1, 0xf7, 0xc0, 2]));
        uint[] buffer = new uint[4];
        Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(0x01c0u, buffer[2]);
        Assert.Equal(4, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(0x80000003u, buffer[2]);
        Assert.Equal(0x00f701f0u, buffer[3]);
        Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(0x02c0u, buffer[2]);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void TimeBudgetAndOversizedPacketAreExplicit()
    {
        using var source = new VmMusicXmiSource(Document([1, 0xc0, 1, 1, 0xc0, 2]));
        Assert.Equal(3, source.MakeEvents(new uint[9], 0));
        Assert.Equal(3, source.MakeEvents(new uint[9], 0));
        Assert.True(source.CheckDone());
        using var sysex = new VmMusicXmiSource(Document([0xf0, 2, 1, 0xf7]));
        Assert.Throws<InvalidDataException>(() => sysex.MakeEvents(new uint[3], 100));
        Assert.False(sysex.CheckDone());
    }

    [Fact]
    public void InvalidSongSelectionDecodingIsAtomic()
    {
        using var source = new VmMusicXmiSource(Document([0xc0, 1], [0x90, 60]));
        Assert.Throws<InvalidDataException>(() => source.SetSubsong(1));
        Assert.Equal(0, source.CurrentSong);
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void FactoryReportsMalformedXmiAndClearsErrorOnSuccess()
    {
        var factory = new VmMusicMidiSourceFactory();
        Assert.Null(factory.Create(Document([0x90, 60]), VmMusicMidiType.Xmi));
        Assert.Equal("Truncated XMI event.", factory.LastError);
        using var source = factory.Create(Document(Array.Empty<byte>()), VmMusicMidiType.Xmi);
        Assert.NotNull(source);
        Assert.Equal("", factory.LastError);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void TimbreChunkDoesNotOverrideNativeEventBasedPrecache()
    {
        var bytes = Chunk("FORM", "XMID"u8.ToArray()
            .Concat(Chunk("TIMB", [1, 0, 99, 5]))
            .Concat(Chunk("EVNT", [0xc0, 7, 0xf2, 1, 2])).ToArray());
        using var source = new VmMusicXmiSource(bytes);
        Assert.Equal(new ushort[] { 7 }, VmMusicMidiPrecache.Collect(source));
        Assert.False(source.CheckDone());
        Assert.Equal(VmMusicSmf.Serialize(60, 500000, VmMusicXmiEvents.Decode([0xc0, 7, 0xf2, 1, 2]).Events),
            VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void NegativeLimitsAndDisposedOperationsFail()
    {
        var source = new VmMusicXmiSource(Document([0xc0, 1]));
        source.StartPlayback(true, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.StartPlayback(false, -1));
        source.Dispose();
        Assert.Throws<ObjectDisposedException>(() => source.Restart());
        Assert.Throws<ObjectDisposedException>(() => source.SetSubsong(0));
        Assert.Throws<ObjectDisposedException>(() => source.CheckDone());
        Assert.Throws<ObjectDisposedException>(() => source.CheckCapabilities(1));
        Assert.Throws<ObjectDisposedException>(() => source.MakeEvents(new uint[3], 0));
    }
}
