namespace HCDE.Client.Tests;

public class VmDMainDoomLoopTests
{
    [Fact]
    public void ShouldIgnoreRestartDuringNetgame()
    {
        Assert.True(VmDMainDoomLoop.ShouldIgnoreRestartDuringNetgame(true, true, false));
    }

    [Fact]
    public void ShouldStartFrame_WhenGameticAdvances()
    {
        Assert.True(VmDMainDoomLoop.ShouldStartFrame(2, 1));
    }

    [Fact]
    public void ShouldExitEndgame_OnlyWhenNotNetgame()
    {
        Assert.True(VmDMainDoomLoop.ShouldAllowEndgameCcmd(false));
    }
}

public class VmDMainStrifeDemoTests
{
    [Fact]
    public void ResolveTitlePageTicDuration()
    {
        Assert.Equal(210, VmDMainStrifeDemo.ResolveTitlePageTicDuration(35));
    }
}

public class VmDMainPageDrawerTests
{
    [Fact]
    public void ShouldAdvanceDemoOnPageTick()
    {
        Assert.True(VmDMainPageDrawer.ShouldAdvanceDemoOnPageTick(-1));
    }
}

public class VmDMainRconPollTests
{
    [Fact]
    public void ShouldRestartListener_WhenPortChanges()
    {
        Assert.True(VmDMainRconPoll.ShouldRestartListenerWhenPortChanges(1, true, 9, 8));
    }

    [Fact]
    public void ShouldRejectClient_WhenNoSlots()
    {
        Assert.True(VmDMainRconPoll.ShouldRejectClientWhenNoSlots(false));
    }
}

public class VmDMainCompatCallbacksTests
{
    [Fact]
    public void IsDmflags3FlagSet_DetectsClip()
    {
        Assert.True(VmDMainCompatCallbacks.IsDmflags3FlagSet(1, VmDMainCompatCallbacks.Df3NoPlayerClip));
    }
}
