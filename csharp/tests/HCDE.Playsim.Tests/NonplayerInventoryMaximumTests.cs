namespace HCDE.Playsim.Tests;

public class NonplayerInventoryMaximumTests
{
    [Theory]
    [InlineData("Clip", 200)]
    [InlineData("Shell", 50)]
    [InlineData("RocketAmmo", 50)]
    [InlineData("Cell", 300)]
    [InlineData("Fist", 1)]
    [InlineData("Pistol", 1)]
    [InlineData("Shotgun", 1)]
    [InlineData("BFG9000", 1)]
    [InlineData("BlueCard", 1)]
    [InlineData("RedSkull", 1)]
    [InlineData("YellowCard", 1)]
    [InlineData("Backpack", 1)]
    public void AbsentInventoryMaximumUsesNativeClassDefault(string type, int maximum)
    {
        var actor = new Actor();
        Assert.Equal(maximum, AcsPlayerInventory.Count(actor, type.ToLowerInvariant(), true));
        Assert.Equal(maximum, AcsPlayerInventory.Count(actor, [type], 0, true));
        Assert.Equal(0, AcsPlayerInventory.Count(actor, type, false));
        Assert.Equal(0, AcsPlayerInventory.Count(null, type, true));
    }

    [Fact]
    public void PlayerQueriesRetainInstanceCapacity()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.SetAmmoCapacity(player, "Clip", -7);
        Assert.Equal(-7, AcsPlayerInventory.Count(player, "Clip", true));
        Assert.Equal(200, AcsPlayerInventory.Count(new Actor(), "Clip", true));
        Assert.Equal(0, AcsPlayerInventory.Count(new Actor(), "UnknownAmmo", true));
    }
}
