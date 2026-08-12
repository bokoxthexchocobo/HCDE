using System.Text;

namespace HCDE.Net.Core.Tests;

public class AuthorityEventsCodecTests
{
    [Fact]
    public void SingleSpawnRecord_RoundTrip()
    {
        var record = AuthorityEventsCodec.CreateSpawnExample("ZombieMan", actorId: 99);
        Span<byte> chunk = stackalloc byte[256];
        var written = AuthorityEventsCodec.Write(chunk, new[] { record });
        Assert.True(written > LiveConstants.AuthorityEventsHeaderSize);

        Assert.True(AuthorityEventsCodec.TryRead(chunk[..written], out var header, out var records, out var consumed, out _));
        Assert.Equal(1, header.EventCount);
        Assert.Equal(written, consumed);
        Assert.Equal(record.ActorId, records[0].ActorId);
        Assert.Equal(AuthorityEventType.Spawn, records[0].EventType);
        Assert.Equal("ZombieMan", Encoding.UTF8.GetString(records[0].ClassName));
        Assert.Equal(record.PosX, records[0].PosX);
    }

    [Fact]
    public void CoopShippingTail_IncludesAuthorityEvents()
    {
        var authorityEvents = new[] { AuthorityEventsCodec.CreateSpawnExample("Imp") };
        Span<byte> tail = stackalloc byte[512];
        var written = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: 12,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: authorityEvents);

        Assert.True(written > ServerSnapshotTailCodec.MinimalTailSize);
        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..written], out var sections, out _, out _));
        Assert.NotNull(sections.AuthorityEvents);
        Assert.Equal(1, sections.AuthorityEvents.Value.EventCount);

        var tailSpan = tail[..written];
        var hcavIndex = -1;
        for (var i = 0; i <= tailSpan.Length - 4; i++)
        {
            if (!AuthorityEventsCodec.TryPeek(tailSpan[i..]))
                continue;

            hcavIndex = i;
            break;
        }

        Assert.True(hcavIndex >= 0);
        Assert.True(AuthorityEventsCodec.TryRead(tailSpan[hcavIndex..], out _, out var records, out _, out _));
        Assert.Equal("Imp", Encoding.UTF8.GetString(records[0].ClassName));
    }
}
