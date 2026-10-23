namespace HCDE.Playsim;

/// <summary>FraggleScript arithmetic / bitwise operators from <c>t_oper.cpp</c> (377–526).</summary>
public static partial class VmFScriptParser
{
    private static void OpMultiply(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
            result.SetDouble(FloatValue(left) * FloatValue(right));
        else
        {
            result.Type = SvtInt;
            result.IntValue = IntValue(left) * IntValue(right);
        }
    }

    private static void OpDivide(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
        {
            var fr = FloatValue(right);
            if (fr == 0)
                ctx.ScriptError("divide by zero\n");
            else
                result.SetDouble(FloatValue(left) / fr);
        }
        else
        {
            var ir = IntValue(right);
            if (ir == 0)
                ctx.ScriptError("divide by zero\n");
            else
            {
                result.Type = SvtInt;
                result.IntValue = IntValue(left) / ir;
            }
        }
    }

    private static void OpRemainder(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        var ir = IntValue(right);
        if (ir == 0)
            ctx.ScriptError("divide by zero\n");
        else
        {
            result.Type = SvtInt;
            result.IntValue = IntValue(left) % ir;
        }
    }

    private static void OpOrBin(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = IntValue(left) | IntValue(right);
    }

    private static void OpAndBin(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = IntValue(left) & IntValue(right);
    }

    private static void OpNotBin(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        EvaluateExpression(ctx, result, n + 1, stop);
        result.Type = SvtInt;
        result.IntValue = ~IntValue(result);
    }
}
