namespace HCDE.Playsim.Tests;

public class VmPUserCrouchSpriteTests
{
    [Fact]
    public void SetupCrouchSprite_AllowsWhenDisabled() =>
        Assert.True(VmPUserCrouchSprite.EvaluateSetupCrouchSprite(new VmPUserCrouchSprite.SetupCrouchSpriteContext(
            0, "PLAY", "PLYC", _ => 0, _ => 0, 1)));

    [Fact]
    public void SetupCrouchSprite_RejectsMissingLumps()
    {
        Assert.False(VmPUserCrouchSprite.EvaluateSetupCrouchSprite(new VmPUserCrouchSprite.SetupCrouchSpriteContext(
            5, "PLAY", "PLYC", _ => -1, _ => 0, 1)));
    }

    [Fact]
    public void CheckPlayerSprite_SquashesWhenNoCrouchSprite()
    {
        var ctx = new VmPUserCrouchSprite.CheckPlayerSpriteContext(
            0, 0, 1, 1, 1, 1, 1, 1,
            true, 0.5, 10, 10, -1, 0, 0, 0);
        var result = VmPUserCrouchSprite.EvaluateCheckPlayerSprite(ctx);
        Assert.Equal(0.5, result.ScaleY);
        Assert.Equal(10, result.Spritenum);
    }

    [Fact]
    public void CheckPlayerSprite_UsesCrouchSprite()
    {
        var ctx = new VmPUserCrouchSprite.CheckPlayerSpriteContext(
            0, 0, 1, 1, 1, 1, 1, 1,
            true, 0.5, 10, 10, 42, 0, 0, 0);
        var result = VmPUserCrouchSprite.EvaluateCheckPlayerSprite(ctx);
        Assert.Equal(42, result.Spritenum);
    }

    [Fact]
    public void CheckPlayerSprite_AppliesSkinScale()
    {
        var ctx = new VmPUserCrouchSprite.CheckPlayerSpriteContext(
            1, 0, 2, 2, 4, 4, 1, 1,
            false, 1, 1, 1, 0, 1, 0, 0);
        var result = VmPUserCrouchSprite.EvaluateCheckPlayerSprite(ctx);
        Assert.Equal(2, result.ScaleX);
        Assert.Equal(2, result.ScaleY);
    }
}
