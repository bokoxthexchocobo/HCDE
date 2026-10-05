using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArmorRoundingTests
{
    [Theory]
    [InlineData(2, 100, 0, 2)]
    [InlineData(3, 100, 1, 2)]
    [InlineData(6, 100, 2, 4)]
    [InlineData(6, 1, 1, 5)]
    public void GreenArmorSavesAnExactThirdLimitedByRemainingArmor(int damage, int armor, int absorbed, int lost)
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = armor;
        player.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var result = ActorDamage.Apply(player, damage);
        Assert.Equal(absorbed, result.ArmorLost);
        Assert.Equal(lost, result.HealthLost);
        Assert.Equal(armor - absorbed, player.Inventory.Armor);
        Assert.Equal(100 - lost, player.Health);
    }

    [Fact]
    public void DrowningIgnoresArmorAndSlimeDoesNot()
    {
        var drowning = new PlayerPawn();
        drowning.Inventory.Armor = 100;
        drowning.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var drowned = ActorDamage.Apply(drowning, 30, damageType: "Drowning");
        Assert.Equal(0, drowned.ArmorLost);
        Assert.Equal(30, drowned.HealthLost);
        Assert.Equal(100, drowning.Inventory.Armor);

        var slime = new PlayerPawn();
        slime.Inventory.Armor = 100;
        slime.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var burned = ActorDamage.Apply(slime, 30, damageType: "Slime");
        Assert.Equal(10, burned.ArmorLost);
        Assert.Equal(20, burned.HealthLost);
        Assert.Equal(90, slime.Inventory.Armor);

        var lower = new PlayerPawn();
        lower.Inventory.Armor = 100;
        lower.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        ActorDamage.Apply(lower, 30, damageType: "drowning");
        Assert.Equal(100, lower.Inventory.Armor);
    }

    [Fact]
    public void FullAbsorbSavesThatSliceBeforeThePercent()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 50;
        player.Inventory.MaxFullAbsorb = 10;
        var result = ActorDamage.Apply(player, 30);
        Assert.Equal(20, result.ArmorLost);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal(20, player.Inventory.AbsorbCount);

        result = ActorDamage.Apply(player, 30);
        Assert.Equal(15, result.ArmorLost);
        Assert.Equal(65, player.Inventory.Armor);
        Assert.Equal(35, player.Inventory.AbsorbCount);
    }

    [Fact]
    public void MaxAbsorbStopsTheRunningTotal()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 100;
        player.Inventory.MaxAbsorb = 25;
        var result = ActorDamage.Apply(player, 40);
        Assert.Equal(25, result.ArmorLost);
        Assert.Equal(75, player.Inventory.Armor);
        Assert.Equal(15, result.HealthLost);

        result = ActorDamage.Apply(player, 40);
        Assert.Equal(0, result.ArmorLost);
        Assert.Equal(75, player.Inventory.Armor);
        Assert.Equal(40, result.HealthLost);
    }

    [Fact]
    public void MegaArmorClearsTheCapsAndKeepsTheCount()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 50;
        player.Inventory.MaxAbsorb = 10;
        player.Inventory.MaxFullAbsorb = 4;
        player.Inventory.AbsorbCount = 3;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.MegaArmor));
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(PlayerInventory.MegaSavePercent, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal(0, player.Inventory.MaxFullAbsorb);
        Assert.Equal(3, player.Inventory.AbsorbCount);
    }

    [Fact]
    public void AbsorbCapsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().Inventory.MaxAbsorb = 25;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().Inventory.MaxAbsorb = 25;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(25, left.Players.Single().Inventory.MaxAbsorb);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ADepletedSuitUsesTheBestStoredArmor()
    {
        var player = new PlayerPawn { Health = 400 };
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        player.Inventory.AbsorbCount = 4;
        Assert.True(player.Inventory.TryKeepArmorPickup(80, 50, maxAbsorb: 9, maxFullAbsorb: 3));
        Assert.True(player.Inventory.TryKeepArmorPickup(40, 100));
        Assert.Equal(2, player.Inventory.SpareArmor.Count);
        Assert.Equal(100, player.Inventory.Armor);

        var hit = ActorDamage.Apply(player, 400);
        Assert.Equal(100, hit.ArmorLost);
        Assert.Equal(300, hit.HealthLost);
        Assert.Equal(40, player.Inventory.Armor);
        Assert.Equal(100, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal(104, player.Inventory.AbsorbCount);
        Assert.Equal(80, player.Inventory.SpareArmor.Single().SaveAmount);

        var next = ActorDamage.Apply(player, 10);
        Assert.Equal(10, next.ArmorLost);
        Assert.Equal(30, player.Inventory.Armor);

        ActorDamage.Apply(player, 100);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal(9, player.Inventory.MaxAbsorb);
        Assert.Equal(3, player.Inventory.MaxFullAbsorb);
        Assert.Empty(player.Inventory.SpareArmor);

        var tied = new PlayerPawn { Health = 100 };
        tied.Inventory.Armor = 100;
        tied.Inventory.TryKeepArmorPickup(20, 50);
        tied.Inventory.TryKeepArmorPickup(30, 50);
        tied.Inventory.Armor = 0;
        ActorDamage.Apply(tied, 5);
        Assert.Equal(20, tied.Inventory.Armor);
        Assert.Equal(30, tied.Inventory.SpareArmor.Single().SaveAmount);
        Assert.Equal(95, tied.Health);

        var empty = new PlayerPawn();
        empty.Inventory.Armor = 1;
        empty.Inventory.ArmorSavePercent = 50;
        ActorDamage.Apply(empty, 10);
        Assert.Equal(0, empty.Inventory.Armor);
        Assert.Equal(0, empty.Inventory.ArmorSavePercent);

        var drowned = new PlayerPawn();
        drowned.Inventory.Armor = 100;
        drowned.Inventory.TryKeepArmorPickup(50, 50);
        drowned.Inventory.Armor = 0;
        drowned.Inventory.ArmorSavePercent = 0;
        ActorDamage.Apply(drowned, 10, damageType: "Drowning");
        Assert.Equal(0, drowned.Inventory.Armor);
        Assert.Single(drowned.Inventory.SpareArmor);

        var worn = new PlayerPawn();
        Assert.True(worn.Inventory.TryKeepArmorPickup(100, 33, maxFullAbsorb: 2));
        Assert.Equal(100, worn.Inventory.Armor);
        Assert.Equal(33, worn.Inventory.ArmorSavePercent);
        Assert.Equal(2, worn.Inventory.MaxFullAbsorb);
        Assert.Empty(worn.Inventory.SpareArmor);

        worn.Inventory.Armor = 200;
        Assert.False(PickupCatalog.TryGive(worn, PickupCatalog.GreenArmor));
        Assert.Empty(worn.Inventory.SpareArmor);
    }

    [Fact]
    public void SpareArmorIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().Inventory.Armor = 100;
        left.Players.Single().Inventory.TryKeepArmorPickup(40, 50);
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().Inventory.Armor = 100;
        right.Players.Single().Inventory.TryKeepArmorPickup(40, 50);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(40, left.Players.Single().Inventory.SpareArmor.Single().SaveAmount);
        Assert.Equal(checksum, left.Checksum);

        var cleared = new PlayerInventory();
        cleared.Armor = 100;
        cleared.TryKeepArmorPickup(40, 50);
        cleared.ResetToPistolStart();
        Assert.Empty(cleared.SpareArmor);

        var kept = new PlayerInventory();
        kept.Armor = 100;
        kept.TryKeepArmorPickup(40, 50);
        kept.FilterCoopRespawn(false, false, false, false, false, false);
        Assert.Single(kept.SpareArmor);
        kept.FilterCoopRespawn(false, false, false, loseArmor: true, false, false);
        Assert.Empty(kept.SpareArmor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
        Things = new[] { new LevelThing { Type = 1 } },
    });
}
