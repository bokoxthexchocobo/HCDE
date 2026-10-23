namespace HCDE.Playsim;

/// <summary>Control-flow and HUD <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (502–721).</summary>
public static partial class VmFScriptParser
{
    const float AttnIdle = 1.001f;

    public static bool MobjLocalView(ParserContext ctx, object? mobj) =>
        mobj != null && ctx.LocalView != null && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, mobj);

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

    public static void SfBeep(ParserContext ctx)
    {
        var soundCtx = new StartSoundContext
        {
            Type = StartSoundContext.SourceNone,
            Channel = StartSoundContext.ChanAuto,
            SoundId = ctx.FindSound("misc/chat"),
            Volume = 1f,
            Attenuation = AttnIdle,
            SfxVolume = 1f,
            SfxAttenuation = 1f,
            IsValidSoundId = id => id != 0,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
            StartSound2D = _ => new object(),
        };
        ctx.StartSoundEngine(soundCtx);
    }

    public static void SfClock(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.GameTic() * 100 / GameTicClock.TicRate;
    }

    public static void SfExitLevel(ParserContext ctx)
    {
        ctx.Simulation?.MarkExited(secret: false);
    }

    public static void SfInput(ParserContext ctx)
    {
        ctx.PrintHigh("input() function not available in Doom\n");
    }

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null && MobjLocalView(ctx, ctx.Script.Trigger))
            ctx.PrintHigh($"{GetFormatString(ctx, 0)}\n");
    }

    public static void SfTimedTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        if (ctx.Script?.Trigger == null || !MobjLocalView(ctx, ctx.Script.Trigger))
            return;
        ctx.PrintHigh($"{GetFormatString(ctx, 1)}\n");
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum == -1)
            return;
        var mo = ctx.PlayerMo(plnum);
        if (mo != null && MobjLocalView(ctx, mo))
            ctx.PrintHigh($"{GetFormatString(ctx, 1)}\n");
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null && MobjLocalView(ctx, ctx.Script.Trigger))
            ctx.PrintHigh($"{GetFormatString(ctx, 0)}\n");
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum == -1)
            return;
        var mo = ctx.PlayerMo(plnum);
        if (mo != null && MobjLocalView(ctx, mo))
            ctx.PrintHigh($"{GetFormatString(ctx, 1)}\n");
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
