namespace HCDE.Playsim;

/// <summary>FraggleScript runner from <c>t_parse.cpp</c> (283–451).</summary>
public static partial class VmFScriptParser
{
    public static void PrintTokens(ScriptParseState st)
    {
        for (var i = 0; i < st.NumTokens; i++)
            st.Printf?.Invoke($"'{st.Tokens[i]}' type={st.TokenType[i]}");
    }

    public static void RunScript(ScriptParseState st, int endIndex, bool scriptDebug = false)
    {
        try
        {
            st.PrevSection = null;
            while (st.Rover < st.Data.Length && st.Data[st.Rover] != '\0')
            {
                if (st.Rover > endIndex)
                    break;
                st.PrevSection = st.Section;
                GetTokens(st, st.Rover);
                if (st.NumTokens == 0)
                {
                    if (st.Section != null)
                    {
                        var ctrl = new SpecControlState
                        {
                            Rover = st.Rover,
                            BraceType = st.BraceType,
                            Section = st.Section,
                            PrevSection = st.PrevSection,
                        };
                        ctrl.Expr.Script = st.Script;
                        ctrl.Expr.GlobalScript = st.GlobalScript;
                        ctrl.Expr.ScriptError = st.ScriptError;
                        SpecBrace(ctrl);
                        st.Rover = ctrl.Rover;
                    }
                    continue;
                }
                if (scriptDebug)
                    PrintTokens(st);
                RunStatement(st);
            }
        }
        catch (VmFScriptPrepro.FsParseError err)
        {
            ErrorMessage(st, err.Message);
        }
        catch (FsTerminatorException)
        {
        }
    }

    public static void RunStatement(ScriptParseState st)
    {
        if (st.TokenType[0] == TokenFunctionParse)
        {
            if (st.Tokens[0] == "if")
            {
                st.Script!.LastIfTrue = RunSpecIf(st);
                return;
            }
            if (st.Tokens[0] == "elseif")
            {
                if (st.PrevSection == null ||
                    (st.PrevSection.Type != VmFScriptPrepro.StIf && st.PrevSection.Type != VmFScriptPrepro.StElseif))
                {
                    st.ScriptError("elseif statement without if\n");
                    return;
                }
                st.Script!.LastIfTrue = RunSpecElseif(st, st.Script.LastIfTrue);
                return;
            }
            if (st.Tokens[0] == "else")
            {
                if (st.PrevSection == null ||
                    (st.PrevSection.Type != VmFScriptPrepro.StIf && st.PrevSection.Type != VmFScriptPrepro.StElseif))
                {
                    st.ScriptError("else statement without if\n");
                    return;
                }
                RunSpecElse(st, st.Script!.LastIfTrue);
                st.Script.LastIfTrue = true;
                return;
            }
            if (st.Tokens[0] == "while")
            {
                RunSpecWhile(st);
                return;
            }
            if (st.Tokens[0] == "for")
            {
                RunSpecFor(st);
                return;
            }
        }
        else if (st.TokenType[0] == TokenNameParse)
        {
            if (st.Tokens[0] == "else")
            {
                if (st.PrevSection == null ||
                    (st.PrevSection.Type != VmFScriptPrepro.StIf && st.PrevSection.Type != VmFScriptPrepro.StElseif))
                {
                    st.ScriptError("else statement without if\n");
                    return;
                }
                RunSpecElse(st, st.Script!.LastIfTrue);
                st.Script.LastIfTrue = true;
                return;
            }
            if (RunSpecVariable(st))
                return;
        }

        var scratch = new ScriptValue();
        var expr = new ExprFrame();
        CopyToExprFrame(st, expr);
        EvaluateExpression(expr, scratch, 0, st.NumTokens - 1);
    }

    private static bool RunSpecIf(ScriptParseState st)
    {
        var ctrl = BuildSpecControl(st);
        var result = SpecIf(ctrl);
        st.Rover = ctrl.Rover;
        return result;
    }

    private static bool RunSpecElseif(ScriptParseState st, bool lastIf)
    {
        var ctrl = BuildSpecControl(st);
        var result = SpecElseif(ctrl, lastIf);
        st.Rover = ctrl.Rover;
        return result;
    }

    private static void RunSpecElse(ScriptParseState st, bool lastIf)
    {
        var ctrl = BuildSpecControl(st);
        SpecElse(ctrl, lastIf);
        st.Rover = ctrl.Rover;
    }

    private static void RunSpecWhile(ScriptParseState st)
    {
        var ctrl = BuildSpecControl(st);
        SpecWhile(ctrl);
        st.Rover = ctrl.Rover;
    }

    private static void RunSpecFor(ScriptParseState st)
    {
        var ctrl = BuildSpecControl(st);
        SpecFor(ctrl);
        st.Rover = ctrl.Rover;
    }

    private static bool RunSpecVariable(ScriptParseState st)
    {
        var rest = new SpecRestState();
        CopyToExprFrame(st, rest.Expr);
        return SpecVariable(rest);
    }

    private static void CopyToExprFrame(ScriptParseState st, ExprFrame expr)
    {
        expr.Script = st.Script;
        expr.GlobalScript = st.GlobalScript;
        expr.NumTokens = st.NumTokens;
        expr.ScriptError = st.ScriptError;
        for (var i = 0; i < st.NumTokens; i++)
        {
            expr.Tokens[i] = st.Tokens[i];
            expr.TokenType[i] = st.TokenType[i] switch
            {
                TokenStringParse => TokenStringExpr,
                TokenNumberParse => TokenNumberExpr,
                TokenOperatorParse => TokenOperatorExpr,
                TokenFunctionParse => TokenFunctionExpr,
                _ => TokenNameExpr,
            };
        }
    }

    private static SpecControlState BuildSpecControl(ScriptParseState st)
    {
        var ctrl = new SpecControlState
        {
            Rover = st.Rover,
            BraceType = st.BraceType,
            Section = st.Section,
            PrevSection = st.PrevSection,
        };
        CopyToExprFrame(st, ctrl.Expr);
        return ctrl;
    }
}
