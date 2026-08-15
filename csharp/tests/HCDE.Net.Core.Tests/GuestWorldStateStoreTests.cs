namespace HCDE.Net.Core.Tests;

public class GuestWorldStateStoreTests
{
    [Fact]
    public void ApplyPose_TracksPlayerHealthAndGroundState()
    {
        var store = new GuestWorldStateStore();
        var pose = new PlayerPoseWorldDelta(
            playerNum: 1,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor | LiveConstants.ServerWorldDeltaPoseOnGround,
            health: 85,
            armor: 0,
            posX: 0,
            posY: 0,
            posZ: 0,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0,
            pitchBams: 0);

        Assert.True(store.ApplyPose(recipientClientSlot: 1, pose, sequenceAck: 7));
        Assert.True(store.Players.TryGetValue(1, out var player));
        Assert.Equal(85, player.Health);
        Assert.True(player.OnGround);
    }

    [Fact]
    public void TryApply_MergesActorDeltaFields()
    {
        var store = new GuestWorldStateStore();
        var record = new ActorDeltaRecord
        {
            ActorId = 42,
            ClassId = 9,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 50,
        };

        Assert.True(store.TryApply(recipientClientSlot: 1, record));
        Assert.True(store.Actors.TryGetValue(42, out var actor));
        Assert.Equal(9, actor.ClassId);
        Assert.Equal(50, actor.Health);
    }
}

public class SnapshotChecksumPlaysimInputsTests
{
    [Fact]
    public void BuildAndCompute_ProducesStableCategoryHashes()
    {
        var store = new GuestWorldStateStore();
        store.ApplyPose(
            recipientClientSlot: 1,
            new PlayerPoseWorldDelta(
                0,
                LiveConstants.ServerWorldDeltaPoseHasActor,
                health: 100,
                armor: 0,
                posX: 0,
                posY: 0,
                posZ: 0,
                velX: 0,
                velY: 0,
                velZ: 0,
                yawBams: 0,
                pitchBams: 0),
            sequenceAck: 0);
        store.ApplySector(new SectorWorldDelta(sectorIndex: 3, flags: 0, floor: 16, ceiling: 128));
        store.TryApply(
            recipientClientSlot: 1,
            new ActorDeltaRecord
            {
                ActorId = 7,
                ClassId = 12,
                FieldMask = LiveConstants.ActorDeltaFieldHealth,
                Health = 30,
            });

        var session = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(session, store, gameTic: 11, rngSeed: 5);

        Assert.True(session.Ring.TryFind(11, out var hashes));
        Assert.Equal(LiveConstants.SnapshotChecksumCategoryCount, hashes.Length);
        Assert.NotEqual(0u, hashes[(int)SnapshotChecksumCategory.Players]);
        Assert.NotEqual(0u, hashes[(int)SnapshotChecksumCategory.Sectors]);
        Assert.NotEqual(0u, hashes[(int)SnapshotChecksumCategory.Actors]);
    }
}

public class CrossLanguageSoakEvidenceTests
{
    [Fact]
    public void TryWrite_WritesJsonWhenConfigured()
    {
        var evidenceDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", evidenceDir);
        try
        {
            var path = CrossLanguageSoakEvidence.TryWrite(
                "test_harness",
                new CrossLanguageSoakResult(CrossLanguageSoakStatus.Skipped, output: string.Empty, skipReason: "unit-test"));
            Assert.NotNull(path);
            Assert.True(File.Exists(path));
            var json = File.ReadAllText(path);
            Assert.Contains("test_harness", json);
            Assert.Contains("unit-test", json);
        }
        finally
        {
            Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", null);
            if (Directory.Exists(evidenceDir))
                Directory.Delete(evidenceDir, recursive: true);
        }
    }
}
