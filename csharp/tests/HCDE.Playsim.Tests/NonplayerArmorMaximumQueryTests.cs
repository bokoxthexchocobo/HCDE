namespace HCDE.Playsim.Tests;

public class NonplayerArmorMaximumQueryTests
{
    [Theory]
    [InlineData("Armor")]
    [InlineData("BasicArmor")]
    public void MaximumUsesBasicArmorClassDefaultWithoutInferringOwnedInventory(string type)
    {
        var actor = new Actor { Armor = 150, ArmorSavePercent = 50 };
        Assert.Equal(1, AcsPlayerInventory.Count(actor, type.ToLowerInvariant(), true));
        Assert.Equal(0, AcsPlayerInventory.Count(actor, type, false));
        Assert.Equal(1, AcsPlayerInventory.Count(actor, [type], 0, true));
        Assert.Equal(0, AcsPlayerInventory.Count(null, type, true));
        Assert.Equal(0, AcsPlayerInventory.Count(actor, [type], -1, true));
    }

    [Theory]
    [InlineData("Armor")]
    [InlineData("BasicArmor")]
    public void PlayerMaximumUsesRetainedInstanceCapacityAfterDepletion(string type)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "BlueArmor", 1);
        AcsPlayerInventory.Take(player, "BasicArmor", 200);
        Assert.Equal(0, AcsPlayerInventory.Count(player, type, false));
        Assert.Equal(200, AcsPlayerInventory.Count(player, type, true));
        Assert.Equal(1, AcsPlayerInventory.Count(new Actor(), type, true));
    }

    [Theory]
    [InlineData("BasicArmorPickup")]
    [InlineData("BasicArmorBonus")]
    [InlineData("GreenArmor")]
    [InlineData("BlueArmor")]
    [InlineData("BlueArmorForMegasphere")]
    public void PickupClassMaximumIsIndependentOfBasicArmorDefault(string type)
    {
        Assert.Equal(0, AcsPlayerInventory.Count(new Actor(), type, true));
        Assert.Equal(0, AcsPlayerInventory.Count(new PlayerPawn(), type, true));
    }
}
