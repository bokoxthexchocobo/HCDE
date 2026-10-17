using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceLandingTests
{
    [Theory]
    [InlineData(-9, -8, true)]
    [InlineData(-8, -8, false)]
    [InlineData(-7, -8, false)]
    [InlineData(-7, -6.5, true)]
    [InlineData(-9, -10, false)]
    public void IceImpactUsesStrictConfiguredLandingSpeed(double velocity, double threshold, bool interrupted)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.DamageType = "iCe"; actor.LandingSpeed = threshold;
        Drop(actor, velocity); ActorPhysics.Step(sim, actor);
        Assert.Equal(interrupted ? 1 : 100, actor.States.RemainingTics);
        Assert.Equal(0, actor.Z.Raw); Assert.Equal(0, actor.VelocityZ.Raw);
        Assert.Equal(interrupted ? 0 : Fixed.FromInt(4).Raw, actor.VelocityX.Raw);
        Assert.Equal(interrupted ? 0 : Fixed.FromInt(2).Raw, actor.VelocityY.Raw);
    }

    [Fact]
    public void IceImpactReturnsBeforeCrashEvenWithoutIceCorpseFlag()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.Killed = true; actor.DamageType = "Ice"; actor.CrashState = 1;
        Drop(actor, -9); ActorPhysics.Step(sim, actor);
        Assert.False(actor.IceCorpse); Assert.False(actor.Crashed);
        Assert.Equal(0, actor.States.Current); Assert.Equal(1, actor.States.RemainingTics);
    }

    [Fact]
    public void IceCorpseFlagAloneDoesNotTriggerIceDamageLandingBranch()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.IceCorpse = true;
        Drop(actor, -9); ActorPhysics.Step(sim, actor);
        Assert.Equal(100, actor.States.RemainingTics);
    }

    [Fact]
    public void ShortenedTimerAdvancesRepresentedShatterAction()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.DamageType = "Ice"; Drop(actor, -9); ActorPhysics.Step(sim, actor);
        actor.States.Tick(actor);
        Assert.Equal(1, actor.States.Current); Assert.True(actor.Shattering);
    }

    [Fact]
    public void IceDoesNotInterruptBeforeReachingFloor()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.DamageType = "Ice"; Drop(actor, -9); actor.Z = Fixed.FromInt(30);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(100, actor.States.RemainingTics); Assert.True(actor.Z.Raw > 0);
    }

    [Fact]
    public void LandingSpeedAndInterruptedTimerSurviveFreshSave()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        var older = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        actor.LandingSpeed = -6.5;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        actor.DamageType = "Ice"; Drop(actor, -7); ActorPhysics.Step(sim, actor);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(314, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved); var loaded = Assert.Single(restored.Actors);
        Assert.Equal(-6.5, loaded.LandingSpeed); Assert.Equal(1, loaded.States.RemainingTics);
        Assert.Equal(0, loaded.VelocityX.Raw); Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older); Assert.Equal(-8, loaded.LandingSpeed);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteSnapshotAndBinaryValuesAreRejected(double value)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        var snapshot = sim.CaptureState(); snapshot.Actors[0].LandingSpeed = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(snapshot));
        Assert.Equal(-8, actor.LandingSpeed);
        actor.LandingSpeed = -6.5; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteDoubleLittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-landingspeed-state", error);
    }

    private static void Drop(Actor actor, double velocity)
    {
        actor.Z = Fixed.FromInt(1); actor.OnGround = false;
        actor.VelocityX = Fixed.FromInt(4); actor.VelocityY = Fixed.FromInt(2);
        actor.VelocityZ = Fixed.FromDouble(velocity);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 18 }] });
        var actor = Assert.Single(sim.Actors); actor.NoFriction = true;
        actor.States.Configure(actor, [new(100, 1), new(-1, 1, self => self.Shattering = true)], 0);
        return sim;
    }
}
