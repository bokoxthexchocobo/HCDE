using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedInitialBulletsTests
{
    [Theory]
    [InlineData(20)]
    [InlineData(100)]
    public void StartAndInventoryResetUsePatchedAmount(int amount)
    {
        var patch = DehackedPatch.Apply($"Misc 0\nInitial Bullets = {amount}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        var inventory = sim.Players.Single().Inventory; Assert.Equal(amount, inventory.Bullets);
        inventory.Bullets = 0; inventory.ResetToPistolStart(); Assert.Equal(amount, inventory.Bullets);
        inventory.Bullets = 0; inventory.FilterCoopRespawn(false, false, false, false, true, false);
        Assert.Equal(amount, inventory.Bullets);
    }

    [Fact]
    public void BaselineAndInvalidInputPreserveAmount()
    {
        var patch = DehackedPatch.Apply("Misc 0\nInitial Bullets = 100\n");
        Assert.Equal(100, DehackedPatch.Apply("", patch).InitialBullets);
        var invalid = DehackedPatch.Apply("Misc 0\nInitial Bullets = invalid\n", patch);
        Assert.NotEmpty(invalid.Errors); Assert.Equal(100, invalid.InitialBullets);
    }
}
