namespace HCDE.Playsim;

/// <summary>Trig/math <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3343–3533).</summary>
public static partial class VmFScriptParser
{
    static double FloatArg(ParserContext ctx, ScriptValue v) => ctx.FloatValue(v);

    public static void SfSin(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Sin(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfAsin(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Asin(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfCos(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Cos(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfAcos(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Acos(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfTan(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Tan(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfAtan(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Atan(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfExp(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Exp(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfLog(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Log(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfSqrt(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        SetReturnDouble(ctx, Math.Sqrt(FloatArg(ctx, ctx.TArgv[0])));
    }

    public static void SfFloor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var fixedBits = (long)FsFixValue(ctx, ctx.TArgv[0]);
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = fixedBits & 0xffff0000L;
    }

    public static void SfPow(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        SetReturnDouble(ctx, Math.Pow(FloatArg(ctx, ctx.TArgv[0]), FloatArg(ctx, ctx.TArgv[1])));
    }

    public static void SfNewHuPic(ParserContext ctx)
    {
        // Native stub: disabled because it was never used and never tested.
    }

    public static void SfDeleteHuPic(ParserContext ctx)
    {
        // Native stub: disabled because it was never used and never tested.
    }

    public static void SfModifyHuPic(ParserContext ctx)
    {
        // Native stub: disabled because it was never used and never tested.
    }

    public static void SfSetHuPicDisplay(ParserContext ctx)
    {
        // Native stub: disabled because it was never used and never tested.
    }

    public static void SfSetCorona(ParserContext ctx)
    {
        if (ctx.TArgc != 3)
        {
            ctx.ScriptError("incorrect arguments to function\n");
            return;
        }

        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }
}
