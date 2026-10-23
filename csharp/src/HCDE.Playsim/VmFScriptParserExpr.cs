using System.Globalization;

namespace HCDE.Playsim;

/// <summary>Expression operators and evaluation from <c>t_oper.cpp</c> + <c>t_parse.cpp</c> (EvaluateExpression).</summary>
public static partial class VmFScriptParser
{
    public const int TokenNameExpr = 4;
    public const int TokenOperatorExpr = 3;
    public const int TokenNumberExpr = 2;
    public const int TokenStringExpr = 1;
    public const int TokenFunctionExpr = 5;
    public const int OpForward = 0;
    public const int OpBackward = 1;

    public sealed partial class ScriptValue
    {
        public long FixedBits { get; set; }

        public void SetDouble(double d)
        {
            Type = VmFScriptVariable.SvtFixed;
            FixedBits = (long)(d * 65536.0);
        }
    }

    public sealed class ExprFrame
    {
        public object? Tag { get; set; }
        public VmFScriptScript.FsScript? Script { get; set; }
        public VmFScriptScript.FsScript? GlobalScript { get; set; }
        public int NumTokens { get; set; }
        public string[] Tokens { get; } = new string[64];
        public int[] TokenType { get; } = new int[64];
        public VmFScriptVariable.VariableHost VarHost { get; init; } = new();
        public Action<string> ScriptError { get; init; } = _ => throw new InvalidOperationException("script error");
        public Action<string> Printf { get; set; } = _ => { };
        public Action<ExprFrame, ScriptValue, int, int>? EvaluateFunctionHook { get; set; }
        public Action<ExprFrame, ScriptValue, int, int, int>? OpStructureHook { get; set; }
    }

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

    public static int IntValue(ScriptValue v) =>
        v.Type == SvtString ? int.Parse(v.String, CultureInfo.InvariantCulture) :
        v.Type == VmFScriptVariable.SvtFixed ? (int)(v.FixedBits / 65536.0) :
        v.Type == SvtMobj ? -1 : v.IntValue;

    public static long FixedValue(ScriptValue v) =>
        v.Type == VmFScriptVariable.SvtFixed ? v.FixedBits :
        v.Type == SvtString ? (long)(double.Parse(v.String, CultureInfo.InvariantCulture) * 65536.0) :
        v.Type == SvtMobj ? -65536 : v.IntValue * 65536L;

    public static double FloatValue(ScriptValue v) =>
        v.Type == SvtString ? double.Parse(v.String, CultureInfo.InvariantCulture) :
        v.Type == VmFScriptVariable.SvtFixed ? v.FixedBits / 65536.0 :
        v.Type == SvtMobj ? -1.0 : v.IntValue;

    public static int FindOperator(ExprFrame ctx, int start, int stop, string value)
    {
        var bracketLevel = 0;
        for (var i = start; i <= stop; i++)
        {
            if (ctx.TokenType[i] != TokenOperatorExpr)
                continue;
            bracketLevel += ctx.Tokens[i] == "(" ? 1 : ctx.Tokens[i] == ")" ? -1 : 0;
            if (bracketLevel == 0 && ctx.Tokens[i] == value)
                return i;
        }
        return -1;
    }

    public static int FindOperatorBackwards(ExprFrame ctx, int start, int stop, string value)
    {
        var bracketLevel = 0;
        for (var i = stop; i >= start; i--)
        {
            if (ctx.TokenType[i] != TokenOperatorExpr)
                continue;
            bracketLevel += ctx.Tokens[i] == "(" ? -1 : ctx.Tokens[i] == ")" ? 1 : 0;
            if (bracketLevel == 0 && ctx.Tokens[i] == value)
                return i;
        }
        return -1;
    }

    public static void PointlessBrackets(ExprFrame ctx, ref int start, ref int stop)
    {
        while (ctx.TokenType[start] == TokenOperatorExpr && ctx.Tokens[start] == "(" &&
               ctx.TokenType[stop] == TokenOperatorExpr && ctx.Tokens[stop] == ")")
        {
            var bracketLevel = 0;
            for (var i = start; i < stop; i++)
            {
                if (ctx.TokenType[i] != TokenOperatorExpr)
                    continue;
                bracketLevel += ctx.Tokens[i] == "(" ? 1 : 0;
                bracketLevel -= ctx.Tokens[i] == ")" ? 1 : 0;
                if (bracketLevel == 0)
                    return;
            }
            start++;
            stop--;
        }
    }

    public static void SimpleEvaluate(ExprFrame ctx, ScriptValue returnVar, int n)
    {
        switch (ctx.TokenType[n])
        {
            case TokenStringExpr:
                returnVar.Type = SvtString;
                returnVar.String = ctx.Tokens[n];
                break;
            case TokenNumberExpr:
                if (ctx.Tokens[n].Contains('.'))
                    returnVar.SetDouble(double.Parse(ctx.Tokens[n], CultureInfo.InvariantCulture));
                else
                {
                    returnVar.Type = SvtInt;
                    returnVar.IntValue = int.Parse(ctx.Tokens[n], CultureInfo.InvariantCulture);
                }
                break;
            case TokenNameExpr:
            {
                var var = VmFScriptVariable.FindVariable(ctx.Script!, ctx.Tokens[n], ctx.GlobalScript)
                          ?? VmFScriptVariable.FindVariable(ctx.GlobalScript!, ctx.Tokens[n], ctx.GlobalScript);
                if (var == null)
                    ctx.ScriptError($"unknown variable '{ctx.Tokens[n]}'\n");
                var tmp = new VmFScriptVariable.ScriptValue();
                VmFScriptVariable.GetValue(var, tmp);
                CopyFromVariableValue(tmp, returnVar);
                break;
            }
        }
    }

    private static void CopyFromVariableValue(VmFScriptVariable.ScriptValue src, ScriptValue dst)
    {
        dst.Type = src.Type switch
        {
            VmFScriptVariable.SvtString => SvtString,
            VmFScriptVariable.SvtMobj => SvtMobj,
            VmFScriptVariable.SvtFixed => VmFScriptVariable.SvtFixed,
            _ => SvtInt,
        };
        dst.String = src.String;
        dst.IntValue = src.IntValue;
        dst.FixedBits = src.FixedBits;
        dst.Mobj = src.Mobj;
    }

    public static void EvaluateExpression(ExprFrame ctx, ScriptValue result, int start, int stop)
    {
        if (ctx.TokenType[start] == TokenOperatorExpr && ctx.TokenType[stop] == TokenOperatorExpr)
            PointlessBrackets(ctx, ref start, ref stop);
        if (start == stop)
        {
            SimpleEvaluate(ctx, result, start);
            return;
        }
        for (var i = 0; i < Operators.Length; i++)
        {
            var entry = Operators[i];
            var n = entry.Direction == OpForward
                ? FindOperatorBackwards(ctx, start, stop, entry.Op)
                : FindOperator(ctx, start, stop, entry.Op);
            if (n != -1)
            {
                entry.Handler(ctx, result, start, n, stop);
                return;
            }
        }
        if (ctx.TokenType[start] == TokenFunctionExpr)
        {
            if (ctx.EvaluateFunctionHook == null)
                ctx.ScriptError("function evaluation not available\n");
            else
                ctx.EvaluateFunctionHook(ctx, result, start, stop);
            return;
        }
        ctx.ScriptError($"couldnt evaluate expression: {string.Join(' ', ctx.Tokens[start..(stop + 1)])}\n");
    }

    private static void EvaluateLeftRight(ExprFrame ctx, ScriptValue left, ScriptValue right, int start, int n, int stop)
    {
        EvaluateExpression(ctx, left, start, n - 1);
        EvaluateExpression(ctx, right, n + 1, stop);
    }

    private static VmFScriptScript.FsVariable? LookupVariable(ExprFrame ctx, string name) =>
        VmFScriptVariable.FindVariable(ctx.Script!, name, ctx.GlobalScript)
        ?? VmFScriptVariable.FindVariable(ctx.GlobalScript!, name, ctx.GlobalScript);

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

    private static void CopyToVariableValue(ScriptValue src, VmFScriptVariable.ScriptValue dst)
    {
        dst.Type = src.Type switch
        {
            SvtString => VmFScriptVariable.SvtString,
            SvtMobj => VmFScriptVariable.SvtMobj,
            VmFScriptVariable.SvtFixed => VmFScriptVariable.SvtFixed,
            _ => VmFScriptVariable.SvtInt,
        };
        dst.String = src.String;
        dst.IntValue = src.IntValue;
        dst.FixedBits = src.FixedBits;
        dst.Mobj = src.Mobj;
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
                ctx.ScriptError($"unknown variable '{ctx.Tokens[stop]}'\n");
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
                ctx.ScriptError($"unknown variable '{ctx.Tokens[start]}'\n");
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

    private static VmFScriptVariable.ScriptValue ToVar(ScriptValue v)
    {
        var dst = new VmFScriptVariable.ScriptValue();
        CopyToVariableValue(v, dst);
        return dst;
    }

    private static void OpStructureDispatch(ExprFrame ctx, ScriptValue result, int start, int n, int stop)
    {
        if (ctx.OpStructureHook == null)
            ctx.ScriptError("structure operator not available\n");
        else
            ctx.OpStructureHook(ctx, result, start, n, stop);
    }
}
