namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumAuthorityEventPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoAppliedEvents()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void TakePendingAuthorityEventsForTail_UpdatesRollingHashOnShip()
    {
        var store = new GuestWorldStateStore();
        store.QueueAuthorityEvent(AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 12));

        _ = store.TakePendingAuthorityEventsForTail();

        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
        Assert.Equal(store.AuthorityEventRollingHash, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_UpdatesRollingHashOnGuestApply()
    {
        var store = new GuestWorldStateStore();
        var record = AuthorityEventsCodec.CreateSpawnExample("ZombieMan", actorId: 33);

        store.CommitAppliedAuthorityEvents(new[] { record });

        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
        Assert.Equal(store.AuthorityEventRollingHash, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void PolishActorDeltaRollingHash_MixesAuthorityEventTailWhenBothPresent()
    {
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 2,
            ClassId = 1,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 30,
        });
        var authorityHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(
            0,
            AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 12));
        var polished = SnapshotChecksumAuthorityEventPolicy.PolishActorDeltaRollingHash(actorHash, authorityHash);

        Assert.NotEqual(actorHash, polished);
        Assert.Equal(actorHash, SnapshotChecksumAuthorityEventPolicy.PolishActorDeltaRollingHash(actorHash, authorityEventHash: 0));
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesActorDeltaRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 9,
                ClassId = 3,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 45,
            },
        });
        var before = store.ActorDeltaRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }

    [Fact]
    public void PolishPresentationEchoRollingHash_MixesAuthorityEventTailWhenBothPresent()
    {
        var echoHash = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, PresentationEchoCodec.CreateExampleBlock());
        var authorityHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(
            0,
            AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 12));
        var polished = SnapshotChecksumAuthorityEventPolicy.PolishPresentationEchoRollingHash(echoHash, authorityHash);

        Assert.NotEqual(echoHash, polished);
        Assert.Equal(echoHash, SnapshotChecksumAuthorityEventPolicy.PolishPresentationEchoRollingHash(echoHash, authorityEventHash: 0));
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesPresentationEchoRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.PresentationEchoRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesActorDeltaRollingHashFromPresentationEcho()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 8,
                ClassId = 2,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 72,
            },
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.ActorDeltaRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesLineSpecRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 5, special: 6, success: true);
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 9,
                ClassId = 3,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 45,
            },
        });
        var before = store.LineSpecRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }
}
