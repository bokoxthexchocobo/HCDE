using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BuddhaPersistenceTests
{
    [Fact]
    public void BinaryRestoreOnFreshActorPreservesBuddhaAndOtherPowers()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.GivePowerBuddha(); player.GivePowerDamage(); player.GivePowerProtection();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(52, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state); var loaded = restored.Players.Single();
        Assert.Equal(2100, loaded.PowerBuddhaTics);
        Assert.Equal(875, loaded.PowerDamageTics); Assert.Equal(875, loaded.PowerProtectionTics);
        ActorDamage.Apply(loaded, 1000);
        Assert.Equal(1, loaded.Health);
        Assert.False(loaded.IsDead);
    }

    [Fact]
    public void PoseRestoresChangedTimerAndLegacyArchiveClearsIt()
    {
        var sim = Room(); var oldBytes = SimSavegame.Write(sim); var player = sim.Players.Single();
        player.PowerBuddhaTics = 123;
        var pose = sim.CaptureState(); player.PowerBuddhaTics = 1;
        sim.RestoreState(pose); Assert.Equal(123, player.PowerBuddhaTics);
        Assert.True(SimSavegame.TryRead(oldBytes, out var old, out var error), error);
        sim.RestoreState(old); Assert.Equal(0, player.PowerBuddhaTics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeDeathRuleRemovesBuddha(bool telefrag)
    {
        var player = new PlayerPawn(); player.GivePowerBuddha();
        ActorDamage.Apply(player, telefrag ? ActorDamage.TelefragDamage : 1000,
            flags: telefrag ? DamageFlags.None : DamageFlags.Forced);
        Assert.True(player.IsDead); Assert.Equal(0, player.PowerBuddhaTics);
    }

    [Fact]
    public void NegativeBuddhaTimerIsRejected()
    {
        var sim = Room(); sim.Players.Single().GivePowerBuddha(); var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-damage-power-timer", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
