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

    const long FixedWholeMask = 0xffff0000L;

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

    static long TruncateFixedWhole(long fixedBits) => fixedBits & FixedWholeMask;

    static int MatchActorTypeIndex(ParserContext ctx, object? mo)
    {
        if (mo == null)
            return -1;
        var cls = ctx.GetActorClass(mo);
        for (var i = 0; i < ctx.ActorTypes.Length; i++)
        {
            if (ReferenceEquals(cls, ctx.ActorTypes[i]))
                return i;
        }

        return -1;
    }

    static string CoronaColorFromArg(ParserContext ctx, ScriptValue rawVal, double val) =>
        rawVal.Type == SvtString
            ? ctx.StringValue(rawVal)
            : val.ToString(System.Globalization.CultureInfo.InvariantCulture);

    static void SetCoronaScalarField(int num, double val)
    {
        // Native `lspr[num].*` assignment disabled; mirrors type/offset/radius cases.
        _ = num;
        _ = val;
    }

    static void SetCoronaColorField(int num, string colorLiteral)
    {
        // Native String2Hex / memcpy path disabled.
        _ = num;
        _ = colorLiteral;
    }

    public static void SfSkinColor(ParserContext ctx)
    {
        // Native: ignoring it for now (`SF_SkinColor` 3245–3248).
        _ = ctx;
    }

    public static void SfPlayDemo(ParserContext ctx)
    {
        // Native: ignoring it for now (`SF_PlayDemo` 3250–3253).
        _ = ctx;
    }

    public static void SfCheckCVar(ParserContext ctx)
    {
        // Native: can't be done so return 0 (`SF_CheckCVar` 3255–3258).
        ReturnInt(ctx, 0);
    }

    public static void SfObjType(ParserContext ctx)
    {
        object? raw;
        if (ctx.TArgc > 0)
            raw = ctx.ActorValue(ctx.TArgv[0]);
        else
            raw = ctx.Script?.Trigger;

        ReturnInt(ctx, MatchActorTypeIndex(ctx, AsActor(raw)));
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
        ctx.TReturn.FixedValue = TruncateFixedWhole(fixedBits);
    }

    public static void SfPow(ParserContext ctx) => RunBinaryDouble(ctx, Math.Pow);

    static void ApplySetCoronaField(ParserContext ctx, int num, int what, double val, ScriptValue rawVal)
    {
        switch (what)
        {
            case CoronaWhatType:
                SetCoronaScalarField(num, val);
                break;
            case CoronaWhatLightXOffset:
            case CoronaWhatLightYOffset:
                SetCoronaScalarField(num, val);
                break;
            case CoronaWhatCoronaRadius:
            case CoronaWhatDynamicRadius:
                SetCoronaScalarField(num, val);
                break;
            case CoronaWhatCoronaColor:
            case CoronaWhatDynamicColor:
                SetCoronaColorField(num, CoronaColorFromArg(ctx, rawVal, val));
                break;
            default:
                ctx.PrintHigh("Error in setcorona\n");
                break;
        }
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
