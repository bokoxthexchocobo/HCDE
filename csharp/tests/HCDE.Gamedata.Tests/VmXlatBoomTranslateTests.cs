namespace HCDE.Gamedata.Tests;

public class VmXlatBoomTranslateTests
{
    [Fact]
    public void TryTranslateBoomish_MatchesRange()
    {
        var boom = new List<VmBoomTranslator> { new() { FirstLineType = 1, LastLineType = 10, NewSpecial = 5 } };
        var args = new int[5];
        Assert.True(VmXlatBoomTranslate.TryTranslateBoomish(2, 3, 0, false, boom, out var special, out _, out _, args));
        Assert.Equal(5, special);
        Assert.Equal(3, args[0]);
    }
}
