namespace HCDE.Scripting.Tests;

public class ZccCompareRelResolveTests
{
    [Fact]
    public void UIntLeftSintRightWarning_OutOfRangeConstant()
    {
        var w = ZccCompareRelResolve.UIntLeftSintRightWarning(
            true, true, true, true, true, false, (uint)int.MaxValue + 1u, 0);
        Assert.Equal(ZccCompareRelResolve.SignedUnsignedWarn.SignedVsOutOfRangeUnsignedConstant, w);
    }

    [Fact]
    public void FoldFloatCompare_Geq()
    {
        Assert.Equal(1, ZccCompareRelResolve.FoldFloatCompare(ZccCompareRelResolve.TkGeq, 2, 2));
    }

    [Fact]
    public void TryFoldConstantCompare_StringLess()
    {
        Assert.Equal(1, ZccCompareRelResolve.TryFoldConstantCompare(
            true, true, false, false, '<', -1, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void NeedsTranslationCast_Below412()
    {
        Assert.True(ZccCompareRelResolve.NeedsTranslationCastToInt(true, true, 40000));
    }
}
