namespace HCDE.Playsim;

/// <summary>Mobj query/kill/teleport <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (877–1039).</summary>
public static partial class VmFScriptParser
{
    public const int SvtFixed = 7;

    public sealed partial class ScriptValue
    {
        public double FixedValue { get; set; }
    }

    public sealed partial class ParserContext
    {
        public Action<object?> ClearActorCounters { get; init; } = _ => { };
        public Action<object?> DestroyActor { get; init; } = _ => { };
        public Action<object?> MarkActorMassacreKillable { get; init; } = _ => { };
        public Action<object?, object?, object?, int> DamageActorMassacre { get; init; } = (_, _, _, _) => { };
        public Func<object?, int> ActorHealth { get; init; } = _ => 0;
        public Func<object?, double> ActorX { get; init; } = _ => 0;
        public Func<object?, double> ActorY { get; init; } = _ => 0;
        public Func<object?, double> ActorZ { get; init; } = _ => 0;
        public Func<object?, double> ActorYawDegrees { get; init; } = _ => 0;
        public Action<int, object?> EvTeleportFog { get; init; } = (_, _) => { };
    }

    static void SetReturnDouble(ParserContext ctx, double value)
    {
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = value;
    }

    static object? OptionalActor(ParserContext ctx) =>
        ctx.TArgc > 0 ? ctx.ActorValue(ctx.TArgv[0]) : ctx.Script?.Trigger;

    public static void SfRemoveObj(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (mo == null)
            return;
        ctx.ClearActorCounters(mo);
        ctx.DestroyActor(mo);
    }

    public static void SfKillObj(ParserContext ctx)
    {
        object? mo;
        if (ctx.TArgc > 0)
            mo = ctx.ActorValue(ctx.TArgv[0]);
        else
            mo = ctx.Script?.Trigger;

        if (mo == null)
            return;
        ctx.MarkActorMassacreKillable(mo);
        ctx.DamageActorMassacre(mo, null, null, ctx.ActorHealth(mo));
    }

    public static void SfObjX(ParserContext ctx) =>
        SetReturnDouble(ctx, ActorCoord(ctx, ctx.ActorX));

    public static void SfObjY(ParserContext ctx) =>
        SetReturnDouble(ctx, ActorCoord(ctx, ctx.ActorY));

    public static void SfObjZ(ParserContext ctx) =>
        SetReturnDouble(ctx, ActorCoord(ctx, ctx.ActorZ));

    public static void SfObjAngle(ParserContext ctx) =>
        SetReturnDouble(ctx, ActorCoord(ctx, ctx.ActorYawDegrees));

    static double ActorCoord(ParserContext ctx, Func<object?, double> read)
    {
        var mo = OptionalActor(ctx);
        return mo == null ? 0 : read(mo);
    }

    public static void SfTeleport(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        object? mo;
        int tag;
        if (ctx.TArgc == 1)
        {
            mo = ctx.Script?.Trigger;
            tag = ctx.IntValue(ctx.TArgv[0]);
        }
        else
        {
            mo = ctx.ActorValue(ctx.TArgv[0]);
            tag = ctx.IntValue(ctx.TArgv[1]);
        }

        if (mo != null)
            ctx.EvTeleportFog(tag, mo);
    }
}
