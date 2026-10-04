using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathmatchWeaponAmmoTests
{
    [Theory]
    [InlineData(false, false, 2, 20, 28)]
    [InlineData(true, false, 2, 20, 28)]
    [InlineData(false, true, 2, 8, 16)]
    [InlineData(true, true, 2, 8, 16)]
    [InlineData(false, false, 0, 40, 50)]
    [InlineData(true, false, 0, 40, 50)]
    public void NewWeaponBonusPrecedesSkillScalingAndDuplicatesUseNormalAmmo(
        bool script, bool disabled, int skill, int first, int second)
    {
        var sim = Room(skill);
        sim.NoExtraAmmo = disabled;
        var player = sim.Players.Single();
        for (var i = 0; i < 2; i++)
        {
            if (script) AcsPlayerInventory.Give(player, "Shotgun", 1);
            else Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
            Assert.Equal(i == 0 ? first : second, player.Inventory.Shells);
        }
    }

    [Fact]
    public void IgnoreSkillStillAllowsDeathmatchBonus()
    {
        var sim = Room(0);
        var player = sim.Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun, ignoreSkill: true));
        Assert.Equal(20, player.Inventory.Shells);
    }

    [Fact]
    public void SettingAffectsChecksumAndSurvivesSameSimulationRestore()
    {
        var normal = Room(2);
        var configured = Room(2);
        configured.NoExtraAmmo = true;
        normal.Tick(); configured.Tick();
        Assert.NotEqual(normal.Checksum, configured.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(configured), out var state, out var error), error);
        configured.RestoreState(state);
        Assert.True(configured.NoExtraAmmo);
    }

    private static AuthoritySimulation Room(int skill) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Skill: skill, Mode: SpawnGameMode.Deathmatch));
}
