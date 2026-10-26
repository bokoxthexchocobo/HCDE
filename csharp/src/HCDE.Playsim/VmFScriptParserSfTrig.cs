namespace HCDE.Playsim;

/// <summary>Trig/math and tail stubs <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3245–3341, 3343–3445, 3481–3533).</summary>
public static partial class VmFScriptParser
{
    public const int CoronaWhatType = 0;
    public const int CoronaWhatLightXOffset = 1;
    public const int CoronaWhatLightYOffset = 2;
    public const int CoronaWhatCoronaColor = 3;
    public const int CoronaWhatCoronaRadius = 4;
    public const int CoronaWhatDynamicColor = 5;
    public const int CoronaWhatDynamicRadius = 6;

    static double FloatArg(ParserContext ctx, ScriptValue v) => ctx.FloatValue(v);

    static void ReturnInt(ParserContext ctx, int value)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = value;
    }

    static bool RunUnaryDouble(ParserContext ctx, Func<double, double> fn)
    {
        if (!CheckArgs(ctx, 1))
            return false;
        SetReturnDouble(ctx, fn(FloatArg(ctx, ctx.TArgv[0])));
        return true;
    }

    static bool RunBinaryDouble(ParserContext ctx, Func<double, double, double> fn)
    {
        if (!CheckArgs(ctx, 2))
            return false;
        SetReturnDouble(ctx, fn(FloatArg(ctx, ctx.TArgv[0]), FloatArg(ctx, ctx.TArgv[1])));
        return true;
    }

    public static void SfSkinColor(ParserContext ctx)
    {
        // Native: ignoring it for now (`SF_SkinColor` 3245–3248).
    }

    public static void SfPlayDemo(ParserContext ctx)
    {
        // Native: ignoring it for now (`SF_PlayDemo` 3250–3253).
    }

    public static void SfCheckCVar(ParserContext ctx)
    {
        // Native: can't be done so return 0 (`SF_CheckCVar` 3255–3258).
        ReturnInt(ctx, 0);
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
                    ReturnInt(ctx, i);
                    return;
                }
            }
        }

        ReturnInt(ctx, -1);
    }

    public static void SfSin(ParserContext ctx) => RunUnaryDouble(ctx, Math.Sin);

    public static void SfAsin(ParserContext ctx) => RunUnaryDouble(ctx, Math.Asin);

    public static void SfCos(ParserContext ctx) => RunUnaryDouble(ctx, Math.Cos);

    public static void SfAcos(ParserContext ctx) => RunUnaryDouble(ctx, Math.Acos);

    public static void SfTan(ParserContext ctx) => RunUnaryDouble(ctx, Math.Tan);

    public static void SfAtan(ParserContext ctx) => RunUnaryDouble(ctx, Math.Atan);

    public static void SfExp(ParserContext ctx) => RunUnaryDouble(ctx, Math.Exp);

    public static void SfLog(ParserContext ctx) => RunUnaryDouble(ctx, Math.Log);

    public static void SfSqrt(ParserContext ctx) => RunUnaryDouble(ctx, Math.Sqrt);

    public static void SfFloor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var fixedBits = (long)FsFixValue(ctx, ctx.TArgv[0]);
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = fixedBits & 0xffff0000L;
    }

    public static void SfPow(ParserContext ctx) => RunBinaryDouble(ctx, Math.Pow);

    static void ApplySetCoronaField(ParserContext ctx, int num, int what, double val, ScriptValue rawVal)
    {
        // Native `lspr` mutation is disabled; cases mirror `SF_SetCorona` switch (3493–3527).
        switch (what)
        {
            case CoronaWhatType:
            case CoronaWhatLightXOffset:
            case CoronaWhatLightYOffset:
            case CoronaWhatCoronaRadius:
            case CoronaWhatDynamicRadius:
                break;
            case CoronaWhatCoronaColor:
            case CoronaWhatDynamicColor:
                _ = rawVal.Type == SvtString ? ctx.StringValue(rawVal) : val.ToString(System.Globalization.CultureInfo.InvariantCulture);
                break;
            default:
                ctx.PrintHigh("Error in setcorona\n");
                break;
        }

        _ = num;
        _ = val;
    }

    public static void SfSetCorona(ParserContext ctx)
    {
        if (ctx.TArgc != 3)
        {
            ctx.ScriptError("incorrect arguments to function\n");
            return;
        }

        var num = ctx.IntValue(ctx.TArgv[0]);
        var what = ctx.IntValue(ctx.TArgv[1]);
        var val = FloatArg(ctx, ctx.TArgv[2]);
        ApplySetCoronaField(ctx, num, what, val, ctx.TArgv[2]);
        ReturnInt(ctx, 0);
    }
}
