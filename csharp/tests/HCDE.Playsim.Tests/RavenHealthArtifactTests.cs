using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RavenHealthArtifactTests
{
    [Theory]
    [InlineData("ArtiHealth", 25, 1, 16)]
    [InlineData("ArtiHealth", 25, 1, 25)]
    [InlineData("ArtiSuperHealth", 100, 2, 16)]
    [InlineData("ArtiSuperHealth", 100, 2, 25)]
    public void GrantsUseGameDefaultLimitAndNativeHealthMode(string name, int health, int mode, int maximum)
    {
        var sim = Room(maximum); var player = sim.Players.Single(); player.Health = 40;
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name.ToLowerInvariant(), int.MaxValue);
        Assert.Equal(new HealthPickupStack(health, maximum, mode, name), Assert.Single(player.Inventory.HealthPickups));
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, AcsPlayerInventory.Use(player, name));
        Assert.Equal(Math.Min(100, 40 + health), player.Health);
        Assert.Equal(maximum - 1, player.Inventory.CountHealthPickup(name));
    }

    [Fact]
    public void GrantedFlaskAndUrnFollowEmergencyPriority()
    {
        var player = Room(16).Players.Single(); player.Health = 10;
        AcsPlayerInventory.Give(player, "ArtiHealth", 2); AcsPlayerInventory.Give(player, "ArtiSuperHealth", 1);
        Assert.False(ActorDamage.Apply(player, 20).Killed); Assert.Equal(15, player.Health);
        Assert.Equal(1, player.Inventory.CountHealthPickup("ArtiHealth")); Assert.Equal(1, player.Inventory.CountHealthPickup("ArtiSuperHealth"));
        Assert.False(ActorDamage.ApplyPoisonDamage(player, 40).Killed); Assert.Equal(75, player.Health);
        Assert.Equal(1, player.Inventory.CountHealthPickup("ArtiHealth")); Assert.Equal(0, player.Inventory.CountHealthPickup("ArtiSuperHealth"));
    }

    [Fact]
    public void DefaultLimitDoesNotOverrideExplicitStrifeClassLimit()
    {
        var player = Room(16).Players.Single(); AcsPlayerInventory.Give(player, "MedPatch", int.MaxValue);
        Assert.Equal(20, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.NotEqual(Room(16).Checksum, Room(25).Checksum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidDefaultLimitIsRejected(int maximum)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Room(maximum));
    }

    private static AuthoritySimulation Room(int maximum) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(DefaultInventoryMaximum: maximum, AutoUseHealth: true));
}
