namespace HCDE.Playsim.Tests;

public class NonplayerAmmoCapacityTests
{
    [Theory]
    [InlineData("Clip", 200)]
    [InlineData("Shell", 50)]
    [InlineData("RocketAmmo", 50)]
    [InlineData("Cell", 300)]
    public void CapacityUsesClassDefaultForNonplayerWithoutInventory(string name, int expected)
    {
        var actor = new Actor();
        Assert.Equal(expected, AcsPlayerInventory.AmmoCapacity(actor, name.ToLowerInvariant()));
        Assert.Equal(expected, AcsPlayerInventory.AmmoCapacity(actor, [name], 0));
        Assert.Equal(expected, AcsPlayerInventory.Count(actor, name, true));
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(null, name));
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(actor, [name], -1));
    }

    [Theory]
    [InlineData("ClipBox")]
    [InlineData("ShellBox")]
    [InlineData("RocketBox")]
    [InlineData("CellPack")]
    [InlineData("Shotgun")]
    [InlineData("UnknownAmmo")]
    public void CapacityRejectsIndirectAmmoAndOtherClasses(string name)
    {
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(new Actor(), name));
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(new PlayerPawn(), name));
    }

    [Fact]
    public void PlayerInstanceOverrideDoesNotChangeNonplayerDefault()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.SetAmmoCapacity(player, "Cell", -7);
        Assert.Equal(-7, AcsPlayerInventory.AmmoCapacity(player, "Cell"));
        Assert.Equal(300, AcsPlayerInventory.AmmoCapacity(new Actor(), "Cell"));
    }
}
