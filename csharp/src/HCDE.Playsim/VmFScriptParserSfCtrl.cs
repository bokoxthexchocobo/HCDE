namespace HCDE.Playsim;

/// <summary>Control-flow and HUD <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (502–721).</summary>
public static partial class VmFScriptParser
{
    public sealed partial class ParserContext
    {
        public Action<string> ParseInclude { get; init; } = _ => { };
        public Action<string> PrintBold { get; init; } = _ => { };
        public Action ExitLevel { get; init; } = () => { };
        public Action<string> MidPrint { get; init; } = _ => { };
        public Func<object?, bool> TriggerLocalView { get; init; } = _ => false;
        public Func<int> GameTic { get; init; } = () => 0;
        public Action PlayChatBeep { get; init; } = () => { };
        public Func<double> ConMidTime { get; init; } = () => 0;
        public Action<double> SetConMidTime { get; init; } = _ => { };
    }

    public static void SfGoto(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        if (ctx.TArgv[0].Type != SvtLabel)
        {
            ctx.ScriptError("goto argument not a label\n");
            return;
        }
        ctx.Rover = ctx.LabelValue(ctx.TArgv[0]);
    }

    public static void SfReturn(ParserContext ctx) => throw new FsTerminatorException();

    public static void SfInclude(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        string name;
        if (ctx.TArgv[0].Type == SvtString)
            name = ctx.StringValue(ctx.TArgv[0]).Length > 8
                ? ctx.StringValue(ctx.TArgv[0])[..8]
                : ctx.StringValue(ctx.TArgv[0]);
        else
            name = ctx.IntValue(ctx.TArgv[0]).ToString();
        ctx.ParseInclude(name);
    }

    public static void SfInput(ParserContext ctx) =>
        ctx.PrintBold("input() function not available in Doom\n");

    public static void SfBeep(ParserContext ctx) => ctx.PlayChatBeep();

    public static void SfClock(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.GameTic() * 100 / GameTicClock.TicRate;
    }

    public static void SfExitLevel(ParserContext ctx) => ctx.ExitLevel();

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null && ctx.TriggerLocalView(ctx.Script.Trigger))
            ctx.MidPrint(GetFormatString(ctx, 0));
    }

    public static void SfTimedTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var saved = ctx.ConMidTime();
        ctx.SetConMidTime(ctx.IntValue(ctx.TArgv[0]) / 100.0);
        ctx.MidPrint(GetFormatString(ctx, 1));
        ctx.SetConMidTime(saved);
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum == -1)
            return;
        var mo = ctx.PlayerMo(plnum);
        if (mo != null && ctx.TriggerLocalView(mo))
            ctx.MidPrint(GetFormatString(ctx, 1));
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null && ctx.TriggerLocalView(ctx.Script.Trigger))
            ctx.PrintHigh(GetFormatString(ctx, 0));
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum == -1)
            return;
        var mo = ctx.PlayerMo(plnum);
        if (mo != null && ctx.TriggerLocalView(mo))
            ctx.PrintHigh(GetFormatString(ctx, 1));
    }

    public static void SfPlayerInGame(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum == -1)
            return;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.PlayerInGame(plnum) ? 1 : 0;
    }
}
