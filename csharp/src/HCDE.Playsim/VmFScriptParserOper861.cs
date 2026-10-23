namespace HCDE.Playsim;

/// <summary>FraggleScript operator table and assign/logic ops from <c>t_oper.cpp</c> (39–226).</summary>
public static partial class VmFScriptParser
{
    private delegate void OperHandler(ExprFrame ctx, ScriptValue result, int start, int n, int stop);

    private readonly record struct OperEntry(string Op, OperHandler Handler, int Direction);

    private static readonly OperEntry[] Operators =
    [
        new("=", OpEquals, OpBackward),
        new("||", OpOr, OpForward),
        new("&&", OpAnd, OpForward),
        new("|", OpOrBin, OpForward),
        new("&", OpAndBin, OpForward),
        new("==", OpCmp, OpForward),
        new("!=", OpNotCmp, OpForward),
        new("<", OpLessThan, OpForward),
        new(">", OpGreaterThan, OpForward),
        new("<=", OpLessThanOrEqual, OpForward),
        new(">=", OpGreaterThanOrEqual, OpForward),
        new("+", OpPlus, OpForward),
        new("-", OpMinus, OpForward),
        new("*", OpMultiply, OpForward),
        new("/", OpDivide, OpForward),
        new("%", OpRemainder, OpForward),
        new("~", OpNotBin, OpForward),
        new("!", OpNot, OpForward),
        new("++", OpIncrement, OpForward),
        new("--", OpDecrement, OpForward),
        new(".", OpStructureDispatch, OpForward),
    ];

    private static void OpEquals(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var var = LookupVariable(ctx, ctx.Tokens[start]);
        if (var == null)
            ctx.ScriptError($"unknown variable '{ctx.Tokens[start]}'\n");
        else
        {
            EvaluateExpression(ctx, result, n + 1, stop);
            var src = new VmFScriptVariable.ScriptValue();
            CopyToVariableValue(result, src);
            VmFScriptVariable.SetValue(ctx.VarHost, var, src);
        }
    }

    private static void OpOr(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        EvaluateExpression(ctx, result, start, n - 1);
        var exprTrue = IntValue(result) != 0;
        if (!exprTrue)
        {
            EvaluateExpression(ctx, result, n + 1, stop);
            exprTrue = IntValue(result) != 0;
        }
        result.Type = SvtInt;
        result.IntValue = exprTrue ? 1 : 0;
    }

    private static void OpAnd(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        EvaluateExpression(ctx, result, start, n - 1);
        var exprTrue = IntValue(result) != 0;
        if (exprTrue)
        {
            EvaluateExpression(ctx, result, n + 1, stop);
            exprTrue = IntValue(result) != 0;
        }
        else
            exprTrue = false;
        result.Type = SvtInt;
        result.IntValue = exprTrue ? 1 : 0;
    }

    private static void OpCmp(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        var left = new ScriptValue();
        var right = new ScriptValue();
        EvaluateLeftRight(ctx, left, right, start, n, stop);
        result.Type = SvtInt;
        if (left.Type == SvtString && right.Type == SvtString)
        {
            result.IntValue = left.String == right.String ? 1 : 0;
            return;
        }
        if (left.Type == SvtMobj && right.Type == SvtMobj)
        {
            result.IntValue = ReferenceEquals(left.Mobj, right.Mobj) ? 1 : 0;
            return;
        }
        if (left.Type == VmFScriptVariable.SvtFixed || right.Type == VmFScriptVariable.SvtFixed)
            result.IntValue = FixedValue(left) == FixedValue(right) ? 1 : 0;
        else
            result.IntValue = IntValue(left) == IntValue(right) ? 1 : 0;
    }
}
