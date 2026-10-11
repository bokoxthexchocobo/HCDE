using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RegenerationPowerTests
{
    [Fact]
    public void GrantUsesNativeDurationAndRefreshRules()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerRegeneration", 7));
        Assert.Equal(4200, player.PowerRegenerationTics);
        player.PowerRegenerationTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerRegeneration"));
        player.PowerRegenerationTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerRegeneration"));
        Assert.Equal(4200, AcsActorPowerups.RemainingTics(player, "PowerRegeneration"));
    }

    [Fact]
    public void HealingUsesMapClockRatherThanTimeSinceGrant()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        sim.Tick(); player.GivePowerRegeneration();
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.Equal(50, player.Health); Assert.Empty(sim.DrainSoundRequests());
        sim.Tick(); Assert.Equal(55, player.Health);
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("*regenerate", sound.Sound); Assert.Equal(3, sound.Channel);
        Assert.Equal(player.Id, sound.ActorId);
    }

    [Theory]
    [InlineData(98, 100, true)]
    [InlineData(100, 100, false)]
    [InlineData(0, 0, false)]
    public void HealingCapsAtMaximumAndNeverRevives(int health, int expected, bool sound)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = health;
        player.GivePowerRegeneration(); sim.Tick();
        Assert.Equal(expected, player.Health);
        Assert.Equal(sound ? 1 : 0, sim.DrainSoundRequests().Count(request => request.Sound == "*regenerate"));
    }

    [Fact]
    public void LastActiveTickHealsThenExpires()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        player.GivePowerRegeneration(1); sim.Tick();
        Assert.Equal(55, player.Health); Assert.Equal(0, player.PowerRegenerationTics);
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.Equal(55, player.Health);
    }

    [Fact]
    public void InventoryActionsRemoveUntossableEffect()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerRegeneration", 1));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerRegeneration", false));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerRegeneration", 0));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerRegeneration"));
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerRegeneration"));
        Assert.Equal(0, player.PowerRegenerationTics);
        player.GivePowerRegeneration(); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerRegenerationTics);
    }

    [Fact]
    public void SavePreservesAllTimersAndHealingSchedule()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        sim.Tick(); player.GivePowerDrain(90); player.GivePowerInfiniteAmmo(80);
        var legacy = SimSavegame.Write(sim);
        player.GivePowerRegeneration(100);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(173, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(100, player.PowerRegenerationTics);
        Assert.Equal(90, player.PowerDrainTics); Assert.Equal(80, player.PowerInfiniteAmmoTics);
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.Equal(50, player.Health); sim.Tick(); Assert.Equal(55, player.Health);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerRegenerationTics);
    }

    [Fact]
    public void InvalidDurationFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerRegeneration(100);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(100, player.PowerRegenerationTics);
        var state = sim.CaptureState(); state.Actors.Single().PowerRegenerationTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(100, player.PowerRegenerationTics);
    }

    [Fact]
    public void DefinitionDurationAndChecksumIncludeEffect()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.Regeneration, -3);
        Assert.Equal(105, player.PowerRegenerationTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
