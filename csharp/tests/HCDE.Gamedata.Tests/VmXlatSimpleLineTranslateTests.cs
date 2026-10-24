namespace HCDE.Gamedata.Tests;

public class VmXlatSimpleLineTranslateTests
{
    [Fact]
    public void TryApplySimpleTranslation_MapsSpecial()
    {
        var trans = new VmSimpleLineTranslation { Special = 12, Flags = 1 };
        Assert.True(VmXlatSimpleLineTranslate.TryApplySimpleTranslation(5, 4, 0, trans, out var special, out _, out _));
        Assert.Equal(12, special);
    }
}
