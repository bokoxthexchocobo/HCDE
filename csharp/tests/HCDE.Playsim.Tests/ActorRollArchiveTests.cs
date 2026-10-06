using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRollArchiveTests
{
    private static byte[] WriteRollArchive()
    {
        var state = Room().CaptureState();
        foreach (var pose in state.Actors) pose.ContactFlags = null;
        return LegacyActorArchiveFixture.Write(state);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 80 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2035, X = 200 }],
    });

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(0x40000000u)]
    [InlineData(0xffff0000u)]
    [InlineData(uint.MaxValue)]
    public void CurrentArchiveRestoresExactRollsAndChecksumContinuation(uint roll)
    {
        var original = Room();
        original.Actors[0].Roll = new BamAngle(roll);
        original.Actors[1].Roll = new BamAngle(123456789);
        original.Level.Sectors[0].HealthFloor = 17;
        original.Tick();
        var bytes = LegacyActorArchiveFixture.Write(original);
        Assert.Equal(18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var restored = Room();
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(roll, restored.Actors[0].Roll.Raw);
        Assert.Equal(123456789u, restored.Actors[1].Roll.Raw);
        Assert.Equal(17, restored.Level.Sectors[0].HealthFloor);
        Assert.Equal(original.Checksum, restored.Checksum);
        original.Tick(); restored.Tick();
        Assert.Equal(original.Checksum, restored.Checksum);
    }

    [Theory]
    [InlineData(77u)]
    [InlineData(0x80000000u)]
    [InlineData(uint.MaxValue)]
    public void LegacyArchiveRestoresDefaultRoll(uint laterRoll)
    {
        var sim = Room();
        var state = sim.CaptureState();
        foreach (var pose in state.Actors) { pose.Roll = null; pose.ContactFlags = null; }
        var bytes = LegacyActorArchiveFixture.Write(state);
        Assert.Equal(15, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Actors[0].Roll = new BamAngle(laterRoll);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0u, sim.Actors[0].Roll.Raw);
        var fresh = Room(); SimSavegame.Apply(fresh, bytes);
        Assert.Equal(fresh.Checksum, sim.Checksum);
        Assert.Equal(SimSavegame.Write(fresh), SimSavegame.Write(sim));
        sim.Tick(); fresh.Tick(); Assert.Equal(fresh.Checksum, sim.Checksum);
    }

    [Fact]
    public void RollChangesChecksum()
    {
        var first = Room(); var second = Room();
        second.Actors[1].Roll = new BamAngle(1);
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void TruncatedArchivesAreRejected()
    {
        var bytes = WriteRollArchive();
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(int.MaxValue)]
    public void InvalidTrailerSizesAreRejected(int size)
    {
        var bytes = WriteRollArchive();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-actor-roll-size", error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(3)]
    public void InvalidTrailerCountsAreRejected(int count)
    {
        var bytes = WriteRollArchive();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), count);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-actor-roll-count", error);
    }

    [Fact]
    public void ValidTrailerWithWrongActorCountIsRejected()
    {
        var bytes = WriteRollArchive();
        var shorter = bytes[..^4];
        BinaryPrimitives.WriteInt32LittleEndian(shorter.AsSpan(shorter.Length - 12), 1);
        BinaryPrimitives.WriteInt32LittleEndian(shorter.AsSpan(shorter.Length - 4), 12);
        Assert.False(SimSavegame.TryRead(shorter, out var state, out var error));
        Assert.Equal("save-actor-roll-count", error);
        Assert.Empty(state.Actors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IncompleteCurrentStateCannotSilentlyDropRoll(bool omitHealth)
    {
        var state = Room().CaptureState();
        if (omitHealth) state.GeometryHealth = null;
        else state.Actors[0].Roll = null;
        Assert.Throws<InvalidOperationException>(() => LegacyActorArchiveFixture.Write(state));
    }
}
