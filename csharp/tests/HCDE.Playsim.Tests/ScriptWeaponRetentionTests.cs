using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ScriptWeaponRetentionTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, false, false, false)]
    [InlineData(SpawnGameMode.Single, true, false, true)]
    [InlineData(SpawnGameMode.Cooperative, false, false, true)]
    [InlineData(SpawnGameMode.Cooperative, false, true, false)]
    [InlineData(SpawnGameMode.Cooperative, true, true, true)]
    [InlineData(SpawnGameMode.Deathmatch, false, false, false)]
    [InlineData(SpawnGameMode.Deathmatch, true, false, true)]
    public void FirstGrantSucceedsAndRepeatGrantUsesRetentionRules(
        SpawnGameMode mode, bool stay, bool applyDm, bool retained)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(Mode: mode));
        sim.WeaponStay = stay;
        sim.AlwaysApplyDmFlags = applyDm;
        var player = sim.Players.Single();
        var selected = player.Inventory.Selected;
        player.Inventory.Pending = WeaponKind.Chaingun;

        AcsPlayerInventory.Give(player, "Shotgun", 1);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        var firstAmmo = mode == SpawnGameMode.Deathmatch ? 20 : 8;
        Assert.Equal(firstAmmo, player.Inventory.Shells);

        AcsPlayerInventory.Give(player, "Shotgun", 5);
        Assert.Equal(firstAmmo + (retained ? 0 : 8), player.Inventory.Shells);
        Assert.Equal(selected, player.Inventory.Selected);
        Assert.Equal(WeaponKind.Chaingun, player.Inventory.Pending);
    }
}
