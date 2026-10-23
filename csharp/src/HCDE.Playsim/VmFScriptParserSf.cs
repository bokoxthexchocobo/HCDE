namespace HCDE.Playsim;

/// <summary>Mid <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (509–721).</summary>
public static partial class VmFScriptParser
{
    public sealed class FsTerminatorException : Exception;

    /// <summary>Per-run capture for uncredited callees invoked only from this band (no forwarding delegates).</summary>
    public sealed partial class ParserContext
    {
        public VmPMobjLocalViewTouch.LocalViewQuery LocalView { get; init; } = new();
        public StartSoundContext? Sfx { get; init; }
        public List<(int PrintLevel, string Text)> FsPrintCapture { get; } = new();
        public List<string> FsMidPrintCapture { get; } = new();
        public List<(int LumpNum, string LumpName)> FsPendingIncludes { get; } = new();
        public bool FsExitLevelPending { get; set; }
        public float FsConMidtime { get; set; } = 2f;
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
        var lumpnum = ctx.CheckLumpForName(temp);
        if (lumpnum < 0)
        {
            ctx.ScriptError($"include lump '{temp}' not found!");
            return;
        }
        ctx.FsPendingIncludes.Add((lumpnum, temp));
    }

    public static void SfInput(ParserContext ctx) =>
        ctx.FsPrintCapture.Add((1, "input() function not available in Doom\n"));

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

    public static void SfExitLevel(ParserContext ctx) => ctx.FsExitLevelPending = true;

    public static void SfTip(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.Script.Trigger))
            ctx.FsMidPrintCapture.Add(GetFormatString(ctx, 0));
    }

    public static void SfTimedTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var saved = ctx.FsConMidtime;
        ctx.FsConMidtime = ctx.IntValue(ctx.TArgv[0]) / 100f;
        ctx.FsMidPrintCapture.Add(GetFormatString(ctx, 1));
        ctx.FsConMidtime = saved;
    }

    public static void SfPlayerTip(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.PlayerMo(plnum)))
            ctx.FsMidPrintCapture.Add(GetFormatString(ctx, 1));
    }

    public static void SfMessage(ParserContext ctx)
    {
        if (ctx.TArgc > 0 && ctx.Script?.Trigger != null
            && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.Script.Trigger))
            ctx.FsPrintCapture.Add((2, $"{GetFormatString(ctx, 0)}\n"));
    }

    public static void SfPlayerMsg(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var plnum = GetPlayerNum(ctx, ctx.TArgv[0]);
        if (plnum != -1 && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, ctx.PlayerMo(plnum)))
            ctx.FsPrintCapture.Add((2, $"{GetFormatString(ctx, 1)}\n"));
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
