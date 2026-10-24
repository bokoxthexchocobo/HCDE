namespace HCDE.Playsim;

/// <summary>FraggleScript compare / unary-not operators from <c>t_oper.cpp</c> (227–376).</summary>
public static partial class VmFScriptParser
{
    private static void OpNotCmp(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        OpCmp(ctx, result, start, n, stop);
        result.IntValue = result.IntValue == 0 ? 1 : 0;
    }

    private static void OpLessThan(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed
            ? FixedValue(left) < FixedValue(right)
            : IntValue(left) < IntValue(right)) ? 1 : 0;
    }

    private static void OpGreaterThan(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed
            ? FixedValue(left) > FixedValue(right)
            : IntValue(left) > IntValue(right)) ? 1 : 0;
    }

    private static void OpNot(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        EvaluateExpression(ctx, result, n + 1, stop);
        result.Type = SvtInt;
        result.IntValue = IntValue(result) == 0 ? 1 : 0;
    }

    private static void OpPlus(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        if (left.Type == SvtString)
        {
            result.Type = SvtString;
            result.String = right.Type == SvtString
                ? left.String + right.String
                : right.Type == VmFScriptVariable.SvtFixed
                    ? $"{left.String}{FloatValue(right):F4}"
                    : $"{left.String}{IntValue(right)}";
        }
        else if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
            result.SetDouble(FloatValue(left) + FloatValue(right));
        else
        {
            result.Type = SvtInt;
            result.IntValue = IntValue(left) + IntValue(right);
        }
    }

    private static void OpMinus(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        if (start == n)
            EvaluateExpression(ctx, right, n + 1, stop);
        else
            EvaluateLeftRight(ctx, left, right, start, n, stop);
        if (start != n && (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed))
            result.SetDouble(FloatValue(left) - FloatValue(right));
        else if (start == n)
        {
            result.Type = SvtInt;
            result.IntValue = -IntValue(right);
        }
        else
        {
            result.Type = SvtInt;
            result.IntValue = IntValue(left) - IntValue(right);
        }
    }

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
}
