using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserExprTests
{
    private static VmFScriptParser.ExprFrame Frame(int count, params (string tok, int type)[] toks)
    {
        var f = new VmFScriptParser.ExprFrame();
        f.NumTokens = count;
        for (var i = 0; i < count; i++)
        {
            f.Tokens[i] = toks[i].tok;
            f.TokenType[i] = toks[i].type;
        }
        return f;
    }

    [Fact]
    public void OpPlus_Integers()
    {
        var f = Frame(3,
            ("2", VmFScriptParser.TokenNumberExpr),
            ("+", VmFScriptParser.TokenOperatorExpr),
            ("3", VmFScriptParser.TokenNumberExpr));
        var r = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(f, r, 0, 2);
        Assert.Equal(5, VmFScriptParser.IntValue(r));
    }

    [Fact]
    public void OpCmp_Equality()
    {
        var f = Frame(3,
            ("1", VmFScriptParser.TokenNumberExpr),
            ("==", VmFScriptParser.TokenOperatorExpr),
            ("1", VmFScriptParser.TokenNumberExpr));
        var r = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(f, r, 0, 2);
        Assert.Equal(1, VmFScriptParser.IntValue(r));
    }

    [Fact]
    public void OpOr_ShortCircuitsSecondOperand()
    {
        var f = Frame(3,
            ("1", VmFScriptParser.TokenNumberExpr),
            ("||", VmFScriptParser.TokenOperatorExpr),
            ("0", VmFScriptParser.TokenNumberExpr));
        var r = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(f, r, 0, 2);
        Assert.Equal(1, VmFScriptParser.IntValue(r));
    }

    [Fact]
    public void OpEquals_AssignsVariable()
    {
        var script = new VmFScriptScript.FsScript();
        VmFScriptVariable.NewVariable(script, "x", VmFScriptVariable.SvtInt);
        var f = new VmFScriptParser.ExprFrame { Script = script, GlobalScript = script };
        f.NumTokens = 3;
        f.Tokens[0] = "x";
        f.Tokens[1] = "=";
        f.Tokens[2] = "7";
        f.TokenType[0] = VmFScriptParser.TokenNameExpr;
        f.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        f.TokenType[2] = VmFScriptParser.TokenNumberExpr;
        var r = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(f, r, 0, 2);
        var v = new VmFScriptVariable.ScriptValue();
        VmFScriptVariable.GetValue(VmFScriptVariable.VariableForName(script, "x")!, v);
        Assert.Equal(7, v.IntValue);
    }
}
