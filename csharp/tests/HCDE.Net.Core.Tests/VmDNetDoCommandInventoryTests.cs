namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandInventoryTests
{
    [Fact]
    public void ShouldRunInventoryLevelCommandNative_WhenAlive() =>
        Assert.True(VmDNetDoCommandInventory.ShouldRunInventoryLevelCommandNative(
            gameState: 2, levelState: 2, paused: false, playerState: 0, deadState: 4));

    [Fact]
    public void InventoryIdMatches_FindsSlot() =>
        Assert.True(VmDNetDoCommandInventory.InventoryIdMatches(7, 7));
}
