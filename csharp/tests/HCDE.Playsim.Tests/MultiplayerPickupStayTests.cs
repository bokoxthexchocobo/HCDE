using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MultiplayerPickupStayTests
{
    [Fact]
    public void CooperativeWeaponRemainsAvailableForEachPlayerWithoutRepeatedAmmo()
    {
        var sim = Room(SpawnGameMode.Cooperative, PickupCatalog.Shotgun);
        sim.Tick();
        Assert.All(sim.Players, p => Assert.True(p.Inventory.Owns(WeaponKind.Shotgun)));
        Assert.All(sim.Players, p => Assert.Equal(8, p.Inventory.Shells));
        Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
        sim.Tick();
        Assert.All(sim.Players, p => Assert.Equal(8, p.Inventory.Shells));
    }

    [Theory]
    [InlineData(SpawnGameMode.Cooperative)]
    [InlineData(SpawnGameMode.Deathmatch)]
    public void MultiplayerKeysStayForOtherPlayers(SpawnGameMode mode)
    {
        var sim = Room(mode, PickupCatalog.RedCard);
        sim.Tick();
        Assert.All(sim.Players, p => Assert.True(p.Inventory.RedKey));
        Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.RedCard);
    }

    [Fact]
    public void DroppedCooperativeWeaponIsConsumedAndGivesAmmoToOnePlayer()
    {
        var sim = Room(SpawnGameMode.Cooperative, PickupCatalog.Shotgun);
        sim.Actors[^1].Dropped = true;
        sim.Tick();
        Assert.Single(sim.Players, p => p.Inventory.Owns(WeaponKind.Shotgun));
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
    }

    [Fact]
    public void DeathmatchWeaponIsConsumedByOnePlayer()
    {
        var sim = Room(SpawnGameMode.Deathmatch, PickupCatalog.Shotgun);
        sim.Tick();
        Assert.Single(sim.Players, p => p.Inventory.Owns(WeaponKind.Shotgun));
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
    }

    [Fact]
    public void AlwaysPickupCannotConsumeStayingOwnedWeapon()
    {
        var sim = Room(SpawnGameMode.Cooperative, PickupCatalog.Pistol);
        AcsActorFlags.TrySet(sim.Actors[^1], "ALWAYSPICKUP", true);
        sim.Tick();
        Assert.All(sim.Players, p => Assert.Equal(50, p.Inventory.Bullets));
        Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
    }

    [Fact]
    public void SinglePlayerDuplicateKeyIsConsumed()
    {
        var sim = Room(SpawnGameMode.Single, PickupCatalog.RedCard);
        sim.Players.Single().Inventory.RedKey = true;
        sim.Tick();
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.RedCard);
    }

    private static AuthoritySimulation Room(SpawnGameMode mode, int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = mode == SpawnGameMode.Single
            ? [new LevelThing { Type = 1 }, new LevelThing { Type = type }]
            : [new LevelThing { Type = 1 }, new LevelThing { Type = 2 }, new LevelThing { Type = type }],
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
