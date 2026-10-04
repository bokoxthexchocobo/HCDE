namespace HCDE.Playsim.Tests;

public class SpareArmorIdentityTests
{
    [Fact]
    public void DepletionWithoutSpareClearsSourceButRetainsCapacity()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(100, 50, armorType: "CustomArmor");
        inventory.Armor = 0;
        inventory.PromoteSpareArmor();
        Assert.Equal("None", inventory.ArmorType);
        Assert.Equal(100, inventory.ArmorMaximum);
        Assert.Equal(100, inventory.ArmorActualSaveAmount);
    }

    [Theory]
    [InlineData("CustomArmor")]
    [InlineData("BlueArmor")]
    public void StoredSuitRetainsItsSourceOnPromotion(string source)
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(100, 25);
        inventory.TryKeepArmorPickup(80, 50, armorType: source);
        Assert.Equal(source, inventory.SpareArmor.Single().ArmorType);
        inventory.Armor = 0;
        inventory.PromoteSpareArmor();
        Assert.Equal(source, inventory.ArmorType);
        Assert.Equal(80, inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void EqualProtectionKeepsFirstStoredSource()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(100, 25);
        inventory.TryKeepArmorPickup(80, 50, armorType: "FirstArmor");
        inventory.TryKeepArmorPickup(70, 50, armorType: "SecondArmor");
        inventory.Armor = 0;
        inventory.PromoteSpareArmor();
        Assert.Equal("FirstArmor", inventory.ArmorType);
        Assert.Equal("SecondArmor", inventory.SpareArmor.Single().ArmorType);
    }
}
