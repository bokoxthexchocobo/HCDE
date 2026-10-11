using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerKeySaveTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(7)]
    public void SavedKeysRestoreAfterInventoryReset(int bits)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.BlueKey = (bits & 1) != 0; player.Inventory.RedKey = (bits & 2) != 0;
        player.Inventory.YellowKey = (bits & 4) != 0;
        player.Inventory.GiveHealthPickup("MedPatch", 2);
        var saved = SimSavegame.Write(sim); Assert.Equal(164, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal((bits & 1) != 0, player.Inventory.BlueKey); Assert.Equal((bits & 2) != 0, player.Inventory.RedKey);
        Assert.Equal((bits & 4) != 0, player.Inventory.YellowKey); Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void KeylessLegacySaveClearsCurrentOwnership()
    {
        var sim = Room(); var player = sim.Players.Single(); var saved = SimSavegame.Write(sim);
        Assert.NotEqual(164, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.Inventory.BlueKey = true; SimSavegame.Apply(sim, saved); Assert.False(player.Inventory.BlueKey);
    }

    [Fact]
    public void InvalidKeyBitsAreRejectedBeforeApplyingState()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.BlueKey = true;
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 8);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.True(player.Inventory.BlueKey);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
