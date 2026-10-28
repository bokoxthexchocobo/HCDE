using System.Globalization;

namespace HCDE.Playsim;

/// <summary>Math and query <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (2242–2393).</summary>
public static partial class VmFScriptParser
{
    const double FsFracUnit = 65536.0;

    /// <summary>Native <c>fixedvalue</c> from <c>t_variable.cpp</c> (58–63).</summary>
    static double FsFixValue(ParserContext ctx, ScriptValue v) =>
        v.Type == SvtFixed ? v.FixedValue
        : v.Type == SvtString ? double.Parse(ctx.StringValue(v), CultureInfo.InvariantCulture) * FsFracUnit
        : v.Type == SvtMobj ? -FsFracUnit
        : ctx.IntValue(v) * FsFracUnit;

    static void SetReturnFsFix(ParserContext ctx, double fsfix)
    {
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = fsfix;
    }

    public static void SfMax(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var n1 = FsFixValue(ctx, ctx.TArgv[0]);
        var n2 = FsFixValue(ctx, ctx.TArgv[1]);
        SetReturnFsFix(ctx, n1 > n2 ? n1 : n2);
    }

    public static void SfMin(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var n1 = FsFixValue(ctx, ctx.TArgv[0]);
        var n2 = FsFixValue(ctx, ctx.TArgv[1]);
        SetReturnFsFix(ctx, n1 < n2 ? n1 : n2);
    }

    public static void SfAbs(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var n1 = FsFixValue(ctx, ctx.TArgv[0]);
        SetReturnFsFix(ctx, n1 < 0 ? -n1 : n1);
    }

    public static void SfGameskill(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = (ctx.Simulation?.Skill ?? 0) + 1;
    }

    public static void SfGamemode(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        var mode = ctx.Simulation?.GameMode ?? SpawnGameMode.Single;
        ctx.TReturn.IntValue = mode switch
        {
            SpawnGameMode.Single => 0,
            SpawnGameMode.Cooperative => 1,
            SpawnGameMode.Deathmatch => 2,
            _ => 0,
        };
    }

    public static void SfIsPlayerObj(ParserContext ctx)
    {
        object? mo;
        if (ctx.TArgc > 0)
            mo = ctx.ActorValue(ctx.TArgv[0]);
        else
            mo = ctx.Script?.Trigger;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = mo is PlayerPawn || (mo != null && ctx.PlayerNum(mo) >= 0) ? 1 : 0;
    }
}
