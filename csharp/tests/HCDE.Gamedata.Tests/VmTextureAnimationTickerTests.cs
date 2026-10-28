namespace HCDE.Gamedata.Tests;

public class VmTextureAnimationTickerTests
{
    [Fact]
    public void AdvanceFrame_ForwardWraps()
    {
        var anim = new VmAnimationState { NumFrames = 3, CurFrame = 2 };
        VmTextureAnimationTicker.AdvanceFrame(anim);
        Assert.Equal(0, anim.CurFrame);
    }

    [Fact]
    public void UpdateUntil_AdvancesWhenPastSwitchTime()
    {
        var anim = new VmAnimationState
        {
            NumFrames = 2,
            SpeedMin = 10,
            AnimType = "Forward",
            SwitchTimeMs = 100
        };
        VmTextureAnimationTicker.UpdateUntil(150, anim);
        Assert.Equal(1, anim.CurFrame);
        Assert.True(anim.SwitchTimeMs > 150);
    }

    [Fact]
    public void ComputeSwitchTime_AddsRandomRange()
    {
        var anim = new VmAnimationState { SpeedMin = 5, SpeedRange = 10 };
        var t = VmTextureAnimationTicker.ComputeSwitchTime(100, anim, new Random(1));
        Assert.InRange(t, 105ul, 115ul);
    }
}
