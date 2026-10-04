namespace HCDE.Playsim.Tests;

public class GreenArmorPrecisionTests
{
    [Theory]
    [InlineData(PickupCatalog.GreenArmor)]
    [InlineData(PickupCatalog.ArmorBonus)]
    public void NativeGreenProtectionIsReportedAsFixedPoint(int pickup)
    {
        var player = new PlayerPawn();
        Assert.True(PickupCatalog.TryGive(player, pickup));
        Assert.Equal(21846, AcsPlayerInventory.ArmorInfo(player, 2, new AcsGlobalStrings()));
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(19999, 6666)]
    [InlineData(20000, 6667)]
    [InlineData(60000, 20001)]
    public void DamageUsesNativeFractionBeforeIntegerTruncation(int damage, int saved)
    {
        Assert.Equal(saved, ActorDamage.AbsorbArmor(damage, 100000, 33, 0, 0, 0));
    }

    [Fact]
    public void FullAllowancePrecedesFractionAndArmorStillCapsSaving()
    {
        Assert.Equal(6677, ActorDamage.AbsorbArmor(20010, 100000, 33, 0, 10, 0));
        Assert.Equal(100, ActorDamage.AbsorbArmor(60000, 100, 33, 0, 0, 0));
    }

    [Theory]
    [InlineData(50, 32768)]
    [InlineData(25, 16384)]
    public void OtherProtectionValuesRetainTheirReportedFraction(int percent, int expected)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, percent);
        Assert.Equal(expected, AcsPlayerInventory.ArmorInfo(player, 2, new AcsGlobalStrings()));
    }
}
