namespace HCDE.Playsim.Tests;

public class ArmorInfoActivatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void MissingAndNonplayerActivatorsReturnNumericZero(int field)
    {
        var globals = new AcsGlobalStrings();
        Assert.Equal(0, AcsPlayerInventory.ArmorInfo(null, field, globals));
        Assert.Equal(0, AcsPlayerInventory.ArmorInfo(new Actor { Armor = 100, ArmorSavePercent = 50 }, field, globals));
        Assert.Equal(string.Empty, globals.GetByIndex(0));
    }

    [Fact]
    public void PlayerWithoutArmorReturnsNoneStringForClassQuery()
    {
        var globals = new AcsGlobalStrings();
        var result = AcsPlayerInventory.ArmorInfo(new PlayerPawn(), 0, globals);
        Assert.Equal("None", globals.Get(result));
    }

    [Fact]
    public void UnknownFieldReturnsZeroWithoutAllocatingString()
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, PickupCatalog.GreenArmor);
        var globals = new AcsGlobalStrings();
        Assert.Equal(0, AcsPlayerInventory.ArmorInfo(player, 99, globals));
        Assert.Equal(string.Empty, globals.GetByIndex(0));
    }
}
