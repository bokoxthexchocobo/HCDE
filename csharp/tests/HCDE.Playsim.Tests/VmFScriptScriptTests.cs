using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptScriptTests
{
    [Fact]
    public void ClearChildren_DestroyAllSlots()
    {
        var destroyed = 0;
        var script = new VmFScriptScript.FsScript { DestroyChild = _ => destroyed++ };
        script.Children[0] = new VmFScriptScript.FsScript();
        script.Children[3] = new VmFScriptScript.FsScript();
        VmFScriptScript.ClearChildren(script);
        Assert.Equal(2, destroyed);
        Assert.Null(script.Children[0]);
    }

    [Fact]
    public void InitNewScript_ClearsArraysAndDefaults()
    {
        var script = new VmFScriptScript.FsScript { ScriptNum = 5, Len = 9 };
        script.Children[1] = new VmFScriptScript.FsScript();
        VmFScriptScript.InitNewScript(script);
        Assert.Equal(-1, script.ScriptNum);
        Assert.Equal(0, script.Len);
        Assert.Null(script.Children[1]);
    }

    [Fact]
    public void ParseScript_OutOfRange_PrintsAndSkipsBody()
    {
        var ran = false;
        var script = new VmFScriptScript.FsScript
        {
            Len = 10,
            ScriptNum = 2,
            ParseScriptBody = (_, _) => ran = true,
        };
        var msg = "";
        VmFScriptScript.ParseScript(script, 20, new VmFScriptScript.FraggleThinkerHost(), s => msg = s, _ => { });
        Assert.Contains("outside script", msg);
        Assert.False(ran);
    }

    [Fact]
    public void InitRunningScript_IndexZero_TrimsToLabels()
    {
        var owner = new VmFScriptScript.FsScript();
        var label = new VmFScriptScript.FsVariable { Type = VmFScriptScript.SvtLabel };
        var temp = new VmFScriptScript.FsVariable { Type = 0, Next = label };
        owner.Variables[0] = temp;
        var run = new VmFScriptScript.RunningScript();
        VmFScriptScript.InitRunningScript(run, null, owner, 0);
        Assert.Same(label, run.Variables[0]);
    }
}
