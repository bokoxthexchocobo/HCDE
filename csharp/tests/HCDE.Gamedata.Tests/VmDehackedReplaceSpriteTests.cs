namespace HCDE.Gamedata.Tests;

public class VmDehackedReplaceSpriteTests
{
    [Fact]
    public void IsPlayerSpritePrefix_DetectsPlay()
    {
        Assert.True(VmDehackedReplaceSprite.IsPlayerSpritePrefix("PLAY"));
    }

    [Fact]
    public void ReplaceSpriteInDataNative_PlayerPrefix()
    {
        Span<char> player = stackalloc char[4] { 'P', 'L', 'A', 'Y' };
        VmDehackedReplaceSprite.ReplaceSpriteInDataNative(
            "PLAY",
            "NEWP",
            player,
            new List<string>(),
            (_, _) => { });
        Assert.Equal('N', player[0]);
    }
}
