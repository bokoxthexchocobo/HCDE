namespace HCDE.Playsim;

/// <summary>FraggleScript control-flow specials from <c>t_spec.cpp</c> (31–221).</summary>
public static partial class VmFScriptParser
{
    public sealed class SpecControlState
    {
        public ExprFrame Expr { get; } = new();
        public int Rover { get; set; }
        public int BraceType { get; set; }
        public FsSection? Section { get; set; }
        public FsSection? PrevSection { get; set; }
        public Func<FsSection, int> SectionEnd { get; init; } = s => s.EndIndex;
        public Func<FsSection, int> SectionLoop { get; init; } = s => s.LoopIndex;
    }

    public static void SpecBrace(SpecControlState st)
    {
        if (st.BraceType != BracketClose)
            return;
        if (st.Section == null)
            return;
        if (st.Section.Type == VmFScriptPrepro.StIf || st.Section.Type == VmFScriptPrepro.StElse)
            return;
        if (st.Section.Type == VmFScriptPrepro.StLoop)
            st.Rover = st.SectionLoop(st.Section);
    }

    public static bool SpecIf(SpecControlState st)
    {
        var endToken = FindOperator(st.Expr, 0, st.Expr.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.Expr.ScriptError("parse error in if statement\n");
            return false;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st.Expr, eval, 2, endToken - 1);
        var ifResult = IntValue(eval) != 0;
        if (st.Section != null && st.BraceType == BracketOpen && endToken == st.Expr.NumTokens - 1)
        {
            if (!ifResult)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else if (ifResult && endToken != st.Expr.NumTokens - 1)
            EvaluateExpression(st.Expr, eval, endToken + 1, st.Expr.NumTokens - 1);
        return ifResult;
    }

    public static bool SpecElseif(SpecControlState st, bool lastIf)
    {
        var endToken = FindOperator(st.Expr, 0, st.Expr.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.Expr.ScriptError("parse error in elseif statement\n");
            return false;
        }
        if (lastIf)
        {
            st.Rover = st.SectionEnd(st.Section!) + 1;
            return true;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st.Expr, eval, 2, endToken - 1);
        var ifResult = IntValue(eval) != 0;
        if (st.Section != null && st.BraceType == BracketOpen && endToken == st.Expr.NumTokens - 1)
        {
            if (!ifResult)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else if (ifResult && endToken != st.Expr.NumTokens - 1)
            EvaluateExpression(st.Expr, eval, endToken + 1, st.Expr.NumTokens - 1);
        return ifResult;
    }

    public static void SpecElse(SpecControlState st, bool lastIf)
    {
        if (lastIf)
            st.Rover = st.SectionEnd(st.Section!) + 1;
    }

    public static void SpecWhile(SpecControlState st)
    {
        if (st.Section == null)
        {
            st.Expr.ScriptError("no {} section given for loop\n");
            return;
        }
        var endToken = FindOperator(st.Expr, 0, st.Expr.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.Expr.ScriptError("parse error in loop statement\n");
            return;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st.Expr, eval, 2, endToken - 1);
        if (IntValue(eval) == 0)
            st.Rover = st.SectionEnd(st.Section) + 1;
    }

    public static void SpecFor(SpecControlState st)
    {
        if (st.Section == null)
        {
            st.Expr.ScriptError("need {} delimiters for for()\n");
            return;
        }
        const int start = 2;
        var comma1 = FindOperator(st.Expr, start, st.Expr.NumTokens - 1, ",");
        var comma2 = comma1 == -1 ? -1 : FindOperator(st.Expr, comma1 + 1, st.Expr.NumTokens - 1, ",");
        if (comma1 == -1 || comma2 == -1)
        {
            st.Expr.ScriptError("incorrect arguments to for()\n");
            return;
        }
        var eval = new ScriptValue();
        if (st.Section == st.PrevSection)
        {
            EvaluateExpression(st.Expr, eval, comma2 + 1, st.Expr.NumTokens - 2);
            EvaluateExpression(st.Expr, eval, comma1 + 1, comma2 - 1);
            if (IntValue(eval) == 0)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else
            EvaluateExpression(st.Expr, eval, start, comma1 - 1);
    }

    public const int BracketOpen = 1;
    public const int BracketClose = 2;
}
