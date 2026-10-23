namespace HCDE.Playsim;

/// <summary>Mid <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (509–721).</summary>
public static partial class VmFScriptParser
{
    public sealed class FsTerminatorException : Exception;

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

    static string IncludeTempFromArg(ParserContext ctx, ScriptValue arg)
    {
        if (arg.Type == SvtString)
        {
            var s = ctx.StringValue(arg);
            return s.Length > 8 ? s[..8] : s;
        }
        return ctx.IntValue(arg).ToString();
    }

    static bool TriggerScriptLocalView(ParserContext ctx) =>
        ctx.Script?.Trigger != null && ctx.TriggerLocalView();

    public static void SfInclude(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        ctx.ParseInclude(IncludeTempFromArg(ctx, ctx.TArgv[0]));
    }

    public static void SfInput(ParserContext ctx) =>
        ctx.PrintBold("input() function not available in Doom");

    public static void SfBeep(ParserContext ctx)
    {
        ctx.PlaySound();
    }

    public static void SfClock(ParserContext ctx, int gameTic, int ticRate)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = gameTic * 100 / ticRate;
    }

    public static void SfExitLevel(ParserContext ctx) => ctx.ExitLevel();

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && TriggerScriptLocalView(ctx))
            ctx.MidPrint(GetFormatString(ctx, 0));
    }

    public static void SfTimedTip(ParserContext ctx, ref float conMidtime)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var saved = conMidtime;
        conMidtime = ctx.IntValue(ctx.TArgv[0]) / 100f;
        ctx.MidPrint(GetFormatString(ctx, 1));
        conMidtime = saved;
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && ctx.PlayerLocalView(plnum))
            ctx.MidPrint(GetFormatString(ctx, 1));
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && TriggerScriptLocalView(ctx))
            ctx.PrintHigh(GetFormatString(ctx, 0));
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && ctx.PlayerLocalView(plnum))
            ctx.PrintHigh(GetFormatString(ctx, 1));
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
