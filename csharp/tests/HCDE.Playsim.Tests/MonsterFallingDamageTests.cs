using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterFallingDamageTests
{
    [Theory]
    [InlineData(-23, true, 100)]
    [InlineData(-24, true, 94)]
    [InlineData(-24.25, true, 93)]
    [InlineData(-35, true, 28)]
    [InlineData(-36, true, -1)]
    [InlineData(-24, false, -1)]
    public void ProperFormulaAndLegacyInstantDeathMatchNative(double velocity, bool proper, int expected)
    {
        var (sim, actor) = Room(enabled: true, proper: proper); Drop(actor, velocity);
        ActorPhysics.Step(sim, actor);
        if (expected < 0) Assert.True(actor.IsDead); else Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(false, false, 100)]
    [InlineData(false, true, 94)]
    [InlineData(true, false, 94)]
    public void MapOrActorFlagEnablesDamage(bool mapFlag, bool actorFlag, int expected)
    {
        var (sim, actor) = Room(mapFlag, proper: true); actor.FallDamage = actorFlag; Drop(actor, -24);
        ActorPhysics.Step(sim, actor); Assert.Equal(expected, actor.Health);
    }

    [Fact]
    public void SectorOptOutOverridesMapAndActorEnablement()
    {
        var (sim, actor) = Room(enabled: true, noFall: true); actor.FallDamage = true; Drop(actor, -40);
        ActorPhysics.Step(sim, actor); Assert.Equal(100, actor.Health);
    }

    [Fact]
    public void NonMonsterDoesNotUseMonsterFallingPolicy()
    {
        var (sim, actor) = Room(enabled: true); actor.IsMonster = false; actor.FallDamage = true; Drop(actor, -40);
        ActorPhysics.Step(sim, actor); Assert.Equal(100, actor.Health);
    }

    [Fact]
    public void TypedFallingDeathOccursBeforeCrash()
    {
        var (sim, actor) = Room(enabled: true); actor.SetTypedDeath("Falling", 1); actor.CrashState = 2;
        Drop(actor, -24); ActorPhysics.Step(sim, actor);
        Assert.Equal("Falling", actor.DamageType); Assert.Equal(2, actor.States.Current); Assert.True(actor.Crashed);
    }

    [Fact]
    public void AirborneMonsterDoesNotTakeImpactDamage()
    {
        var (sim, actor) = Room(enabled: true); Drop(actor, -24); actor.Z = Fixed.FromInt(100);
        ActorPhysics.Step(sim, actor); Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(true, 100)]
    [InlineData(false, -1)]
    public void ScaledDamageRespectsInvulnerabilityButLegacyTelefragDoesNot(bool proper, int expected)
    {
        var (sim, actor) = Room(enabled: true, proper: proper); actor.Invulnerable = true; Drop(actor, -24);
        ActorPhysics.Step(sim, actor);
        if (expected < 0) Assert.True(actor.IsDead); else Assert.Equal(expected, actor.Health);
    }

    [Fact]
    public void AcsActorFlagSaveDefaultsChecksumAndContinuationWork()
    {
        var (sim, actor) = Room(proper: true); var older = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        Assert.True(AcsActorFlags.TrySet(actor, "actor.falldamage", true));
        Assert.True(AcsActorFlags.TryGet(actor, "FALLDAMAGE", out var value)); Assert.True(value);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        Drop(actor, -24); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(318, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var (restored, loaded) = Room(proper: true); SimSavegame.Apply(restored, saved);
        Assert.True(loaded.FallDamage); Assert.Equal(saved, SimSavegame.Write(restored, true));
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(restored, loaded);
        Assert.Equal(94, loaded.Health);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
        SimSavegame.Apply(restored, older); Assert.False(loaded.FallDamage);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidSavedFlagIsRejected(int value)
    {
        var (sim, actor) = Room(); var state = sim.CaptureState(); state.Actors[0].FallDamageFlag = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.False(actor.FallDamage);
        actor.FallDamage = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-falldamage-value", error);
    }

    private static void Drop(Actor actor, double velocity)
    { actor.Z = Fixed.FromInt(1); actor.OnGround = false; actor.VelocityZ = Fixed.FromDouble(velocity); }

    private static (AuthoritySimulation Sim, Actor Actor) Room(bool enabled = false, bool proper = false, bool noFall = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MonsterFallingDamage = enabled, ProperMonsterFallingDamage = proper,
            Sectors = [new LevelSector { CeilingHeight = 256, NoFallingDamage = noFall }],
            Things = [new LevelThing { Type = 3004 }],
        });
        var actor = Assert.Single(sim.Actors); actor.Brain = null; actor.Health = 100; actor.DeathState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 0);
        return (sim, actor);
    }
}
