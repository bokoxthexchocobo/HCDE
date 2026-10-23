using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSpecRestTests
{
    [Fact]
    public void SpecVariable_IntDecl_CreatesVariable()
    {
        var script = new VmFScriptScript.FsScript();
        var st = new VmFScriptParser.SpecRestState();
        st.Expr.Script = script;
        st.Expr.GlobalScript = script;
        st.Expr.NumTokens = 2;
        st.Expr.Tokens[0] = "int";
        st.Expr.Tokens[1] = "foo";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenNameExpr;
        Assert.True(VmFScriptParser.SpecVariable(st));
        Assert.NotNull(VmFScriptVariable.VariableForName(script, "foo"));
    }

    [Fact]
    public void EvaluateFunction_InvokesHandler()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "fn", VmFScriptVariable.SvtFunction);
        var ran = false;
        fn.FunctionHandler = () => ran = true;
        var st = new VmFScriptParser.SpecRestState();
        st.Expr.Script = script;
        st.Expr.GlobalScript = script;
        st.Expr.NumTokens = 3;
        st.Expr.Tokens[0] = "fn";
        st.Expr.Tokens[1] = "(";
        st.Expr.Tokens[2] = ")";
        st.Expr.TokenType[0] = VmFScriptParser.TokenFunctionExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[2] = VmFScriptParser.TokenOperatorExpr;
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateFunction(st, result, 0, 2);
        Assert.True(ran);
    }

    [Fact]
    public void OpStructure_InvokesDotCall()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "bar", VmFScriptVariable.SvtFunction);
        var ran = false;
        fn.FunctionHandler = () => ran = true;
        var st = new VmFScriptParser.SpecRestState();
        st.Expr.Script = script;
        st.Expr.GlobalScript = script;
        st.Expr.NumTokens = 5;
        st.Expr.Tokens[0] = "1";
        st.Expr.Tokens[1] = ".";
        st.Expr.Tokens[2] = "bar";
        st.Expr.Tokens[3] = "(";
        st.Expr.Tokens[4] = ")";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNumberExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[2] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[3] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[4] = VmFScriptParser.TokenOperatorExpr;
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(st.Expr, result, 0, 4);
        Assert.True(ran);
    }
}
