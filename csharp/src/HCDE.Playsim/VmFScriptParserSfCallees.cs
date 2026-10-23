namespace HCDE.Playsim;

/// <summary>Native callees for chunk **838** <c>SF_*</c> (no <c>ParserContext</c> forwarding delegates).</summary>
public static partial class VmFScriptParser
{
    public const int FsPrintBold = 1;
    public const int FsPrintHigh = 2;
    public const int ChanAuto = 0;
    public const float AttnIdle = 1f;

    public sealed class IncludeLumpRequest
    {
        public int LumpNum { get; init; }
        public string LumpName { get; init; } = "";
    }

    public sealed partial class ParserContext
    {
        public VmPMobjLocalViewTouch.LocalViewQuery LocalView { get; init; } = new();
        public List<(int Level, string Text)> PrintLines { get; } = new();
        public List<string> MidPrintLines { get; } = new();
        public List<IncludeLumpRequest> IncludeLumpRequests { get; } = new();
        public bool ExitLevelRequested { get; set; }
        public int ExitLevelSecret { get; set; }
        public bool ExitLevelHub { get; set; }
        public float ConMidtime { get; set; } = 2f;
        public StartSoundContext? Sfx { get; init; }
    }

    public static void Printf(ParserContext ctx, int printLevel, string text) =>
        ctx.PrintLines.Add((printLevel, text));

    public static void MidPrint(ParserContext ctx, string text)
    {
        if (text.Length > 0)
            ctx.MidPrintLines.Add(text);
    }

    public static bool ActorCheckLocalView(ParserContext ctx, object? actor) =>
        actor != null && VmPMobjLocalViewTouch.CheckLocalView(ctx.LocalView, actor);

    public static void ParseIncludeLump(ParserContext ctx, string lumpname)
    {
        if (ctx.Script == null)
            return;
        var lumpnum = ctx.CheckLumpForName(lumpname);
        if (lumpnum < 0)
        {
            ctx.ScriptError($"include lump '{lumpname}' not found!");
            return;
        }
        ctx.IncludeLumpRequests.Add(new IncludeLumpRequest
        {
            LumpNum = lumpnum,
            LumpName = lumpname,
        });
    }

    public static void PlayChatBeep(ParserContext ctx)
    {
        var soundId = FindSound(ctx, "misc/chat");
        var sfx = ctx.Sfx;
        if (sfx == null || soundId == 0)
            return;
        sfx.SoundId = soundId;
        sfx.Channel = ChanAuto;
        sfx.Volume = 1f;
        sfx.Attenuation = AttnIdle;
        VmSoundEngineStartSound.StartSound(sfx);
    }

    public static void RequestExitLevel(ParserContext ctx)
    {
        ctx.ExitLevelSecret = 0;
        ctx.ExitLevelHub = false;
        ctx.ExitLevelRequested = true;
    }
}
