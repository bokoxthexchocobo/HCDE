using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSoundSaveTests
{
    [Fact]
    public void ActorSoundsRoundTripAndSnapshotsOwnTheirDictionaries()
    {
        var sim = Room(); var actor = sim.Players.Single();
        var sounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push", [ActorSoundType.See] = "" };
        actor.ActorSounds = sounds;
        var snapshot = sim.CaptureState(); sounds[ActorSoundType.Push] = "changed";
        sim.RestoreState(snapshot); actor = sim.Players.Single(); Assert.Equal("push", actor.ActorSounds[ActorSoundType.Push]);
        var bytes = SimSavegame.Write(sim); Assert.Equal(122, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        actor.ActorSounds = new Dictionary<ActorSoundType, string>(); sim.RestoreState(saved);
        Assert.Equal("push", sim.Players.Single().ActorSounds[ActorSoundType.Push]); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacySaveClearsSoundOverrides()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.Players.Single().ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        sim.RestoreState(saved); Assert.Empty(sim.Players.Single().ActorSounds);
    }

    [Fact]
    public void MalformedSoundTrailerAndInvalidSnapshotAreRejected()
    {
        var sim = Room(); sim.Players.Single().ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 12), 99);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.NotNull(error);
        var snapshot = sim.CaptureState();
        snapshot.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(snapshot));
        snapshot.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [(ActorSoundType)99] = "bad" };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
    }

    [Fact]
    public void SoundChecksumDetectsNameAndSelectorChanges()
    {
        var sim = Room(); var baseline = sim.Checksum;
        var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        sim.RestoreState(state); var see = sim.Checksum; Assert.NotEqual(baseline, see);
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "other" };
        sim.RestoreState(state); Assert.NotEqual(see, sim.Checksum);
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = "see" };
        sim.RestoreState(state); Assert.NotEqual(see, sim.Checksum);
    }

    [Fact]
    public void SoundChecksumIgnoresDictionaryInsertionOrderAndSurvivesSaveRestore()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string>
        { [ActorSoundType.Push] = "push", [ActorSoundType.See] = "see" };
        sim.RestoreState(state); var expected = sim.Checksum;
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string>
        { [ActorSoundType.See] = "see", [ActorSoundType.Push] = "push" };
        sim.RestoreState(state); Assert.Equal(expected, sim.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var saved, out var error), error);
        sim.RestoreState(saved); Assert.Equal(expected, sim.Checksum);
    }

    [Fact]
    public void EmptySoundOverrideIsDistinctFromAbsentAndLegacyRestoreRecoversChecksum()
    {
        var sim = Room(); var baseline = sim.Checksum; var legacy = SimSavegame.Write(sim);
        var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "" };
        sim.RestoreState(state); Assert.NotEqual(baseline, sim.Checksum);
        Assert.True(SimSavegame.TryRead(legacy, out var saved, out var error), error);
        sim.RestoreState(saved); Assert.Equal(baseline, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
