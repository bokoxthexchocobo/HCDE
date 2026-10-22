using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptThinkerTests
{
    [Fact]
    public void WaitFinished_Delay_CountsDown()
    {
        var th = new VmFScriptThinker.ThinkerState();
        var script = new VmFScriptScript.RunningScript { WaitType = VmFScriptScript.WtDelay, WaitData = 2 };
        Assert.False(VmFScriptThinker.WaitFinished(th, script));
        Assert.True(VmFScriptThinker.WaitFinished(th, script));
    }

    [Fact]
    public void RunScript_InvalidNum_ReturnsFalse()
    {
        var th = new VmFScriptThinker.ThinkerState { LevelScript = new VmFScriptScript.FsScript() };
        Assert.False(VmFScriptThinker.RunScript(th, -1, null));
        Assert.False(VmFScriptThinker.RunScript(th, VmFScriptScript.MaxScripts, null));
    }

    [Fact]
    public void RunScript_QueuesChildScript()
    {
        var th = new VmFScriptThinker.ThinkerState
        {
            LevelScript = new VmFScriptScript.FsScript(),
            RunningScripts = new VmFScriptScript.RunningScript(),
            CreateRunningScript = (t, s, i) => new VmFScriptScript.RunningScript { Trigger = t, Script = s, SavePoint = i },
        };
        var child = new VmFScriptScript.FsScript();
        th.LevelScript.Children[3] = child;
        Assert.True(VmFScriptThinker.RunScript(th, 3, new object()));
        Assert.NotNull(th.RunningScripts.Next);
        Assert.Same(child, th.RunningScripts.Next!.Script);
    }
}
