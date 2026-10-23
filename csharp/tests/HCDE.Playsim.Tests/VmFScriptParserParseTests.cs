using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserParseTests
{
    static VmFScriptParser.ScriptParseState State(string text, Action<string>? onError = null)
    {
        var data = (text + "\0").ToCharArray();
        return new VmFScriptParser.ScriptParseState
        {
            Data = data,
            Script = new VmFScriptScript.FsScript(),
            GlobalScript = new VmFScriptScript.FsScript(),
            ScriptError = onError ?? (_ => throw new InvalidOperationException("script error")),
        };
    }

    [Fact]
    public void GetTokens_ParsesIntegerExpression()
    {
        var st = State("2 + 3;");
        VmFScriptParser.GetTokens(st, 0);
        Assert.Equal(3, st.NumTokens);
        Assert.Equal("2", st.Tokens[0]);
        Assert.Equal("+", st.Tokens[1]);
        Assert.Equal("3", st.Tokens[2]);
    }

    [Fact]
    public void GetTokens_ParsesStringLiteral()
    {
        var st = State("\"hi\";");
        VmFScriptParser.GetTokens(st, 0);
        Assert.Equal(1, st.NumTokens);
        Assert.Equal("hi", st.Tokens[0]);
        Assert.Equal(VmFScriptParser.TokenStringParse, st.TokenType[0]);
    }

    [Fact]
    public void GetTokens_DoubleEqualsOperator_TwoTokens()
    {
        var st = State("a==b;");
        VmFScriptParser.GetTokens(st, 0);
        Assert.Equal(3, st.NumTokens);
        Assert.Equal("==", st.Tokens[1]);
    }

    [Fact]
    public void GetTokens_MissingSemicolon_ReportsError()
    {
        var err = "";
        var st = State("1 + 2", msg => err = msg);
        VmFScriptParser.GetTokens(st, 0);
        Assert.Contains("missing ';'", err);
    }

    [Fact]
    public void RunScript_EvaluatesExpression()
    {
        var st = State("1 + 2;");
        VmFScriptParser.RunScript(st, st.Data.Length);
        Assert.True(st.Rover > 0);
    }

    [Fact]
    public void RunScript_TwoStatements_AdvancesPastBoth()
    {
        var st = State("1; 2;");
        VmFScriptParser.RunScript(st, st.Data.Length);
        Assert.True(st.Rover >= st.Data.Length - 1);
    }
}
