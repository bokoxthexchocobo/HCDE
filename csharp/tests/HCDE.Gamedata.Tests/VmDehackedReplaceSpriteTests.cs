namespace HCDE.Gamedata.Tests;

public class VmDehackedReplaceSpriteTests
{
    [Fact]
    public void IsPlayerSpritePrefix_DetectsPlay()
    {
        Assert.True(VmDehackedReplaceSprite.IsPlayerSpritePrefix("PLAY"));
    }
}
