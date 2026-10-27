namespace HCDE.Playsim;

/// <summary>Reaction/momentum/trig <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (1216–1378); `SF_PushThing` in chunk **906** slice.</summary>
public static partial class VmFScriptParser
{
    public sealed partial class ParserContext
    {
        public AuthoritySimulation? Simulation { get; init; }
    }

    static Actor? AsActor(object? mobj) => mobj as Actor;

    static void SetReturnMobj(ParserContext ctx, object? mobj)
    {
        ctx.TReturn.Type = SvtMobj;
        ctx.TReturn.Mobj = mobj;
    }

    public static void SfReactionTime(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (ctx.TArgc > 1 && mo != null)
            mo.ReactionTime = ctx.IntValue(ctx.TArgv[1]) * GameTicClock.TicRate / 100;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = mo?.ReactionTime ?? 0;
    }

    public static void SfMobjTarget(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (ctx.TArgc > 1)
        {
            var target = AsActor(ctx.ActorValue(ctx.TArgv[1]));
            if (mo != null && target != null && mo.SeeState > 0 && mo.States.HasState(mo.SeeState))
            {
                mo.SetTargetPointer(target);
                mo.States.Enter(mo, mo.SeeState);
                mo.JustHit = true;
            }
        }

        Actor? currentTarget = null;
        if (mo != null && ctx.Simulation != null)
            currentTarget = AcsActorPointer.Resolve(ctx.Simulation, mo, AcsActorPointer.Target);
        SetReturnMobj(ctx, currentTarget);
    }

    public static void SfMobjMomx(ParserContext ctx) => MobjMomAxis(ctx, 0);

    public static void SfMobjMomy(ParserContext ctx) => MobjMomAxis(ctx, 1);

    public static void SfMobjMomz(ParserContext ctx) => MobjMomAxis(ctx, 2);

    static void MobjMomAxis(ParserContext ctx, int axis)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (ctx.TArgc > 1 && mo != null)
        {
            var value = ctx.FloatValue(ctx.TArgv[1]);
            switch (axis)
            {
                case 0: mo.VelocityX = Fixed.FromDouble(value); break;
                case 1: mo.VelocityY = Fixed.FromDouble(value); break;
                case 2: mo.VelocityZ = Fixed.FromDouble(value); break;
            }
        }

        var read = mo == null ? 0.0 : axis switch
        {
            0 => mo.VelocityX.ToDouble(),
            1 => mo.VelocityY.ToDouble(),
            2 => mo.VelocityZ.ToDouble(),
            _ => 0.0,
        };
        SetReturnDouble(ctx, read);
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
