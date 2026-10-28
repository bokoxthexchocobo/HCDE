namespace HCDE.Client.Tests;

public class VmDMainStartupProgressTests
{
    [Fact]
    public void ShouldSkipStartupUi_WhenBatchRun()
    {
        Assert.True(VmDMainStartupProgress.ShouldSkipStartupUi(true));
    }

    [Fact]
    public void ShouldOverrideWithCmdlineSkill_WhenSet()
    {
        Assert.True(VmDMainCheckDefaultSkill.ShouldOverrideWithCmdlineSkill(2));
    }

    [Fact]
    public void ResolveRenameTable_Heretic_HasPairs()
    {
        var (_, pairs) = VmDMainRenameSpritesPolicy.ResolveRenameTable(VmDMainRenameSpritesPolicy.GameHeretic);
        Assert.Equal(1, pairs);
    }

    [Fact]
    public void ShouldRenameSprites_FalseForDoom()
    {
        Assert.False(VmDMainRenameSpritesPolicy.ShouldRenameSprites(VmDMainRenameSpritesPolicy.GameDoom));
    }
}
