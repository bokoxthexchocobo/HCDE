namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumActorDeltaPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoAppliedDeltas()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumActorDeltaPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void MixShippedActorDeltas_UpdatesRollingHashOnShip()
    {
        var store = new GuestWorldStateStore();
        store.SeedActor(actorId: 9, classId: 3, health: 40);
        var actorDeltas = new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 9,
                ClassId = 3,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 40,
            },
        };

        store.MixShippedActorDeltas(actorDeltas);

        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
        Assert.Equal(store.ActorDeltaRollingHash, SnapshotChecksumActorDeltaPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void CommitAppliedActorDeltas_UpdatesRollingHashOnGuestApply()
    {
        var store = new GuestWorldStateStore();
        var record = new ActorDeltaRecord
        {
            ActorId = 21,
            ClassId = 5,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 88,
        };

        store.CommitAppliedActorDeltas(new[] { record });

        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
        Assert.Equal(store.ActorDeltaRollingHash, SnapshotChecksumActorDeltaPolicy.ComputeRollingHash(store));
    }
}
