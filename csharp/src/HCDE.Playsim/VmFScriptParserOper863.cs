namespace HCDE.Playsim;

/// <summary>FraggleScript `/`/`%`, bitwise, inc/dec, and <c>.</c> dispatch from <c>t_oper.cpp</c> (350–626).</summary>
public static partial class VmFScriptParser
{
    private static void OpDivide(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);

        // haleyjd: 8-17
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
        result.IntValue = ~IntValue(result);
        result.Type = SvtInt;
    }

    private static void OpIncrement(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        if (start == n)
        {
            var var = LookupVariable(ctx, ctx.Tokens[stop]);
            if (var == null)
            {
                ctx.ScriptError($"unknown variable '{ctx.Tokens[stop]}'\n");
                return;
            }

            var scratch = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, scratch);
            CopyFromVariableValue(scratch, result);

            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                result.Type = SvtInt;
                result.IntValue = IntValue(result) + 1;
                VmFScriptVariable.SetValue(ctx.VarHost, var, ToVar(result));
            }
            else
            {
                result.SetDouble(FloatValue(result) + 1);
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

            var scratch = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, scratch);
            CopyFromVariableValue(scratch, result);

            var newValue = new VmFScriptVariable.ScriptValue();
            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                newValue.Type = VmFScriptVariable.SvtInt;
                newValue.IntValue = IntValue(result) + 1;
                VmFScriptVariable.SetValue(ctx.VarHost, var, newValue);
            }
            else
            {
                newValue.Type = VmFScriptVariable.SvtFixed;
                newValue.FixedBits = (long)((FloatValue(result) + 1) * 65536.0);
                VmFScriptVariable.SetValue(ctx.VarHost, var, newValue);
            }
        }
        else
            ctx.ScriptError("incorrect arguments to ++ operator\n");
    }

    private static void OpDecrement(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        if (start == n)
        {
            var var = LookupVariable(ctx, ctx.Tokens[stop]);
            if (var == null)
            {
                ctx.ScriptError($"unknown variable '{ctx.Tokens[stop]}'\n");
                return;
            }

            var scratch = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, scratch);
            CopyFromVariableValue(scratch, result);

            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                result.Type = SvtInt;
                result.IntValue = IntValue(result) - 1;
                VmFScriptVariable.SetValue(ctx.VarHost, var, ToVar(result));
            }
            else
            {
                result.SetDouble(FloatValue(result) - 1);
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

            var scratch = new VmFScriptVariable.ScriptValue();
            VmFScriptVariable.GetValue(var, scratch);
            CopyFromVariableValue(scratch, result);

            var newValue = new VmFScriptVariable.ScriptValue();
            if (var.Type != VmFScriptVariable.SvtFixed)
            {
                newValue.Type = VmFScriptVariable.SvtInt;
                newValue.IntValue = IntValue(result) - 1;
                VmFScriptVariable.SetValue(ctx.VarHost, var, newValue);
            }
            else
            {
                newValue.Type = VmFScriptVariable.SvtFixed;
                newValue.FixedBits = (long)((FloatValue(result) - 1) * 65536.0);
                VmFScriptVariable.SetValue(ctx.VarHost, var, newValue);
            }
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
        if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
            result.IntValue = FixedValue(left) <= FixedValue(right) ? 1 : 0;
        else
            result.IntValue = IntValue(left) <= IntValue(right) ? 1 : 0;
    }

    private static void OpGreaterThanOrEqual(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
            result.IntValue = FixedValue(left) >= FixedValue(right) ? 1 : 0;
        else
            result.IntValue = IntValue(left) >= IntValue(right) ? 1 : 0;
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
