namespace HCDE.Gamedata.Tests;

public class VmXlatLineFlagTranslateTests
{
    [Fact]
    public void ApplyZeroSpecial_ClearsArgs()
    {
        var args = new[] { 0, 5, 6, 7, 8 };
        VmXlatLineFlagTranslate.ApplyZeroSpecial(4, args, out var special, out _, out var arg0);
        Assert.Equal(0, special);
        Assert.Equal(4, arg0);
        Assert.Equal(0, args[1]);
    }
}
