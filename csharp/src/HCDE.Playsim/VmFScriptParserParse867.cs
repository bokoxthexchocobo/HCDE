namespace HCDE.Playsim;

/// <summary>Expression evaluation and parse errors from <c>t_parse.cpp</c> (577–731).</summary>
public static partial class VmFScriptParser
{
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
