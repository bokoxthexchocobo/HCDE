namespace HCDE.Playsim;

/// <summary><c>FParser</c> helpers and early script functions from <c>fragglescript/t_func.cpp</c> (219–501).</summary>
public static partial class VmFScriptParser
{
    public const int SvtString = 1;
    public const int SvtInt = 2;
    public const int SvtMobj = 3;
    public const int SvtLabel = 6;
    public const int StLoop = 1;
    public const int MaxPlayers = 8;

    public sealed partial class ScriptValue
    {
        public int Type { get; set; }
        public string String { get; set; } = "";
        public int IntValue { get; set; }
        public object? Mobj { get; set; }
    }

    public sealed class FsSection
    {
        public int Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public FsSection? Next { get; set; }
    }

    public sealed partial class ParserContext
    {
        public VmFScriptScript.FsScript? Script { get; set; }
        public int Rover { get; set; }
        public int TArgc { get; set; }
        public ScriptValue[] TArgv { get; init; } = Array.Empty<ScriptValue>();
        public ScriptValue TReturn { get; } = new();
        public string TFunc { get; set; } = "";
        public object?[] ActorTypes { get; init; } = Array.Empty<object?>();
        public Func<string, object?> FindActorClass { get; init; } = _ => null;
        public Func<object?, object?> GetActorClass { get; init; } = _ => null;
        public Func<ScriptValue, object?> ActorValue { get; init; } = _ => null;
        public Func<ScriptValue, int> IntValue { get; init; } = _ => 0;
        public Func<ScriptValue, string> StringValue { get; init; } = _ => "";
        public Func<int, bool> PlayerInGame { get; init; } = _ => false;
        public Func<object?, int> PlayerNum { get; init; } = _ => -1;
        public Func<int, object?> PlayerMo { get; init; } = _ => null;
        public VmPMobjLocalViewTouch.LocalViewQuery? LocalView { get; init; }
        public Func<int> GameTic { get; init; } = () => 0;
        public Func<StartSoundContext, object?> StartSoundEngine { get; init; } = VmSoundEngineStartSound.StartSound;
        public Action<string> ScriptError { get; init; } = _ => { };
        public Action<string> PrintHigh { get; init; } = _ => { };
        public Func<int> RandomScript { get; init; } = () => 0;
        public Func<string, int> FindSound { get; init; } = _ => 0;
        public Func<string, bool> SoundLumpExists { get; init; } = _ => false;
        public Func<string, string, int> AddSound { get; init; } = (_, _) => 0;
        public Action HashSounds { get; init; } = () => { };
        public bool GameDoomStrifeChex { get; set; }
        public Func<string, int> CheckLumpForName { get; init; } = _ => -1;
        public Func<ScriptValue, int, string> FormatStringFromArgs { get; init; } = (_, _) => "";
        public Func<FsSection, int> SectionEnd { get; init; } = _ => 0;
        public Func<ScriptValue, int> LabelValue { get; init; } = _ => 0;
    }

    public static object? GetMobjType(ParserContext ctx, ScriptValue arg)
    {
        if (arg.Type == SvtString)
        {
            var pclass = ctx.FindActorClass(arg.String);
            if (pclass == null)
                ctx.ScriptError($"unknown object type: {arg.String}");
            return pclass;
        }
        if (arg.Type == SvtMobj)
        {
            var mo = ctx.ActorValue(arg);
            return mo == null ? null : ctx.GetActorClass(mo);
        }
        var objType = ctx.IntValue(arg);
        if (objType >= 0 && objType < ctx.ActorTypes.Length)
        {
            var pclass = ctx.ActorTypes[objType];
            if (pclass == null)
                ctx.ScriptError($"unknown object type: {objType}");
            return pclass;
        }
        ctx.ScriptError($"unknown object type: {objType}");
        return null;
    }

    public static int GetPlayerNum(ParserContext ctx, ScriptValue arg)
    {
        if (arg.Type == SvtMobj)
        {
            var mo = ctx.ActorValue(arg);
            if (mo == null || ctx.PlayerNum(mo) < 0)
                return -1;
            return ctx.PlayerNum(mo);
        }
        var playernum = ctx.IntValue(arg);
        if (playernum < 0 || playernum > MaxPlayers)
            return -1;
        if (!ctx.PlayerInGame(playernum))
            return -1;
        return playernum;
    }

    public static object? GetPlayerActor(ParserContext ctx, ScriptValue arg)
    {
        var num = GetPlayerNum(ctx, arg);
        return num == -1 ? null : ctx.PlayerMo(num);
    }

    public static object? ClassType(ParserContext ctx, ScriptValue arg) =>
        ctx.FindActorClass(ctx.StringValue(arg));

    public static object? GetAmmo(ParserContext ctx, ScriptValue t)
    {
        string p;
        if (t.Type == SvtString)
            p = ctx.StringValue(t);
        else
        {
            var ammoNum = ctx.IntValue(t);
            if (ammoNum < 0 || ammoNum >= 4)
            {
                ctx.ScriptError($"ammo number out of range: {ammoNum}");
                return null;
            }
            p = ammoNum switch
            {
                0 => "Clip",
                1 => "Shell",
                2 => "Cell",
                _ => "RocketAmmo",
            };
        }
        var am = ctx.FindActorClass(p);
        if (am == null)
            ctx.ScriptError($"unknown ammo type : {p}");
        return am;
    }

    public static int FindSound(ParserContext ctx, string name)
    {
        var so = ctx.FindSound(name);
        if (so != 0)
            return so;
        var buffer = name;
        if (ctx.GameDoomStrifeChex)
        {
            var dsName = $"DS{name}";
            if (ctx.CheckLumpForName(dsName) < 0)
                buffer = name;
            else
                buffer = dsName;
        }
        else if (ctx.CheckLumpForName(buffer) < 0)
            buffer = $"DS{name}";
        var id = ctx.AddSound(name, buffer);
        ctx.HashSounds();
        return id;
    }

    public static string GetFormatString(ParserContext ctx, int startArg) =>
        ctx.FormatStringFromArgs(ctx.TArgv[startArg], ctx.TArgc - startArg);

    public static bool CheckArgs(ParserContext ctx, int cnt)
    {
        if (ctx.TArgc < cnt)
        {
            ctx.ScriptError($"Insufficient parameters for '{ctx.TFunc}'");
            return false;
        }
        return true;
    }

    public static void SfPrint(ParserContext ctx) =>
        ctx.PrintHigh(GetFormatString(ctx, 0));

    public static void SfRnd(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.RandomScript();
    }

    public static FsSection? LoopingSection(ParserContext ctx)
    {
        FsSection? best = null;
        var roverIndex = ctx.Rover;
        for (var n = 0; n < VmFScriptScript.SectionSlots; n++)
        {
            var current = ctx.Script?.Sections[n] as FsSection;
            while (current != null)
            {
                if (current.Type == StLoop &&
                    roverIndex >= current.StartIndex &&
                    roverIndex <= current.EndIndex &&
                    (best == null || current.StartIndex > best.StartIndex))
                    best = current;
                current = current.Next;
            }
        }
        return best;
    }

    public static void SfContinue(ParserContext ctx)
    {
        var section = LoopingSection(ctx);
        if (section == null)
        {
            ctx.ScriptError("continue() not in loop");
            return;
        }
        ctx.Rover = ctx.SectionEnd(section);
    }

    public static void SfBreak(ParserContext ctx)
    {
        var section = LoopingSection(ctx);
        if (section == null)
        {
            ctx.ScriptError("break() not in loop");
            return;
        }
        ctx.Rover = ctx.SectionEnd(section) + 1;
    }
}
