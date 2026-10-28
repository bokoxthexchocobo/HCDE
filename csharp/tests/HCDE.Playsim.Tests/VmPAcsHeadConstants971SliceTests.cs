namespace HCDE.Playsim.Tests;

public class VmPAcsHeadConstants971SliceTests
{
    [Fact]
    public void PcodeCommandCount_MatchesNativeEnum()
    {
        Assert.Equal(385, VmPAcsHeadConstants971Slice.PcodeCommandCount);
        Assert.True(VmPAcsHeadConstants971Slice.ValidPcodeOpcode(384));
        Assert.False(VmPAcsHeadConstants971Slice.ValidPcodeOpcode(385));
    }

    [Fact]
    public void EnumTail_IncludesTranslationRange5()
    {
        Assert.True(VmPAcsHeadConstants971Slice.IsEnumTailOpcode(
            VmPAcsHeadConstants971Slice.PcdTranslationrange5));
        Assert.True(VmPAcsHeadConstants971Slice.IsScriptArrayOpcode(
            VmPAcsHeadConstants971Slice.PcdDivscriptarray));
    }

    [Fact]
    public void AcsFixedPointConversions()
    {
        Assert.Equal(1.0, VmPAcsHeadConstants971Slice.AcsToDouble(65536));
        Assert.Equal(65536, VmPAcsHeadConstants971Slice.DoubleToAcs(1.0));
    }

    [Fact]
    public void HudmsgLayerMask_MatchesNativeShift()
    {
        Assert.Equal(0x0000F000u, VmPAcsHeadConstants971Slice.HudmsgLayerMask);
        Assert.Equal(12, VmPAcsHeadConstants971Slice.HudmsgLayerShift);
    }
}
