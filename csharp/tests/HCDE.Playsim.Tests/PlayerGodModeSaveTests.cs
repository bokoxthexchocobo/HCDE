using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerGodModeSaveTests
{
    [Theory]
    [InlineData(20, DamageFlags.None, false)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.None, true)]
    [InlineData(200, DamageFlags.Forced, true)]
    public void SavedGodModePreservesOrdinaryProtectionAndBypasses(int damage, DamageFlags flags, bool killed)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GodMode = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(156, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.GodMode = false; SimSavegame.Apply(sim, bytes);
        Assert.True(player.GodMode); Assert.Equal(bytes, SimSavegame.Write(sim));
        var result = ActorDamage.Apply(player, damage, flags: flags);
        Assert.Equal(killed, result.Killed);
        if (!killed) Assert.Equal(100, player.Health);
    }

    [Fact]
    public void SnapshotAndLegacyRestoreDoNotRetainStaleGodMode()
    {
        var sim = Room(); var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim);
        player.GodMode = true; var snapshot = sim.CaptureState();
        player.GodMode = false; sim.RestoreState(snapshot); Assert.True(player.GodMode);
        SimSavegame.Apply(sim, legacy); Assert.False(player.GodMode);
        Assert.Equal(20, ActorDamage.Apply(player, 20).HealthLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedFlagsAndRecursivePriorVersionAreRejected(bool invalidPrior)
    {
        var sim = Room(); sim.Players.Single().GodMode = true; var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (invalidPrior) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 156);
        else bytes[start + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-player-god-mode-invalid", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
