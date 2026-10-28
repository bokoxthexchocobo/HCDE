namespace HCDE.Gamedata.Tests;

public class VmXlatLineFlagTranslateTests
{
    [Fact]
    public void ApplyMaskPass_ClearsMaskedBits()
    {
        var rules = new VmLineFlagTranslation[16];
        rules[0] = new VmLineFlagTranslation { IsMask = true, NewValue = 0 };
        var result = VmXlatLineFlagTranslate.ApplyMaskPass(1, rules, false, 0);
        Assert.Equal(0u, result);
    }

    [Fact]
    public void ApplyValuePass_SetsPassthrough()
    {
        var rules = new VmLineFlagTranslation[16];
        rules[1] = new VmLineFlagTranslation { IsMask = false, NewValue = unchecked((uint)-1) };
        var (_, _, passthrough, _) = VmXlatLineFlagTranslate.ApplyValuePass(2, rules);
        Assert.True(passthrough);
    }

    [Fact]
    public void ApplyZeroSpecial_ClearsArgs()
    {
        var args = new int[] { 5, 1, 2, 3, 4 };
        VmXlatLineFlagTranslate.ApplyZeroSpecial(args);
        Assert.Equal(5, args[0]);
        Assert.Equal(0, args[1]);
    }
}
