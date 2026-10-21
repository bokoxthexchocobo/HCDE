namespace HCDE.Scripting.Tests;

public class ZccCompareRelStringEmitTests
{
    [Fact]
    public void StringCompareOpcode_MatchesNativeCmpFlags()
    {
        Assert.Equal(ZccCompareRelStringEmit.CmpLt, ZccCompareRelStringEmit.StringCompareOpcode('<', false));
        Assert.Equal(ZccCompareRelStringEmit.CmpLe | ZccCompareRelStringEmit.CmpCheck,
            ZccCompareRelStringEmit.StringCompareOpcode('>', false));
    }

    [Fact]
    public void ApplyKonstFlags_SetsBkAndCk()
    {
        var op = ZccCompareRelStringEmit.ApplyKonstFlags(ZccCompareRelStringEmit.CmpLt, true, true);
        Assert.Equal(ZccCompareRelStringEmit.CmpBk | ZccCompareRelStringEmit.CmpCk | ZccCompareRelStringEmit.CmpLt, op);
    }
}
