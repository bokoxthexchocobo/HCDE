namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfWaitTests
{
    static VmFScriptThinker.ThinkerState Thinker()
    {
        return new VmFScriptThinker.ThinkerState
        {
            RunningScripts = new VmFScriptScript.RunningScript(),
            LevelScript = new VmFScriptScript.FsScript(),
            CreateRunningScript = (t, s, i) => new VmFScriptScript.RunningScript { Trigger = t, Script = s, SavePoint = i },
            SectorsByTag = _ => Array.Empty<int>(),
        };
    }

    static VmFScriptParser.ParserContext Ctx(VmFScriptThinker.ThinkerState th, int argc, int intArg)
    {
        var argv = new VmFScriptParser.ScriptValue[] { new() { Type = VmFScriptParser.SvtInt, IntValue = intArg } };
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            IntValue = v => v.IntValue,
            Script = new VmFScriptScript.FsScript(),
            FraggleThinker = th,
            Rover = 4,
            ScriptError = _ => { },
        };
    }

    [Fact]
    public void SfWait_SchedulesDelay()
    {
        var th = Thinker();
        var ctx = Ctx(th, 1, 100);
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfWait(ctx));
        var run = th.RunningScripts!.Next;
        Assert.NotNull(run);
        Assert.Equal(VmFScriptScript.WtDelay, run!.WaitType);
        Assert.Equal(GameTicClock.TicRate, run.WaitData);
    }

    [Fact]
    public void SfTagWait_SchedulesTag()
    {
        var th = Thinker();
        var ctx = Ctx(th, 1, 7);
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfTagWait(ctx));
        Assert.Equal(7, th.RunningScripts!.Next!.WaitData);
        Assert.Equal(VmFScriptScript.WtTagwait, th.RunningScripts.Next.WaitType);
    }

    [Fact]
    public void SfScriptWait_SchedulesScript()
    {
        var th = Thinker();
        var ctx = Ctx(th, 1, 3);
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfScriptWait(ctx));
        Assert.Equal(VmFScriptScript.WtScriptwait, th.RunningScripts!.Next!.WaitType);
        Assert.Equal(3, th.RunningScripts.Next.WaitData);
    }

    [Fact]
    public void SfScriptWaitPre_SchedulesScript()
    {
        var th = Thinker();
        var ctx = Ctx(th, 1, 2);
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfScriptWaitPre(ctx));
        Assert.Equal(VmFScriptScript.WtScriptwaitpre, th.RunningScripts!.Next!.WaitType);
    }

    [Fact]
    public void SfStartScript_QueuesChild()
    {
        var th = Thinker();
        var child = new VmFScriptScript.FsScript { ScriptNum = 5 };
        th.LevelScript!.Children[5] = child;
        var ctx = Ctx(th, 1, 5);
        VmFScriptParser.SfStartScript(ctx);
        Assert.Same(child, th.RunningScripts!.Next!.Script);
    }

    [Fact]
    public void SfScriptRunning_DetectsRunner()
    {
        var th = Thinker();
        var child = new VmFScriptScript.FsScript { ScriptNum = 2 };
        th.RunningScripts!.Next = new VmFScriptScript.RunningScript { Script = child, Prev = th.RunningScripts };
        var ctx = Ctx(th, 1, 2);
        VmFScriptParser.SfScriptRunning(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
