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
    public void SpecScript_StoresChildAndAdvancesRover()
    {
        var parent = new VmFScriptScript.FsScript();
        var section = new VmFScriptParser.FsSection { StartIndex = 0, EndIndex = 10 };
        VmFScriptScript.FsScript? child = null;
        var st = new VmFScriptParser.SpecRestState
        {
            Section = section,
            SectionStartChars = (_, _, _, size) =>
            {
                var buf = new char[size + 2];
                buf[0] = '{';
                for (var i = 0; i < size; i++)
                    buf[i + 1] = (char)('a' + i);
                return buf;
            },
            StoreChildScript = (_, num, c) =>
            {
                Assert.Equal(2, num);
                child = c;
            },
        };
        st.Expr.Script = parent;
        st.Expr.GlobalScript = parent;
        st.Expr.NumTokens = 2;
        st.Expr.Tokens[0] = "script";
        st.Expr.Tokens[1] = "2";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenNumberExpr;
        Assert.True(VmFScriptParser.SpecVariable(st));
        Assert.NotNull(child);
        Assert.Equal(11, st.Rover);
    }

    [Fact]
    public void SpecScript_CopiesSectionBodyIntoChildData()
    {
        var parent = new VmFScriptScript.FsScript();
        var section = new VmFScriptParser.FsSection { StartIndex = 5, EndIndex = 9 };
        VmFScriptScript.FsScript? child = null;
        var st = new VmFScriptParser.SpecRestState
        {
            Section = section,
            SectionStartChars = (_, _, _, size) =>
            {
                var buf = new char[size + 2];
                buf[0] = '{';
                buf[1] = 'x';
                buf[2] = 'y';
                buf[size + 1] = '}';
                return buf;
            },
            StoreChildScript = (_, _, c) => child = c,
        };
        st.Expr.Script = parent;
        st.Expr.GlobalScript = parent;
        st.Expr.NumTokens = 2;
        st.Expr.Tokens[0] = "script";
        st.Expr.Tokens[1] = "1";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenNumberExpr;
        VmFScriptParser.SpecScript(st);
        Assert.NotNull(child);
        Assert.Equal(2, child!.Data!.Length - 1);
        Assert.Equal((byte)'x', child.Data[0]);
        Assert.Equal((byte)'y', child.Data[1]);
        Assert.Equal(1, child.ScriptNum);
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
    public void EvaluateFunction_PassesArgumentValues()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "sum", VmFScriptVariable.SvtFunction);
        var st = new VmFScriptParser.SpecRestState();
        var seen = 0;
        st.Expr.Script = script;
        st.Expr.GlobalScript = script;
        st.Expr.NumTokens = 5;
        st.Expr.Tokens[0] = "sum";
        st.Expr.Tokens[1] = "(";
        st.Expr.Tokens[2] = "4";
        st.Expr.Tokens[3] = ",";
        st.Expr.Tokens[4] = ")";
        st.Expr.TokenType[0] = VmFScriptParser.TokenFunctionExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[2] = VmFScriptParser.TokenNumberExpr;
        st.Expr.TokenType[3] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[4] = VmFScriptParser.TokenOperatorExpr;
        fn.FunctionHandler = () => seen = VmFScriptParser.IntValue(st.TArgv[0]);
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateFunction(st, result, 0, 4);
        Assert.Equal(4, seen);
        Assert.Equal(1, st.TArgc);
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

    [Fact]
    public void OpStructure_DotCallWithExtraArgument()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "pick", VmFScriptVariable.SvtFunction);
        var st = new VmFScriptParser.SpecRestState();
        var seen = 0;
        fn.FunctionHandler = () => seen = VmFScriptParser.IntValue(st.TArgv[1]);
        st.Expr.Script = script;
        st.Expr.GlobalScript = script;
        st.Expr.NumTokens = 7;
        st.Expr.Tokens[0] = "9";
        st.Expr.Tokens[1] = ".";
        st.Expr.Tokens[2] = "pick";
        st.Expr.Tokens[3] = "(";
        st.Expr.Tokens[4] = "1";
        st.Expr.Tokens[5] = ",";
        st.Expr.Tokens[6] = ")";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNumberExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[2] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[3] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[4] = VmFScriptParser.TokenNumberExpr;
        st.Expr.TokenType[5] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[6] = VmFScriptParser.TokenOperatorExpr;
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateExpression(st.Expr, result, 0, 6);
        Assert.Equal(1, seen);
        Assert.Equal(2, st.TArgc);
    }
}
