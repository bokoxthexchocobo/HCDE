using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CorpseCrashTests
{
    [Theory]
    [InlineData(-99, 1)]
    [InlineData(-100, 1)]
    [InlineData(-101, 2)]
    public void CrashSelectsStrictGibThresholdAndMarksAfterAction(int health, int expected)
    {
        var actor = new Actor { Corpse = true, CrashState = 1, ExtremeCrashState = 2, GibHealth = -100 };
        actor.Health = health;
        var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, self =>
            { calls++; Assert.False(self.Crashed); }), new(-1, 2, self =>
            { calls++; Assert.False(self.Crashed); })], 0);
        actor.Crash(); actor.Crash();
        Assert.True(actor.Crashed); Assert.Equal(expected, actor.States.Current); Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, false, false, false)]
    [InlineData(false, false, true, false, false)]
    [InlineData(false, false, false, true, false)]
    [InlineData(false, false, false, false, true)]
    public void CrashHonorsFlags(bool dontCorpse, bool iceCorpse, bool crashed, bool living, bool killed)
    {
        var actor = new Actor { DontCorpse = dontCorpse, IceCorpse = iceCorpse,
            Crashed = crashed, Corpse = !living && !killed, Killed = killed, CrashState = 1 };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        actor.Crash();
        Assert.Equal(killed ? 1 : 0, actor.States.Current);
        Assert.Equal(crashed || killed, actor.Crashed);
    }

    [Fact]
    public void MissingExtremeFallsBackToNormalAndMarksCrashed()
    {
        var actor = new Actor { Corpse = true, CrashState = 1 };
        actor.Health = -101;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        actor.Crash(); Assert.Equal(1, actor.States.Current); Assert.True(actor.Crashed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloorLandingAndStationaryCorpseCrashOnlyOnce(bool falling)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); var calls = 0;
        actor.Corpse = true; actor.CrashState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, _ => calls++)], 0);
        if (falling) { actor.Z = Fixed.FromInt(2); actor.VelocityZ = Fixed.FromInt(-3); actor.OnGround = false; }
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        Assert.True(actor.Crashed); Assert.Equal(1, calls); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void StateLabelsAndConfiguredIndicesSurviveSaveAndOlderSaveClearsThem()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); var older = SimSavegame.Write(sim, true);
        actor.CrashState = actor.SpawnState; actor.ExtremeCrashState = actor.GenericCrushState;
        Assert.True(AcsActorStates.TryFindNamedState(actor, "Crash", true, out var normal));
        Assert.True(AcsActorStates.TryFindNamedState(actor, "Crash.Extreme", true, out var extreme));
        Assert.Equal(actor.CrashState, normal); Assert.Equal(actor.ExtremeCrashState, extreme);
        sim.RestoreState(sim.CaptureState());
        var saved = SimSavegame.Write(sim, true); var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = Assert.Single(restored.Actors);
        Assert.Equal(actor.CrashState, loaded.CrashState); Assert.Equal(actor.ExtremeCrashState, loaded.ExtremeCrashState);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older); Assert.Equal(-1, loaded.CrashState); Assert.Equal(-1, loaded.ExtremeCrashState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidNegativeStateInArchiveIsRejected(bool extreme)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        if (extreme) actor.ExtremeCrashState = 0; else actor.CrashState = 0;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), -2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void LandingOnActorCrashesKilledActorWithoutCorpseFlag()
    {
        var sim = Room(); var rider = sim.AddBot(100, 0); var support = sim.AddBot(100, 0);
        rider.Killed = true; rider.Corpse = false; rider.CanPass = true; rider.CrashState = 1;
        rider.States.Configure(rider, [new(-1, 0), new(-1, 1)], 0);
        rider.Z = Fixed.FromInt(60); rider.VelocityZ = Fixed.FromInt(-4); rider.OnGround = false;
        ActorPhysics.Step(sim, rider);
        Assert.Equal(support.Z.ToDouble() + support.Height.ToDouble(), rider.Z.ToDouble());
        Assert.True(rider.OnMobj); Assert.True(rider.Crashed); Assert.Equal(1, rider.States.Current);
    }

    [Fact]
    public void AirborneCorpseWaitsUntilLandingAndPreservesActionVelocity()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.Corpse = true; actor.CrashState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, self => self.VelocityX = Fixed.FromInt(7))], 0);
        actor.Z = Fixed.FromInt(20); actor.OnGround = false;
        ActorPhysics.Step(sim, actor); Assert.False(actor.Crashed); Assert.Equal(0, actor.States.Current);
        actor.Z = Fixed.FromInt(1); actor.VelocityZ = Fixed.FromInt(-2);
        ActorPhysics.Step(sim, actor); Assert.True(actor.Crashed); Assert.Equal(Fixed.FromInt(7), actor.VelocityX);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 18 }] });
}
