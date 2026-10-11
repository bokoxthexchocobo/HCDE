using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RavenWorldHealthTests
{
    [Theory]
    [InlineData(82, "ArtiHealth")]
    [InlineData(32, "ArtiSuperHealth")]
    public void ContactStoresArtifactAndRestoresInventory(int number, string name)
    {
        var sim = Room(number); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Health = 40; Assert.True(item.SpecialPickup); Assert.False(item.CanTakeDamage);
        sim.Tick(); Assert.True(item.Destroyed); Assert.Equal(40, player.Health);
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
        Assert.False(player.Inventory.Owns(WeaponKind.SuperShotgun));
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
    }

    [Theory]
    [InlineData(82, "ArtiHealth")]
    [InlineData(32, "ArtiSuperHealth")]
    public void FullStackRetainsWorldActorAcrossRestore(int number, string name)
    {
        var sim = Room(number); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup(name, 16);
        sim.Tick(); var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        var item = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.True(item.SpecialPickup); Assert.False(item.Destroyed);
        player.Inventory.TakeHealthPickup(name, 1); sim.Tick(); Assert.True(item.Destroyed);
        Assert.Equal(16, player.Inventory.CountHealthPickup(name));
    }

    [Fact]
    public void DefaultDoomMappingStillGrantsSuperShotgun()
    {
        var sim = Room(82, false); sim.Tick(); Assert.True(sim.Players.Single().Inventory.Owns(WeaponKind.SuperShotgun));
        Assert.Empty(sim.Players.Single().Inventory.HealthPickups);
    }

    [Fact]
    public void MappingChangesChecksumAndDoesNotClassifyDoomThing32AsPickup()
    {
        Assert.NotEqual(Room(32).Checksum, Room(32, false).Checksum);
        Assert.False(Room(32, false).Actors[^1].SpecialPickup);
    }

    [Fact]
    public void ConflictingHealthMappingsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => AuthoritySimulation.Start(new PlayLevel(),
            spawnOptions: new SpawnOptions(StrifeHealthPickups: true, RavenHealthPickups: true)));
    }

    [Fact]
    public void DoomWeaponDropCannotSpawnArtifactUnderRavenMapping()
    {
        var sim = Room(32); var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.SuperShotgun;
        var count = sim.Actors.Count;
        Assert.False(AcsPlayerInventory.Drop(player, "SuperShotgun"));
        Assert.True(player.Inventory.Owns(WeaponKind.SuperShotgun));
        Assert.Equal(0, ActorDropItem.Drop(sim, 0, player, "SuperShotgun", 1, 256));
        Assert.Equal(count, sim.Actors.Count);
    }

    private static AuthoritySimulation Room(int number, bool raven = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = number }]
    }, spawnOptions: new SpawnOptions(RavenHealthPickups: raven, DefaultInventoryMaximum: 16));
}
