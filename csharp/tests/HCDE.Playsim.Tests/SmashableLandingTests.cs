using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SmashableLandingTests
{
    [Theory]
    [InlineData(false, false, 20)]
    [InlineData(true, false, 0)]
    [InlineData(true, true, 20)]
    public void SmashUsesCurrentHealthAndOrdinaryInvulnerabilityGates(bool smashable, bool invulnerable, int health)
    {
        var (sim, actor) = Room(); actor.Smashable = smashable; actor.Invulnerable = invulnerable;
        Drop(actor); ActorPhysics.Step(sim, actor);
        Assert.Equal(health, actor.Health);
    }

    [Fact]
    public void SmashDamageRespectsDamageFactor()
    {
        var (sim, actor) = Room(); actor.Smashable = true; actor.DamageFactor = Fixed.FromDouble(0.5);
        Drop(actor); ActorPhysics.Step(sim, actor);
        Assert.Equal(10, actor.Health);
    }

    [Fact]
    public void SmashTypedDeathRunsAtFloorBeforeCrashAndPreservesActionVelocity()
    {
        var (sim, actor) = Room(); var calls = new List<string>(); actor.Smashable = true; actor.CrashState = 2;
        actor.SetTypedDeath("Smash", 1);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, self =>
        {
            Assert.Equal(0, self.Z.Raw); Assert.Equal("Smash", self.DamageType);
            self.VelocityX = Fixed.FromInt(7); calls.Add("death");
        }), new(-1, 2, _ => calls.Add("crash"))], 0);
        Drop(actor); ActorPhysics.Step(sim, actor);
        Assert.Equal(new[] { "death", "crash" }, calls); Assert.True(actor.Crashed);
        Assert.Equal(Fixed.FromInt(7), actor.VelocityX);
    }

    [Theory]
    [InlineData(-9, 20)]
    [InlineData(-8, 0)]
    public void IceHardImpactReturnsBeforeSmashButThresholdEqualityDoesNot(int velocity, int health)
    {
        var (sim, actor) = Room(); actor.Smashable = true; actor.DamageType = "Ice";
        Drop(actor); actor.VelocityZ = Fixed.FromInt(velocity); ActorPhysics.Step(sim, actor);
        Assert.Equal(health, actor.Health);
        if (health > 0) Assert.Equal(1, actor.States.RemainingTics);
    }

    [Theory]
    [InlineData(20, -2)]
    [InlineData(0, 0)]
    public void AirborneOrStationaryActorIsNotSmashed(int z, int velocity)
    {
        var (sim, actor) = Room(); actor.Smashable = true;
        actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity); actor.OnGround = z == 0;
        ActorPhysics.Step(sim, actor); Assert.Equal(20, actor.Health);
    }

    [Fact]
    public void AcsSaveChecksumAndOldSaveDefaultsWork()
    {
        var (sim, actor) = Room(); sim.RestoreState(sim.CaptureState());
        var baseline = sim.Checksum; var older = SimSavegame.Write(sim, true);
        Assert.True(AcsActorFlags.TrySet(actor, "actor.smashable", true));
        Assert.True(AcsActorFlags.TryGet(actor, "SMASHABLE", out var value)); Assert.True(value);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(315, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var (restored, loaded) = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(loaded.Smashable); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        Drop(loaded); ActorPhysics.Step(restored, loaded); Assert.Equal(0, loaded.Health);
        SimSavegame.Apply(restored, older); Assert.False(loaded.Smashable);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedFlagsAreRejected(int value)
    {
        var (sim, actor) = Room(); var snapshot = sim.CaptureState(); snapshot.Actors[0].SmashableFlag = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot)); Assert.False(actor.Smashable);
        actor.Smashable = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-smashable-value", error);
    }

    private static void Drop(Actor actor)
    { actor.Z = Fixed.FromInt(1); actor.VelocityZ = Fixed.FromInt(-2); actor.OnGround = false; }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = Assert.Single(sim.Actors); actor.Brain = null; actor.DeathState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 0);
        return (sim, actor);
    }
}
