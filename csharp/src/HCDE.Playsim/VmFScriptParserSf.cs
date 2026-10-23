namespace HCDE.Playsim;

/// <summary>Mid <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (509–721).</summary>
public static partial class VmFScriptParser
{
    public sealed class FsTerminatorException : Exception;

    public sealed partial class ParserContext
    {
        public VmPMobjLocalViewTouch.LocalViewQuery LocalView { get; init; } = new();
        public StartSoundContext? Sfx { get; init; }
    }

    public static void SfGoto(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        if (ctx.TArgv[0].Type != SvtLabel)
        {
            ctx.ScriptError("goto argument not a label");
            return;
        }
        ctx.Rover = ctx.LabelValue(ctx.TArgv[0]);
    }

    public static void SfReturn() => throw new FsTerminatorException();

    public static void SfInclude(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        string temp;
        if (ctx.TArgv[0].Type == SvtString)
        {
            var s = ctx.StringValue(ctx.TArgv[0]);
            temp = s.Length > 8 ? s[..8] : s;
        }
        else
            temp = ctx.IntValue(ctx.TArgv[0]).ToString();
        if (ctx.Script == null)
            return;
        _ = temp;
        // Gap: Script->ParseInclude(Level, temp) — DFsScript::ParseInclude (t_prepro.cpp 401–425), call at t_func.cpp 557.
    }

    public static void SfInput(ParserContext ctx)
    {
        // Gap: Printf(PRINT_BOLD, "input() function not available in Doom\\n") — t_func.cpp 569.
    }

    public static void SfBeep(ParserContext ctx)
    {
        var soundId = FindSound(ctx, "misc/chat");
        var sfx = ctx.Sfx;
        if (sfx == null || soundId == 0)
            return;
        sfx.SoundId = soundId;
        sfx.Channel = 0;
        sfx.Volume = 1f;
        sfx.Attenuation = 1f;
        VmSoundEngineStartSound.StartSound(sfx);
    }

    public static void SfClock(ParserContext ctx, int gameTic, int ticRate)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = gameTic * 100 / ticRate;
    }

    public static void SfExitLevel(ParserContext ctx)
    {
        // Gap: Level->ExitLevel(0, false) — t_func.cpp 605 (FLevelLocals::ExitLevel).
    }

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.Script.Trigger))
        {
            _ = GetFormatString(ctx, 0);
            // Gap: C_MidPrint(nullptr, …) — t_func.cpp 619.
        }
    }

    public static void SfTimedTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        _ = ctx.IntValue(ctx.TArgv[0]);
        _ = GetFormatString(ctx, 1);
        // Gap: con_midtime save/set/restore + C_MidPrint — t_func.cpp 637–639.
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        var mo = plnum != -1 ? ctx.PlayerMo(plnum) : null;
        if (mo != null && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, mo))
        {
            _ = GetFormatString(ctx, 1);
            // Gap: C_MidPrint(nullptr, …) — t_func.cpp 658.
        }
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.Script.Trigger))
            ctx.PrintHigh($"{GetFormatString(ctx, 0)}\n");
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        var mo = plnum != -1 ? ctx.PlayerMo(plnum) : null;
        if (mo != null && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, mo))
            ctx.PrintHigh($"{GetFormatString(ctx, 1)}\n");
    }

    public static void SfPlayerInGame(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = ctx.PlayerInGame(plnum) ? 1 : 0;
        }
    }
}
