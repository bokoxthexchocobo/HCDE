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

    [Fact]
    public void PolishPresentationEchoRollingHash_MixesActorDeltaTailWhenBothPresent()
    {
        var echoHash = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, PresentationEchoCodec.CreateExampleBlock());
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 4,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 55,
        });
        var polished = SnapshotChecksumActorDeltaPolicy.PolishPresentationEchoRollingHash(echoHash, actorHash);

        Assert.NotEqual(echoHash, polished);
        Assert.Equal(echoHash, SnapshotChecksumActorDeltaPolicy.PolishPresentationEchoRollingHash(echoHash, actorDeltaHash: 0));
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesPresentationEchoRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.PresentationEchoRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 7,
                ClassId = 1,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 60,
            },
        });

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesPresentationEchoRollingHashFromLineSpec()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 2, special: 5, success: true);
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.PresentationEchoRollingHash;

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

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }

    [Fact]
    public void PolishAuthorityEventRollingHash_MixesActorDeltaTailWhenBothPresent()
    {
        var authorityHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(
            0,
            AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 12));
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 2,
            ClassId = 1,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 30,
        });
        var polished = SnapshotChecksumActorDeltaPolicy.PolishAuthorityEventRollingHash(authorityHash, actorHash);

        Assert.NotEqual(authorityHash, polished);
        Assert.Equal(authorityHash, SnapshotChecksumActorDeltaPolicy.PolishAuthorityEventRollingHash(authorityHash, actorDeltaHash: 0));
    }

    [Fact]
    public void PolishActorDeltaRollingHash_MixesAuthorityEventTailWhenBothPresent()
    {
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 7,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 30,
        });
        var authorityHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(0, AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19));
        var polished = SnapshotChecksumActorDeltaPolicy.PolishActorDeltaRollingHash(actorHash, authorityHash);

        Assert.NotEqual(actorHash, polished);
        Assert.Equal(actorHash, SnapshotChecksumActorDeltaPolicy.PolishActorDeltaRollingHash(actorHash, authorityEventHash: 0));
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesActorDeltaRollingHashFromAuthorityEvent()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.ActorDeltaRollingHash;

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

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesAuthorityEventRollingHash()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 19),
        });
        var before = store.AuthorityEventRollingHash;

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

        Assert.NotEqual(before, store.AuthorityEventRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void PolishLineSpecRollingHash_MixesActorDeltaTailWhenBothPresent()
    {
        var lineSpecHash = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 3, special: 6, success: true);
        var actorHash = SnapshotChecksumActorDeltaPolicy.MixRecord(0, new ActorDeltaRecord
        {
            ActorId = 6,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 42,
        });
        var polished = SnapshotChecksumActorDeltaPolicy.PolishLineSpecRollingHash(lineSpecHash, actorHash);

        Assert.NotEqual(lineSpecHash, polished);
        Assert.Equal(lineSpecHash, SnapshotChecksumActorDeltaPolicy.PolishLineSpecRollingHash(lineSpecHash, actorDeltaHash: 0));
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesLineSpecRollingHashFromActorDelta()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 5, special: 9, success: true);
        var before = store.LineSpecRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 13,
                ClassId = 4,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 62,
            },
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesLineSpecRollingHashAtTailFromActorDelta()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 6, special: 10, success: true);
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 20),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.LineSpecRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 14,
                ClassId = 5,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 64,
            },
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesPresentationEchoRollingHashAtTailFromActorDelta()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 21),
        });
        var before = store.PresentationEchoRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 15,
                ClassId = 6,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 66,
            },
        });

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesAuthorityEventRollingHashAtTailFromActorDelta()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 22),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.AuthorityEventRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 16,
                ClassId = 7,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 68,
            },
        });

        Assert.NotEqual(before, store.AuthorityEventRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesActorDeltaRollingHashAtTailFromPresentationEcho()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 23),
        });
        var before = store.ActorDeltaRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 17,
                ClassId = 8,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 70,
            },
        });

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesLineSpecRollingHashAtTailFromPresentationEcho()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        store.NoteLineSpec(lineIndex: 7, special: 11, success: true);
        var before = store.LineSpecRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 18,
                ClassId = 9,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 72,
            },
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesPresentationEchoRollingHashAtTailFromLineSpec()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        store.NoteLineSpec(lineIndex: 8, special: 12, success: true);
        var before = store.PresentationEchoRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 19,
                ClassId = 10,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 74,
            },
        });

        Assert.NotEqual(before, store.PresentationEchoRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesAuthorityEventRollingHashAtTailFromPresentationEcho()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 23),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        store.NoteLineSpec(lineIndex: 9, special: 13, success: true);
        var before = store.AuthorityEventRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 20,
                ClassId = 11,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 76,
            },
        });

        Assert.NotEqual(before, store.AuthorityEventRollingHash);
        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesLineSpecRollingHashAtTailFromAuthorityEvent()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 10, special: 14, success: true);
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 24),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.LineSpecRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 21,
                ClassId = 12,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 78,
            },
        });

        Assert.NotEqual(before, store.LineSpecRollingHash);
        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
    }

    [Fact]
    public void CommitAppliedActorDeltas_PolishesActorDeltaRollingHashAtTailFromLineSpec()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 11, special: 15, success: true);
        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 25),
        });
        store.CommitAppliedPresentationEcho(PresentationEchoCodec.CreateExampleBlock());
        var before = store.ActorDeltaRollingHash;

        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 22,
                ClassId = 13,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 82,
            },
        });

        Assert.NotEqual(before, store.ActorDeltaRollingHash);
        Assert.NotEqual(0u, store.LineSpecRollingHash);
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_PolishesAuthorityEventRollingHashFromActorDelta()
    {
        var store = new GuestWorldStateStore();
        store.CommitAppliedActorDeltas(new[]
        {
            new ActorDeltaRecord
            {
                ActorId = 10,
                ClassId = 5,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 91,
            },
        });
        var before = store.AuthorityEventRollingHash;

        store.CommitAppliedAuthorityEvents(new[]
        {
            AuthorityEventsCodec.CreateSpawnExample("DoomImp", actorId: 22),
        });

        Assert.NotEqual(before, store.AuthorityEventRollingHash);
        Assert.NotEqual(0u, store.ActorDeltaRollingHash);
    }
}
