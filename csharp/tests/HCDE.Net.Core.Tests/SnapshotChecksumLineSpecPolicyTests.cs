namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumLineSpecPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoLineSpecs()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumLineSpecPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void NoteLineSpec_UpdatesRollingHashOnGuestStore()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 9, special: 11, success: true);

        Assert.NotEqual(0u, store.LineSpecRollingHash);
        Assert.Equal(store.LineSpecRollingHash, SnapshotChecksumLineSpecPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void MixRecord_ProducesStableHashForSameInputs()
    {
        var first = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 4, special: 7, success: false);
        var second = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 4, special: 7, success: false);
        Assert.Equal(first, second);
        Assert.NotEqual(0u, first);
    }

    [Fact]
    public void PolishRollingHash_MixesPresentationEchoTailWhenBothPresent()
    {
        var lineSpecHash = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 2, special: 5, success: true);
        var polished = SnapshotChecksumLineSpecPolicy.PolishRollingHash(lineSpecHash, presentationEchoHash: 0xA5A5A5A5u);

        Assert.NotEqual(lineSpecHash, polished);
        Assert.Equal(lineSpecHash, SnapshotChecksumLineSpecPolicy.PolishRollingHash(lineSpecHash, presentationEchoHash: 0));
    }

    [Fact]
    public void CommitAppliedPresentationEcho_PolishesLineSpecRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 3, special: 8, success: true);
        var before = store.LineSpecRollingHash;

        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }

    [Fact]
    public void NoteLineSpec_PolishesPresentationEchoRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.PresentationEchoRollingHash;

        store.NoteLineSpec(lineIndex: 3, special: 8, success: true);

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }

    [Fact]
    public void PolishRollingHashWithActorDelta_MixesActorDeltaTailWhenBothPresent()
    {
        var lineSpecHash = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 6, special: 9, success: true);
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 3,
            ClassId = 1,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 40,
        });
        var polished = SnapshotChecksumLineSpecPolicy.PolishRollingHashWithActorDelta(lineSpecHash, actorHash);

        Assert.NotEqual(lineSpecHash, polished);
        Assert.Equal(lineSpecHash, SnapshotChecksumLineSpecPolicy.PolishRollingHashWithActorDelta(lineSpecHash, actorDeltaHash: 0));
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesLineSpecRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 5, special: 6, success: true);
        var before = store.LineSpecRollingHash;

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

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesLineSpecRollingHashFromAuthorityEvent()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 5, special: 6, success: true);
        var before = store.LineSpecRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }

    [Fact]
    public void PolishActorDeltaRollingHash_MixesLineSpecTailWhenBothPresent()
    {
        var lineSpecHash = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 6, special: 9, success: true);
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 5,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 50,
        });
        var polished = SnapshotChecksumLineSpecPolicy.PolishActorDeltaRollingHash(actorHash, lineSpecHash);

        Assert.NotEqual(actorHash, polished);
        Assert.Equal(actorHash, SnapshotChecksumLineSpecPolicy.PolishActorDeltaRollingHash(actorHash, lineSpecHash: 0));
    }

    [Fact]
    public void NoteLineSpec_PolishesActorDeltaRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 11,
                ClassId = 4,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 65,
            },
        });
        var before = store.ActorDeltaRollingHash;

        store.NoteLineSpec(lineIndex: 2, special: 4, success: true);

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }
}
