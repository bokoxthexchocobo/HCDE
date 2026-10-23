namespace HCDE.Playsim;

/// <summary>Momentum and trig <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (1201–1378).</summary>
public static partial class VmFScriptParser
{
    static void SetReturnMobj(ParserContext ctx, object? mobj)
    {
        ctx.TReturn.Type = SvtMobj;
        ctx.TReturn.Mobj = mobj;
    }

    public static void SfPushThing(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 3))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (mo == null)
            return;
        _ = ctx.FloatValue(ctx.TArgv[1]);
        _ = ctx.FloatValue(ctx.TArgv[2]);
        // Gap: mo->Thrust(DAngle::fromDeg(floatvalue(t_argv[1])), floatvalue(t_argv[2])) — AActor::Thrust (actor.h 1683–1687), t_func.cpp 1211–1213.
    }

    public static void SfReactionTime(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (ctx.TArgc > 1 && mo != null)
            _ = ctx.IntValue(ctx.TArgv[1]);
        // Gap: mo->reactiontime assign/read (t_func.cpp 1232–1236).
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }

    public static void SfMobjTarget(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (ctx.TArgc > 1)
        {
            var target = ctx.ActorValue(ctx.TArgv[1]);
            if (mo != null && target != null)
                _ = target;
            // Gap: target, SeeState, SetState, MF_JUSTHIT (t_func.cpp 1257–1263).
        }
        // Gap: t_return.value.mobj = mo ? mo->target (t_func.cpp 1266–1267).
        SetReturnMobj(ctx, null);
    }

    public static void SfMobjMomx(ParserContext ctx) => MobjMom(ctx);

    public static void SfMobjMomy(ParserContext ctx) => MobjMom(ctx);

    public static void SfMobjMomz(ParserContext ctx) => MobjMom(ctx);

    static void MobjMom(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (ctx.TArgc > 1 && mo != null)
            _ = ctx.FloatValue(ctx.TArgv[1]);
        // Gap: mo->Vel.X|Y|Z read/write (t_func.cpp 1284–1290, 1307–1313, 1330–1336).
        SetReturnDouble(ctx, 0);
    }

    public static void SfPointToAngle(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 4))
            return;
        var x1 = ctx.FloatValue(ctx.TArgv[0]);
        var y1 = ctx.FloatValue(ctx.TArgv[1]);
        var x2 = ctx.FloatValue(ctx.TArgv[2]);
        var y2 = ctx.FloatValue(ctx.TArgv[3]);
        var angle = Math.Atan2(y2 - y1, x2 - x1) * (180.0 / Math.PI);
        if (angle < 0)
            angle += 360.0;
        SetReturnDouble(ctx, angle);
    }

    public static void SfPointToDist(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 4))
            return;
        var x = ctx.FloatValue(ctx.TArgv[2]) - ctx.FloatValue(ctx.TArgv[0]);
        var y = ctx.FloatValue(ctx.TArgv[3]) - ctx.FloatValue(ctx.TArgv[1]);
        SetReturnDouble(ctx, Math.Sqrt(x * x + y * y));
    }
}
