using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProsperityPowerTests
{
    [Fact]
    public void NativeDurationAndRefreshRulesApply()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerProsperity", 7));
        Assert.Equal(4200, player.PowerProsperityTics); player.PowerProsperityTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerProsperity"));
        Assert.Equal(500, player.PowerProsperityTics); player.PowerProsperityTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerProsperity"));
        Assert.Equal(4200, AcsActorPowerups.RemainingTics(player, "PowerProsperity"));
    }

    [Fact]
    public void MarkerExpiresWithoutHealingOrChangingHealthMaximum()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        player.GivePowerProsperity(1); sim.Tick();
        Assert.Equal(0, player.PowerProsperityTics); Assert.Equal(50, player.Health);
        Assert.Equal(100, player.GetMaxHealth(true)); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void InventoryCheckSetTakeAndClearUsePowerRules()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerProsperity", 1));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerProsperity", 0));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerProsperity"));
        player.PowerProsperityTics = 50;
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerProsperity", 5));
        Assert.Equal(50, player.PowerProsperityTics);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerProsperity"));
        Assert.Equal(0, player.PowerProsperityTics);
        player.GivePowerProsperity(); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerProsperityTics);
    }

    [Fact]
    public void SavePreservesMarkerAndCustomStrengthsAndLegacyClearsMarker()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerRegeneration(70, strength: 9);
        var legacy = SimSavegame.Write(sim); player.GivePowerProsperity(80);
        var bytes = SimSavegame.Write(sim); Assert.Equal(180, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(80, player.PowerProsperityTics); Assert.Equal(9, player.PowerRegenerationStrength);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerProsperityTics);
    }

    [Fact]
    public void InvalidDurationFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerProsperity(80);
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        var state = sim.CaptureState(); state.Actors.Single().PowerProsperityTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(80, player.PowerProsperityTics);
    }

    [Fact]
    public void DefinitionUnitsAndChecksumIncludeMarker()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.Prosperity, -3); Assert.Equal(105, player.PowerProsperityTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
