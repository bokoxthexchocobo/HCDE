using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ManualOnlyHealthPickupTests
{
    [Fact]
    public void DefaultModeCanBeUsedManuallyAfterSaveRestore()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 40;
        player.Inventory.AddHealthPickup(20, 2);
        var archive = SimSavegame.Write(sim);
        player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, archive);
        Assert.Equal(0, Assert.Single(player.Inventory.HealthPickups).AutoUseMode);
        Assert.True(player.Inventory.UseHealthPickup(0)); Assert.Equal(60, player.Health);
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LethalDamageCannotUseManualOnlyItemsEvenWithAutoUseSkill(bool poison)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 10;
        player.Inventory.AddHealthPickup(100, 3);
        var result = poison ? ActorDamage.ApplyPoisonDamage(player, 20) : ActorDamage.Apply(player, 20);
        Assert.True(result.Killed); Assert.Equal(3, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void NeitherAutomaticCollectorConsumesManualOnlyStack()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(100, 3);
        Assert.Equal(0, inventory.AutoUseHealth(20, true));
        Assert.Equal(40, inventory.AutoUseStrifeHealth(40, 100));
        Assert.Equal(3, Assert.Single(inventory.HealthPickups).Amount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void UndefinedModesRemainRejectedByInventoryAndArchive(int mode)
    {
        var inventory = new PlayerInventory();
        Assert.Equal("autoUseMode", Assert.Throws<ArgumentOutOfRangeException>(() => inventory.AddHealthPickup(20, 1, mode)).ParamName);
        var state = Room().CaptureState(); state.Actors.Single(a => a.HealthPickups is not null).HealthPickups = [new(20, 1, mode)];
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(AutoUseHealth: true));
}
