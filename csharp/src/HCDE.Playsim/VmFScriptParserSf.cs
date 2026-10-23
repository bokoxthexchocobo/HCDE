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
        ParseIncludeLump(ctx, temp);
    }

    public static void SfInput(ParserContext ctx) =>
        Printf(ctx, FsPrintBold, "input() function not available in Doom\n");

    public static void SfBeep(ParserContext ctx) => PlayChatBeep(ctx);

    public static void SfClock(ParserContext ctx, int gameTic, int ticRate)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = gameTic * 100 / ticRate;
    }

    public static void SfExitLevel(ParserContext ctx) => RequestExitLevel(ctx);

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && ActorCheckLocalView(ctx, ctx.Script.Trigger))
            MidPrint(ctx, GetFormatString(ctx, 0));
    }

    public static void SfTimedTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var saved = ctx.ConMidtime;
        ctx.ConMidtime = ctx.IntValue(ctx.TArgv[0]) / 100f;
        MidPrint(ctx, GetFormatString(ctx, 1));
        ctx.ConMidtime = saved;
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && ActorCheckLocalView(ctx, ctx.PlayerMo(plnum)))
            MidPrint(ctx, GetFormatString(ctx, 1));
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && ActorCheckLocalView(ctx, ctx.Script.Trigger))
            Printf(ctx, FsPrintHigh, $"{GetFormatString(ctx, 0)}\n");
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && ActorCheckLocalView(ctx, ctx.PlayerMo(plnum)))
            Printf(ctx, FsPrintHigh, $"{GetFormatString(ctx, 1)}\n");
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
