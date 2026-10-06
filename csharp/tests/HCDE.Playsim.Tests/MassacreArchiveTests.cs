using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class MassacreArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RestoredDeathRetainsSuppressionAndMovementFlags(bool massacre, bool floating)
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors);
        ActorDamage.Apply(parent, 1000, damageType: massacre ? "Massacre" : null);
        parent.InFloat = parent.VerticalFriction = floating;
        var bytes = WriteWithoutPainTimer(sim);
        Assert.Equal(massacre ? 20 : floating ? 19 : 18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        parent.Health = parent.ResurrectionHealth; parent.InFloat = parent.VerticalFriction = !floating;
        sim.RestoreState(state);
        Assert.True(parent.IsDead); Assert.Equal(massacre ? "Massacre" : null, parent.DeathDamageType);
        Assert.Equal(floating, parent.InFloat); Assert.Equal(floating, parent.VerticalFriction);
        for (var tic = 0; tic < 32; tic++) sim.Tick();
        Assert.Equal(massacre ? 0 : 3, sim.Actors.Count(a => a.DoomEdNum == 3006));
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidDeathBitsAreRejected(int bits)
    {
        var sim = Room(); ActorDamage.Apply(Assert.Single(sim.Actors), 1000, damageType: "Massacre");
        var bytes = WriteWithoutPainTimer(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), bits);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-death-flags", error); Assert.Empty(state.Actors);
    }
    [Fact]
    public void InvalidMemoryDeathBitsRejectWriteAndRestore()
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); var health = parent.Health;
        var state = sim.CaptureState(); state.Actors[0].DeathFlags = 2;
        Assert.Throws<InvalidOperationException>(() => LegacyActorArchiveFixture.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(health, parent.Health);
    }
    private static byte[] WriteWithoutPainTimer(AuthoritySimulation sim)
    {
        var state = sim.CaptureState();
        // Keep this fixture at its historical version; movement overrides have a later archive.
        foreach (var pose in state.Actors) { pose.PainDeath = null; pose.MovementActionFlags = null; }
        return LegacyActorArchiveFixture.Write(state);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 71 }] });
}