using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserParseTests
{
    [Fact]
    public void GetTokens_ParsesIntegerExpression()
    {
        var data = "2 + 3;\0".ToCharArray();
        var script = new VmFScriptScript.FsScript();
        var st = new VmFScriptParser.ScriptParseState
        {
            Data = data,
            Script = script,
            GlobalScript = script,
            ScriptError = msg => throw new InvalidOperationException(msg),
        };
        VmFScriptParser.GetTokens(st, 0);
        Assert.Equal(3, st.NumTokens);
        Assert.Equal("2", st.Tokens[0]);
        Assert.Equal("+", st.Tokens[1]);
        Assert.Equal("3", st.Tokens[2]);
    }

    [Fact]
    public void RunScript_EvaluatesExpression()
    {
        var data = "1 + 2;\0".ToCharArray();
        var script = new VmFScriptScript.FsScript();
        var st = new VmFScriptParser.ScriptParseState
        {
            Data = data,
            Script = script,
            GlobalScript = script,
            ScriptError = _ => { },
        };
        VmFScriptParser.RunScript(st, data.Length);
        Assert.True(st.Rover > 0);
    }
}
