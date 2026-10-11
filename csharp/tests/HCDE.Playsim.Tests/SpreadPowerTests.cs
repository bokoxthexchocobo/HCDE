using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpreadPowerTests
{
    [Fact]
    public void NativeDurationAndRefreshRulesApply()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerSpread", 7));
        Assert.Equal(1050, player.PowerSpreadTics); player.PowerSpreadTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerSpread"));
        Assert.Equal(500, player.PowerSpreadTics); player.PowerSpreadTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerSpread"));
        Assert.Equal(1050, AcsActorPowerups.RemainingTics(player, "PowerSpread"));
    }

    [Fact]
    public void MarkerExpiresWithoutHealingOrChangingHealthMaximum()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        player.GivePowerSpread(1); sim.Tick();
        Assert.Equal(0, player.PowerSpreadTics); Assert.Equal(50, player.Health);
        Assert.Equal(100, player.GetMaxHealth(true)); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void InventoryCheckSetTakeAndClearUsePowerRules()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerSpread", 1));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerSpread", 0));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerSpread"));
        player.PowerSpreadTics = 50;
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerSpread", 5));
        Assert.Equal(50, player.PowerSpreadTics);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerSpread"));
        Assert.Equal(0, player.PowerSpreadTics);
        player.GivePowerSpread(); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerSpreadTics);
    }

    [Fact]
    public void SavePreservesMarkerAndCustomStrengthsAndLegacyClearsMarker()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerRegeneration(70, strength: 9);
        var legacy = SimSavegame.Write(sim); player.GivePowerSpread(80);
        var bytes = SimSavegame.Write(sim); Assert.Equal(181, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(80, player.PowerSpreadTics); Assert.Equal(9, player.PowerRegenerationStrength);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerSpreadTics);
    }

    [Fact]
    public void InvalidDurationFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerSpread(80);
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        var state = sim.CaptureState(); state.Actors.Single().PowerSpreadTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(80, player.PowerSpreadTics);
    }

    [Fact]
    public void DefinitionUnitsAndChecksumIncludeMarker()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.Spread, -3); Assert.Equal(105, player.PowerSpreadTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
