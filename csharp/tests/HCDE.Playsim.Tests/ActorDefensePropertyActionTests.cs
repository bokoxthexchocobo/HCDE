using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDefensePropertyActionTests
{
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void MassPreservesSignedNativeValueAndSave(int mass)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetMass(actor, mass);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Mass = 100; sim.RestoreState(state);
        Assert.Equal(mass, actor.Mass); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void InvulnerabilityBlocksOrdinaryDamageUntilUnset()
    {
        var actor = Room().AddBot(0, 0); var health = actor.Health;
        ActorPropertyActions.SetInvulnerable(actor);
        Assert.Equal(0, ActorDamage.Apply(actor, 5).HealthLost);
        Assert.Equal(health, actor.Health);
        ActorPropertyActions.UnsetInvulnerable(actor);
        Assert.Equal(5, ActorDamage.Apply(actor, 5, flags: DamageFlags.NoPain).HealthLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShootabilityUpdatesBothFlagsAndDamageEligibility(bool shootable)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Shootable = !shootable; actor.NonShootable = shootable;
        if (shootable) ActorPropertyActions.SetShootable(actor); else ActorPropertyActions.UnsetShootable(actor);
        Assert.Equal(shootable, actor.Shootable); Assert.Equal(!shootable, actor.NonShootable);
        Assert.Equal(shootable, actor.CanTakeDamage);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Shootable = !shootable; actor.NonShootable = shootable; sim.RestoreState(state);
        Assert.Equal(shootable, actor.Shootable); Assert.Equal(!shootable, actor.NonShootable);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameInvulnerabilityAndExplicitClearSurviveSave()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: ActorPropertyActions.SetInvulnerable)], 0);
        actor.States.Enter(actor, 1); Assert.True(actor.Invulnerable);
        ActorPropertyActions.UnsetInvulnerable(actor);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Invulnerable = true; sim.RestoreState(state);
        Assert.False(actor.Invulnerable); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 72)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    [InlineData(16, 8)]
    public void MalformedDefenseRecordIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.SetMass(sim.AddBot(0, 0), 5);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.StartsWith("save-defense-properties-", error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void InvalidMemoryFlagsRejectWriteAndRestoreBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors.Single().DefenseProperties = new(5, flags);
        actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(42, actor.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
