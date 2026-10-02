namespace HCDE.Playsim.Tests;

public class AcsHealthKeyMaximumTests
{
    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public void MaximumHealthIsIndependentOfCurrentAndBonusHealth(int health)
    {
        var player = new PlayerPawn { Health = health };
        Assert.Equal(health, AcsPlayerInventory.Count(player, "Health", false));
        Assert.Equal(100, AcsPlayerInventory.Count(player, "Health", true));
    }

    [Theory]
    [InlineData("BlueCard")]
    [InlineData("BlueSkull")]
    [InlineData("RedCard")]
    [InlineData("RedSkull")]
    [InlineData("YellowCard")]
    [InlineData("YellowSkull")]
    public void MaximumKeyCountDoesNotRequireOwnership(string name)
    {
        var player = new PlayerPawn();
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name, 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        AcsPlayerInventory.Take(player, name, 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name.ToLowerInvariant(), true));
    }
}
