namespace HCDE.Playsim.Tests;

public class VmPAcsCallfuncGameplay1023SliceTests
{
    [Fact]
    public void NamedExecuteScriptIndex_NegatesValidNameIndex()
    {
        var idx = VmPAcsCallfuncGameplay1023Slice.NamedExecuteScriptIndex(
            "foo", name => name == "foo" ? 42 : 0);
        Assert.Equal(-42, idx);
    }

    [Fact]
    public void UniqueTidStartArg_DefaultsWhenEmpty()
    {
        Assert.Equal(0, VmPAcsCallfuncGameplay1023Slice.UniqueTidStartArg(Array.Empty<int>(), 0));
        Assert.Equal(7, VmPAcsCallfuncGameplay1023Slice.UniqueTidStartArg(new[] { 7, -1 }, 2));
        Assert.Equal(0, VmPAcsCallfuncGameplay1023Slice.UniqueTidMinInUseArg(new[] { 1, -3 }, 2));
        Assert.Equal(5, VmPAcsCallfuncGameplay1023Slice.UniqueTidMinInUseArg(new[] { 1, 5 }, 2));
    }

    [Fact]
    public void IntegerSqrtRoundDown_MatchesNativeRoundDown()
    {
        Assert.Equal(3, VmPAcsCallfuncGameplay1023Slice.IntegerSqrtRoundDown(10));
        Assert.Equal(4, VmPAcsCallfuncGameplay1023Slice.IntegerSqrtRoundDown(17));
    }

    [Fact]
    public void FixedSqrtAndVectorLength_Use65536Scale()
    {
        Assert.Equal(65536, VmPAcsCallfuncGameplay1023Slice.FixedSqrtFromAcs(65536));
        var len = VmPAcsCallfuncGameplay1023Slice.VectorLengthFixed(65536, 0);
        Assert.Equal(65536, len);
    }

    [Fact]
    public void BuildHudClipRect_DefaultsHandleAspectTrue()
    {
        var state = VmPAcsCallfuncGameplay1023Slice.BuildHudClipRect(new[] { 1, 2, 3, 4 }, 4);
        Assert.Equal(new VmPAcsCallfuncGameplay1023Slice.HudClipRectState(1, 2, 3, 4, 0, true), state);
        var off = VmPAcsCallfuncGameplay1023Slice.BuildHudClipRect(new[] { 0, 0, 0, 0, 0, 0 }, 6);
        Assert.False(off.HandleAspect);
    }

    [Fact]
    public void LineAttackFhFlags_MapsNativeBits()
    {
        var flags = VmPAcsCallfuncGameplay1023Slice.FhfNorandompuffz
                    | VmPAcsCallfuncGameplay1023Slice.FhfNoimpactdecal;
        var fh = VmPAcsCallfuncGameplay1023Slice.LineAttackFhFlags(flags);
        Assert.Equal(
            VmPAcsCallfuncGameplay1023Slice.LafNorandompuffz
            | VmPAcsCallfuncGameplay1023Slice.LafNoimpactdecal,
            fh);
    }

    [Fact]
    public void BuildPlaySoundArgs_DefaultChannelAndVolume()
    {
        var args = VmPAcsCallfuncGameplay1023Slice.BuildPlaySoundArgs(new[] { 0, 0 }, 2);
        Assert.Equal(VmPAcsCallfuncGameplay1023Slice.ChanBody, args.Channel);
        Assert.Equal(1f, args.Volume);
        Assert.True(VmPAcsCallfuncGameplay1023Slice.PlaySoundUsesActivator(0));
    }
}
