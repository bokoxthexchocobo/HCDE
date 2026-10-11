using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerAmmoSaveTests
{
    [Fact]
    public void ChangedAmmoCapacitiesAndBackpackRestoreAlongsideKeysAndHealthItems()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory;
        inventory.Bullets = 123; inventory.Shells = 32; inventory.Rockets = 17; inventory.Cells = 250;
        inventory.MaxBullets = 400; inventory.MaxShells = 100; inventory.MaxRockets = 100; inventory.MaxCells = 600;
        inventory.HasBackpack = true; inventory.BlueKey = true; inventory.GiveHealthPickup("MedPatch", 2);
        var saved = SimSavegame.Write(sim); Assert.Equal(165, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(123, inventory.Bullets); Assert.Equal(32, inventory.Shells); Assert.Equal(17, inventory.Rockets);
        Assert.Equal(250, inventory.Cells); Assert.Equal(400, inventory.MaxBullets); Assert.Equal(100, inventory.MaxShells);
        Assert.Equal(100, inventory.MaxRockets); Assert.Equal(600, inventory.MaxCells); Assert.True(inventory.HasBackpack);
        Assert.True(inventory.BlueKey); Assert.Equal(2, inventory.CountHealthPickup("MedPatch"));
    }

    [Theory]
    [InlineData(12, -1)]
    [InlineData(44, 2)]
    [InlineData(8, 2)]
    public void InvalidAmmoTrailerIsRejectedBeforeMutation(int relativeOffset, int invalid)
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory; inventory.Bullets = 20;
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + relativeOffset), invalid);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.Equal(20, inventory.Bullets);
    }

    [Fact]
    public void OmittedDefaultAmmoRestoresStartingValuesWithoutClearingOtherInventory()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory;
        inventory.BlueKey = true; inventory.GiveHealthPickup("MedPatch", 2);
        var saved = SimSavegame.Write(sim);
        inventory.Bullets = 0; inventory.Shells = 45; inventory.Rockets = 12; inventory.Cells = 99;
        inventory.MaxBullets = 400; inventory.MaxShells = 100; inventory.MaxRockets = 100; inventory.MaxCells = 600;
        inventory.HasBackpack = true;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(50, inventory.Bullets); Assert.Equal(0, inventory.Shells); Assert.Equal(0, inventory.Rockets);
        Assert.Equal(0, inventory.Cells); Assert.Equal(200, inventory.MaxBullets); Assert.Equal(50, inventory.MaxShells);
        Assert.Equal(50, inventory.MaxRockets); Assert.Equal(300, inventory.MaxCells); Assert.False(inventory.HasBackpack);
        Assert.True(inventory.BlueKey); Assert.Equal(2, inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void InMemoryDefaultSnapshotAlsoRestoresStartingAmmo()
    {
        var sim = Room(); var state = sim.CaptureState(); var inventory = sim.Players.Single().Inventory;
        inventory.Bullets = 1; inventory.Cells = 20;
        sim.RestoreState(state); Assert.Equal(50, inventory.Bullets); Assert.Equal(0, inventory.Cells);
    }

    [Fact]
    public void OmittedCanonicalAmmoDoesNotBecomePatchedStartingAmmo()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: HCDE.Gamedata.DehackedPatch.Apply("Misc 0\nInitial Bullets = 20\n"));
        var inventory = sim.Players.Single().Inventory; inventory.Bullets = 50;
        var saved = SimSavegame.Write(sim); inventory.Bullets = 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(50, inventory.Bullets);
    }

    [Fact]
    public void ChangedAndDefaultCooperativeOwnersRestoreIndependently()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }]
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        var players = sim.Players.ToArray(); players[0].Inventory.Bullets = 123;
        var saved = SimSavegame.Write(sim); players[0].Inventory.Bullets = 0; players[1].Inventory.Bullets = 0;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(123, players[0].Inventory.Bullets); Assert.Equal(50, players[1].Inventory.Bullets);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
