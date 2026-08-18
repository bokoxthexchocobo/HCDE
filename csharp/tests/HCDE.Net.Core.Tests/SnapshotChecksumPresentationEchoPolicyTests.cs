namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumPresentationEchoPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoAppliedEcho()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumPresentationEchoPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void CommitAppliedPresentationEcho_UpdatesRollingHashOnGuestApply()
    {
        var store = new GuestWorldStateStore();
        var block = PresentationEchoCodec.CreateExampleBlock();

        store.CommitAppliedPresentationEcho(block);

        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
        Assert.Equal(store.PresentationEchoRollingHash, SnapshotChecksumPresentationEchoPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void MixBlock_ProducesStableHashForSameBlock()
    {
        var block = PresentationEchoCodec.CreateExampleBlock();
        var first = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, block);
        var second = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, block);
        Assert.Equal(first, second);
        Assert.NotEqual(0u, first);
    }

    [Fact]
    public void PolishRollingHash_MixesLineSpecTailWhenBothPresent()
    {
        var echoHash = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, PresentationEchoCodec.CreateExampleBlock());
        var polished = SnapshotChecksumPresentationEchoPolicy.PolishRollingHash(echoHash, lineSpecHash: 0x5A5A5A5Au);

        Assert.NotEqual(echoHash, polished);
        Assert.Equal(echoHash, SnapshotChecksumPresentationEchoPolicy.PolishRollingHash(echoHash, lineSpecHash: 0));
    }

    [Fact]
    public void PolishActorDeltaRollingHash_MixesPresentationEchoTailWhenBothPresent()
    {
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 3,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 40,
        });
        var echoHash = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, PresentationEchoCodec.CreateExampleBlock());
        var polished = SnapshotChecksumPresentationEchoPolicy.PolishActorDeltaRollingHash(actorHash, echoHash);

        Assert.NotEqual(actorHash, polished);
        Assert.Equal(actorHash, SnapshotChecksumPresentationEchoPolicy.PolishActorDeltaRollingHash(actorHash, presentationEchoHash: 0));
    }

    [Fact]
    public void CommitAppliedPresentationEcho_PolishesActorDeltaRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 8,
                ClassId = 2,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 70,
            },
        });
        var before = store.ActorDeltaRollingHash;

        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }
}
