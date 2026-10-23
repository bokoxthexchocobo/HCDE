namespace HCDE.Playsim;

/// <summary>Legacy act/query <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (2778–3047, 3245–3341).</summary>
public static partial class VmFScriptParser
{
    static Actor? ActActor(ParserContext ctx) => AsActor(OptionalActor(ctx));

    public static void SfObjAwaken(ParserContext ctx)
    {
        var mo = ActActor(ctx);
        if (mo != null)
            mo.Activate(AsActor(ctx.Script?.Trigger));
    }

    public static void SfAmbientSound(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var name = ctx.StringValue(ctx.TArgv[0]);
        _ = FindSound(ctx, name);
        ActorSoundActions.StartGlobalSound(ctx.Simulation, name);
    }

    public static void SfExitSecret(ParserContext ctx) =>
        ctx.Simulation?.MarkExited(secret: true);

    public static void SfMobjValue(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        ctx.TReturn.Type = SvtMobj;
        ctx.TReturn.Mobj = ctx.ActorValue(ctx.TArgv[0]);
    }

    public static void SfStringValue(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        ctx.TReturn.Type = SvtString;
        ctx.TReturn.String = ctx.TArgv[0].Type == SvtString
            ? ctx.TArgv[0].String
            : ctx.StringValue(ctx.TArgv[0]);
    }

    public static void SfIntValue(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.IntValue(ctx.TArgv[0]);
    }

    public static void SfFixedValue(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = FsFixValue(ctx, ctx.TArgv[0]);
    }

    public static void SfSetObjPosition(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var mobj = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (mobj == null)
            return;
        mobj.X = Fixed.FromDouble(ctx.FloatValue(ctx.TArgv[1]));
        mobj.Y = ctx.TArgc >= 3
            ? Fixed.FromDouble(ctx.FloatValue(ctx.TArgv[2]))
            : mobj.Y;
        mobj.Z = ctx.TArgc >= 4
            ? Fixed.FromDouble(ctx.FloatValue(ctx.TArgv[3]))
            : mobj.Z;
    }

    public static void SfObjDead(ParserContext ctx)
    {
        var mo = ActActor(ctx);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = mo != null && (mo.Health <= 0 || mo.Corpse) ? 1 : 0;
    }

    public static void SfSkinColor(ParserContext ctx)
    {
    }

    public static void SfPlayDemo(ParserContext ctx)
    {
    }

    public static void SfCheckCVar(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }

    public static void SfObjType(ParserContext ctx)
    {
        var mo = ActActor(ctx);
        if (mo != null)
        {
            var cls = ctx.GetActorClass(mo);
            for (var i = 0; i < ctx.ActorTypes.Length; i++)
            {
                if (ReferenceEquals(cls, ctx.ActorTypes[i]))
                {
                    ctx.TReturn.Type = SvtInt;
                    ctx.TReturn.IntValue = i;
                    return;
                }
            }
        }

        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = -1;
    }
}
