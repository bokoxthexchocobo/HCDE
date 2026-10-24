namespace HCDE.Playsim;

/// <summary>Script wait/start <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3850–4008).</summary>
public static partial class VmFScriptParser
{
    public static void SfWait(ParserContext ctx)
    {
        if (ctx.TArgc != 1)
        {
            ctx.ScriptError("incorrect arguments to function\n");
            return;
        }

        var runscr = SaveCurrentScript(ctx);
        if (runscr == null)
            return;
        runscr.WaitType = VmFScriptScript.WtDelay;
        runscr.WaitData = ctx.IntValue(ctx.TArgv[0]) * GameTicClock.TicRate / 100;
        throw new FsTerminatorException();
    }

    public static void SfTagWait(ParserContext ctx)
    {
        if (ctx.TArgc != 1)
        {
            ctx.ScriptError("incorrect arguments to function\n");
            return;
        }

        var runscr = SaveCurrentScript(ctx);
        if (runscr == null)
            return;
        runscr.WaitType = VmFScriptScript.WtTagwait;
        runscr.WaitData = ctx.IntValue(ctx.TArgv[0]);
        throw new FsTerminatorException();
    }

    public static void SfScriptWait(ParserContext ctx)
    {
        if (ctx.TArgc != 1)
        {
            ctx.ScriptError("insufficient arguments to function\n");
            return;
        }

        var runscr = SaveCurrentScript(ctx);
        if (runscr == null)
            return;
        runscr.WaitType = VmFScriptScript.WtScriptwait;
        runscr.WaitData = ctx.IntValue(ctx.TArgv[0]);
        throw new FsTerminatorException();
    }

    public static void SfScriptWaitPre(ParserContext ctx)
    {
        if (ctx.TArgc != 1)
        {
            ctx.ScriptError("insufficient arguments to function\n");
            return;
        }

        var runscr = SaveCurrentScript(ctx);
        if (runscr == null)
            return;
        runscr.WaitType = VmFScriptScript.WtScriptwaitpre;
        runscr.WaitData = ctx.IntValue(ctx.TArgv[0]);
        throw new FsTerminatorException();
    }

    public static void SfStartScript(ParserContext ctx)
    {
        if (ctx.TArgc != 1)
        {
            ctx.ScriptError("incorrect arguments to function\n");
            return;
        }

        var snum = ctx.IntValue(ctx.TArgv[0]);
        if (snum < 0 || snum >= VmFScriptScript.MaxScripts)
        {
            ctx.ScriptError($"script number {snum} out of range\n");
            return;
        }

        var th = ctx.FraggleThinker;
        if (th?.LevelScript == null)
            return;
        var script = th.LevelScript.Children[snum];
        if (script == null)
        {
            ctx.ScriptError($"script {snum} not defined\n");
            return;
        }

        var runscr = th.CreateRunningScript(ctx.Script?.Trigger, script, 0);
        VmFScriptThinker.AddRunningScript(th, runscr);
    }

    public static void SfScriptRunning(ParserContext ctx)
    {
        if (ctx.TArgc < 1)
        {
            ctx.ScriptError("not enough arguments to function\n");
            return;
        }

        var snum = ctx.IntValue(ctx.TArgv[0]);
        var th = ctx.FraggleThinker;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
        if (th?.RunningScripts == null)
            return;
        for (var current = th.RunningScripts.Next; current != null; current = current.Next)
        {
            if (current.Script?.ScriptNum == snum)
            {
                ctx.TReturn.IntValue = 1;
                return;
            }
        }
    }
}
