namespace HCDE.Playsim;

/// <summary>FraggleScript `/`/`%`, bitwise, inc/dec, and <c>.</c> dispatch from <c>t_oper.cpp</c> (350–626).</summary>
public static partial class VmFScriptParser
{
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

    private static void OpIncrement(ExprFrame ctx, ScriptValue result, int start, int n, int stop) =>
        OpIncDec(ctx, result, start, n, stop, +1);

    private static void OpDecrement(ExprFrame ctx, ScriptValue result, int start, int n, int stop) =>
        OpIncDec(ctx, result, start, n, stop, -1);

    private static void OpIncDec(ExprFrame ctx, ScriptValue result, int start, int n, int stop, int delta)
    {
        if (start == n)
        {
            var var = LookupVariable(ctx, ctx.Tokens[stop]);
            if (var == null)
            {
                ctx.ScriptError($"unknown variable '{ctx.Tokens[stop]}'\n");
                return;
            }
            var tmp = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, tmp);
            CopyFromVariableValue(tmp, result);
            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                result.Type = SvtInt;
                result.IntValue = IntValue(result) + delta;
                VmFScriptVariable.SetValue(ctx.VarHost, var, ToVar(result));
            }
            else
            {
                result.SetDouble(FloatValue(result) + delta);
                VmFScriptVariable.SetValue(ctx.VarHost, var, ToVar(result));
            }
        }
        else if (stop == n)
        {
            var var = LookupVariable(ctx, ctx.Tokens[start]);
            if (var == null)
            {
                ctx.ScriptError($"unknown variable '{ctx.Tokens[start]}'\n");
                return;
            }
            var tmp = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, tmp);
            CopyFromVariableValue(tmp, result);
            var newValue = new VmFScriptVariable.ScriptValue();
            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                newValue.Type = VmFScriptVariable.SvtInt;
                newValue.IntValue = IntValue(result) + delta;
            }
            else
            {
                newValue.Type = VmFScriptVariable.SvtFixed;
                newValue.FixedBits = (long)((FloatValue(result) + delta) * 65536.0);
            }
            VmFScriptVariable.SetValue(ctx.VarHost, var, newValue);
        }
        else
            ctx.ScriptError("incorrect arguments to ++ operator\n");
    }

    private static void OpLessThanOrEqual(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed
            ? FixedValue(left) <= FixedValue(right)
            : IntValue(left) <= IntValue(right)) ? 1 : 0;
    }

    private static void OpGreaterThanOrEqual(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        result.IntValue = (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed
            ? FixedValue(left) >= FixedValue(right)
            : IntValue(left) >= IntValue(right)) ? 1 : 0;
    }

    private static void OpStructureDispatch(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        if (ctx.Tag is not SpecRestState st)
        {
            ctx.ScriptError("structure operator not available\n");
            return;
        }
        OpStructure(st, result, start, n, stop);
    }
}
