using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponStaySettingTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, false, false, false)]
    [InlineData(SpawnGameMode.Single, true, false, true)]
    [InlineData(SpawnGameMode.Cooperative, false, false, true)]
    [InlineData(SpawnGameMode.Cooperative, false, true, false)]
    [InlineData(SpawnGameMode.Cooperative, true, true, true)]
    [InlineData(SpawnGameMode.Deathmatch, true, false, true)]
    public void SettingsControlRetentionAndDuplicateAmmo(SpawnGameMode mode, bool stay, bool applyDm, bool retained)
    {
        var sim = Room(mode); sim.WeaponStay = stay; sim.AlwaysApplyDmFlags = applyDm;
        sim.Tick();
        Assert.Equal(retained, sim.Actors.Any(a => a.DoomEdNum == PickupCatalog.Shotgun));
        Assert.Equal(retained ? sim.Players.Count() : 1,
            sim.Players.Count(p => p.Inventory.Owns(WeaponKind.Shotgun)));
        sim.Tick();
        Assert.All(sim.Players.Where(p => p.Inventory.Owns(WeaponKind.Shotgun)),
            p => Assert.Equal(mode == SpawnGameMode.Deathmatch ? 20 : 8, p.Inventory.Shells));
    }

    [Theory]
    [InlineData(SpawnGameMode.Single)]
    [InlineData(SpawnGameMode.Cooperative)]
    [InlineData(SpawnGameMode.Deathmatch)]
    public void DroppedWeaponDoesNotStayEvenWhenEnabled(SpawnGameMode mode)
    {
        var sim = Room(mode); sim.WeaponStay = true;
        sim.Actors[^1].Dropped = true;
        sim.Tick();
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void SettingsAffectChecksumAndRemainSimulationConfigurationOnRestore(bool stay, bool applyDm)
    {
        var normal = Room(SpawnGameMode.Single); var configured = Room(SpawnGameMode.Single);
        foreach (var sim in new[] { normal, configured }) sim.Players.Single().X = Fixed.FromInt(500);
        configured.WeaponStay = stay; configured.AlwaysApplyDmFlags = applyDm;
        normal.Tick(); configured.Tick();
        Assert.NotEqual(normal.Checksum, configured.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(configured), out var state, out var error), error);
        configured.RestoreState(state);
        Assert.Equal(stay, configured.WeaponStay);
        Assert.Equal(applyDm, configured.AlwaysApplyDmFlags);
    }

    private static AuthoritySimulation Room(SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = mode == SpawnGameMode.Single
            ? [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Shotgun }]
            : [new LevelThing { Type = 1 }, new LevelThing { Type = 2 }, new LevelThing { Type = PickupCatalog.Shotgun }],
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
