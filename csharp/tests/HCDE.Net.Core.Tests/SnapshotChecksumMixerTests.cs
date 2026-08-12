namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumMixerTests
{
    [Fact]
    public void ComputeRng_IsStable()
    {
        var hashA = SnapshotChecksumMixer.ComputeRng(rngSeed: 12345, gameTic: 99);
        var hashB = SnapshotChecksumMixer.ComputeRng(rngSeed: 12345, gameTic: 99);
        Assert.Equal(hashA, hashB);
        Assert.NotEqual(hashA, SnapshotChecksumMixer.ComputeRng(rngSeed: 12345, gameTic: 100));
    }

    [Fact]
    public void ComputeActors_IsOrderAgnostic()
    {
        var actorsA = new[]
        {
            new SnapshotChecksumActorSample("DoomImp"u8, health: 60),
            new SnapshotChecksumActorSample("ZombieMan"u8, health: 20),
        };
        var actorsB = new[]
        {
            new SnapshotChecksumActorSample("ZombieMan"u8, health: 20),
            new SnapshotChecksumActorSample("DoomImp"u8, health: 60),
        };

        Assert.Equal(
            SnapshotChecksumMixer.ComputeActors(actorsA),
            SnapshotChecksumMixer.ComputeActors(actorsB));
    }

    [Fact]
    public void Session_ComputeIfStale_WritesMatchingServerChunk()
    {
        var session = new SnapshotChecksumSession();
        var inputs = new SnapshotChecksumInputs(
            players: new[] { new SnapshotChecksumPlayerSample(0, health: 100, playerState: 1, onGround: true) },
            sectors: new[] { new SnapshotChecksumSectorSample(0, floorHeight: 0.0, ceilingHeight: 128.0, lightLevel: 160, special: 0) },
            movers: Array.Empty<SnapshotChecksumMoverSample>(),
            actors: new[] { new SnapshotChecksumActorSample("ShotgunGuy"u8, health: 70) },
            rngSeed: 4242,
            gameTic: 50);

        session.ComputeIfStale(50, inputs);
        session.ComputeIfStale(50, inputs);

        Span<byte> chunk = stackalloc byte[LiveConstants.SnapshotChecksumBlockSize];
        Assert.Equal(LiveConstants.SnapshotChecksumBlockSize, session.WriteServerChunk(chunk, gameTic: 50));

        Assert.True(SnapshotChecksumCodec.TryRead(chunk, out var remoteTic, out var remoteHashes, out _, out _));
        Assert.Equal(50u, remoteTic);
        Assert.Equal(session.CategoryHashes.ToArray(), remoteHashes);

        var cursor = 0;
        Assert.True(session.Ring.TryReadAndCompare(
            chunk,
            ref cursor,
            serverTic: 50,
            checksumEnabled: true,
            SnapshotChecksumRing.DefaultEnabledCategoryMask,
            out var mismatches,
            out _));
        Assert.Empty(mismatches);
    }

    [Fact]
    public void NoteLineSpec_AffectsLineSpecCategory()
    {
        var session = new SnapshotChecksumSession();
        session.NoteLineSpec(lineIndex: 9, special: 11, success: true);
        var inputs = new SnapshotChecksumInputs(
            Array.Empty<SnapshotChecksumPlayerSample>(),
            Array.Empty<SnapshotChecksumSectorSample>(),
            Array.Empty<SnapshotChecksumMoverSample>(),
            Array.Empty<SnapshotChecksumActorSample>(),
            rngSeed: 0,
            gameTic: 1);

        session.ComputeIfStale(1, inputs, categoryMask: 1 << (int)SnapshotChecksumCategory.LineSpec);
        Assert.NotEqual(0u, session.CategoryHashes[(int)SnapshotChecksumCategory.LineSpec]);
    }
}
