namespace HCDE.Playsim;

/// <summary>Player and spawn <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (722–869).</summary>
public static partial class VmFScriptParser
{
    public sealed partial class ParserContext
    {
        public Func<object?, object?> TriggerPlayer { get; init; } = _ => null;
        public Func<int, string> PlayerDisplayName { get; init; } = _ => "";
        public Func<object?, double, double, double, object?> SpawnActor { get; init; } = (_, _, _, _) => null;
        public Func<object?, bool> TestMobjLocation { get; init; } = _ => true;
        public Func<double, double, double> SectorFloorZ { get; init; } = (_, _) => 0;
        public Action<object?> DestroyFailedSpawn { get; init; } = _ => { };
        public Action<object?, double> SetActorYaw { get; init; } = (_, _) => { };
        public double OnFloorZ { get; init; }
        public bool FsNoCheckPosition { get; init; }
        public Func<ScriptValue, double> FloatValue { get; init; } = v => v.IntValue;
    }

    static int ResolveOptionalPlayerNum(ParserContext ctx)
    {
        if (ctx.TArgc > 0)
            return GetPlayerNum(ctx, ctx.TArgv[0]);
        var trigger = ctx.Script?.Trigger;
        if (trigger == null)
            return -1;
        var player = ctx.TriggerPlayer(trigger);
        if (player == null)
            return -1;
        return ctx.PlayerNum(player);
    }

    public static void SfPlayerName(ParserContext ctx)
    {
        var plnum = ResolveOptionalPlayerNum(ctx);
        if (plnum != -1)
        {
            ctx.TReturn.Type = SvtString;
            ctx.TReturn.String = ctx.PlayerDisplayName(plnum);
        }
        else
            ctx.ScriptError("script not started by player\n");
    }

    public static void SfPlayerObj(ParserContext ctx)
    {
        var plnum = ResolveOptionalPlayerNum(ctx);
        if (plnum != -1)
        {
            ctx.TReturn.Type = SvtMobj;
            ctx.TReturn.Mobj = ctx.PlayerMo(plnum);
        }
        else
            ctx.ScriptError("script not started by player\n");
    }

    public static void SfPlayer(ParserContext ctx)
    {
        object? mo;
        if (ctx.TArgc > 0)
            mo = ctx.ActorValue(ctx.TArgv[0]);
        else
            mo = ctx.Script?.Trigger;

        ctx.TReturn.Type = SvtInt;
        if (mo != null && ctx.PlayerNum(mo) >= 0)
            ctx.TReturn.IntValue = ctx.PlayerNum(mo);
        else
            ctx.TReturn.IntValue = -1;
    }

    public static void SfSpawn(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 3))
            return;
        var pclass = GetMobjType(ctx, ctx.TArgv[0]);
        if (pclass == null)
            return;

        var x = ctx.FloatValue(ctx.TArgv[1]);
        var y = ctx.FloatValue(ctx.TArgv[2]);
        double z;
        if (ctx.TArgc >= 5)
        {
            z = ctx.FloatValue(ctx.TArgv[4]);
            if (ctx.TArgc > 5 && ctx.IntValue(ctx.TArgv[5]) != 0)
                z += ctx.SectorFloorZ(x, y);
        }
        else
            z = ctx.OnFloorZ;

        var angle = ctx.TArgc >= 4 ? ctx.FloatValue(ctx.TArgv[3]) : 0;

        ctx.TReturn.Type = SvtMobj;
        var mobj = ctx.SpawnActor(pclass, x, y, z);
        ctx.TReturn.Mobj = mobj;
        if (mobj == null)
            return;

        ctx.SetActorYaw(mobj, angle);
        if (ctx.FsNoCheckPosition)
            return;
        if (ctx.TestMobjLocation(mobj))
            return;
        ctx.DestroyFailedSpawn(mobj);
        ctx.TReturn.Mobj = null;
    }
}
