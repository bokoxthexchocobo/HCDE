using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPitchArchiveTests
{
    [Theory]
    [InlineData(1, 45.25)]
    [InlineData(1, -89)]
    [InlineData(3001, 120.5)]
    [InlineData(3001, -180)]
    public void CurrentArchiveRoundTripsActorPitch(int type, double pitch)
    {
        var sim = Room(type); var actor = sim.Actors.Single(); actor.PitchDegrees = pitch;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.PitchDegrees = 0; sim.RestoreState(state);
        Assert.Equal(pitch, actor.PitchDegrees);
    }

    [Theory]
    [InlineData(1, 90)]
    [InlineData(1, 181)]
    [InlineData(1, -181)]
    public void InvalidPitchIsRejectedBeforeAnyActorOrClockMutation(int type, int pitch)
    {
        var sim = Room(type); var actor = sim.Actors.Single(); actor.PitchDegrees = 10;
        var state = new SimSaveState { Tic = 200, Exited = true };
        state.Actors.Add(new SimActorPose { Id = actor.Id, X = Fixed.FromInt(123).Raw,
            Health = 1, Pitch = Fixed.FromInt(pitch).Raw, HasPhysics = false });
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic); Assert.False(sim.Exited);
        Assert.Equal(0, actor.X.Raw); Assert.Equal(10, actor.PitchDegrees);
        Assert.True(actor.Health > 1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3001)]
    public void VersionFiveArchiveRemainsReadable(int type)
    {
        var sim = Room(type); sim.Actors.Single().PitchDegrees = type == 1 ? 30 : 0;
        var captured = sim.CaptureState();
        foreach (var pose in captured.Actors) { pose.Roll = null; pose.ContactFlags = null; }
        var legacyState = new SimSaveState { Tic = captured.Tic, CombatRandomState = captured.CombatRandomState };
        legacyState.Actors.AddRange(captured.Actors);
        legacyState.Sectors.AddRange(captured.Sectors);
        var bytes = SimSavegame.Write(legacyState); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 5);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.Actors.Single().PitchDegrees = -20; sim.RestoreState(state);
        Assert.Equal(type == 1 ? 30 : 0, sim.Actors.Single().PitchDegrees);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }],
    });
}
