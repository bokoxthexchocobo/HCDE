namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandPauseSaveTests
{
    [Fact]
    public void ShouldTogglePauseOnLevelNative_OnLevel() =>
        Assert.True(VmDNetDoCommandPauseSave.ShouldTogglePauseOnLevelNative(gameState: 2, levelState: 2));

    [Fact]
    public void ShouldReadSavegamePathsNative_OnLevel() =>
        Assert.True(VmDNetDoCommandPauseSave.ShouldReadSavegamePathsNative(gameState: 2, levelState: 2));

}
