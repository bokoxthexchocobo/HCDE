using System.Globalization;

namespace HCDE.Playsim;

/// <summary>Expression evaluation and parse errors from <c>t_parse.cpp</c> (453–731).</summary>
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
        public Action<string> ScriptError { get; set; } = _ => throw new InvalidOperationException("script error");
        public Action<string> Printf { get; set; } = _ => { };
    }

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
                {
                    ctx.ScriptError($"unknown variable '{ctx.Tokens[n]}'\n");
                    return;
                }
                var tmp = new VmFScriptVariable.ScriptValue();
                VmFScriptVariable.GetValue(var, tmp);
                CopyFromVariableValue(tmp, returnVar);
                break;
            }
        }
    }

    internal static void CopyFromVariableValue(VmFScriptVariable.ScriptValue src, ScriptValue dst)
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
            if (ctx.Tag is not SpecRestState st)
            {
                ctx.ScriptError("function evaluation not available\n");
                return;
            }
            EvaluateFunction(st, result, start, stop);
            return;
        }
        ctx.ScriptError($"couldnt evaluate expression: {string.Join(' ', ctx.Tokens[start..(stop + 1)])}\n");
    }

    internal static void EvaluateLeftRight(ExprFrame ctx, ScriptValue left, ScriptValue right, int start, int n, int stop)
    {
        EvaluateExpression(ctx, left, start, n - 1);
        EvaluateExpression(ctx, right, n + 1, stop);
    }

    internal static VmFScriptScript.FsVariable? LookupVariable(ExprFrame ctx, string name) =>
        VmFScriptVariable.FindVariable(ctx.Script!, name, ctx.GlobalScript)
        ?? VmFScriptVariable.FindVariable(ctx.GlobalScript!, name, ctx.GlobalScript);

    internal static void CopyToVariableValue(ScriptValue src, VmFScriptVariable.ScriptValue dst)
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

    internal static VmFScriptVariable.ScriptValue ToVar(ScriptValue v)
    {
        var dst = new VmFScriptVariable.ScriptValue();
        CopyToVariableValue(v, dst);
        return dst;
    }

    public static void ErrorMessage(ScriptParseState st, string msg)
    {
        var lineNum = 0;
        if (st.Script != null && st.Rover >= 0 && st.Rover <= st.Script.Len)
        {
            for (var i = 0; i < st.LineStart && i < st.Data.Length; i++)
                if (st.Data[i] == '\n')
                    lineNum++;
        }
        throw new VmFScriptPrepro.FsParseError($"Script {st.Script?.ScriptNum}, line {lineNum}: {msg}");
    }
}
