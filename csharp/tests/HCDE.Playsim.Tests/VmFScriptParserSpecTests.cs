using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSpecTests
{
    private static VmFScriptParser.SpecState NewState(
        VmFScriptScript.FsScript script,
        VmFScriptParser.FsSection? section = null)
    {
        var st = new VmFScriptParser.SpecState
        {
            Script = script,
            GlobalScript = script,
            Section = section,
            SectionEnd = s => s.EndIndex,
            SectionLoop = s => s.LoopIndex,
        };
        for (var i = 0; i < st.TArgv.Length; i++)
            st.TArgv[i] = new VmFScriptParser.ScriptValue();
        return st;
    }

    [Fact]
    public void SpecBrace_LoopSection_RewindsRover()
    {
        var section = new VmFScriptParser.FsSection
        {
            Type = VmFScriptPrepro.StLoop,
            LoopIndex = 42,
            EndIndex = 99,
        };
        var script = new VmFScriptScript.FsScript();
        var st = NewState(script, section);
        st.BraceType = VmFScriptParser.BracketClose;
        st.Rover = 10;
        VmFScriptParser.SpecBrace(st);
        Assert.Equal(42, st.Rover);
    }

    [Fact]
    public void SpecIf_FalseWithBraces_SkipsToSectionEnd()
    {
        var section = new VmFScriptParser.FsSection { StartIndex = 0, EndIndex = 50 };
        var st = NewState(new VmFScriptScript.FsScript(), section);
        st.BraceType = VmFScriptParser.BracketOpen;
        st.NumTokens = 4;
        st.Rover = 5;
        st.Tokens[0] = "if";
        st.Tokens[1] = "(";
        st.Tokens[2] = "0";
        st.Tokens[3] = ")";
        st.TokenType[0] = VmFScriptParser.TokenName;
        st.TokenType[1] = VmFScriptParser.TokenOperator;
        st.TokenType[2] = VmFScriptParser.TokenNumber;
        st.TokenType[3] = VmFScriptParser.TokenOperator;
        VmFScriptParser.SpecIf(st);
        Assert.Equal(51, st.Rover);
    }

    [Fact]
    public void SpecVariable_IntDecl_CreatesVariable()
    {
        var script = new VmFScriptScript.FsScript();
        var st = NewState(script);
        st.NumTokens = 2;
        st.Tokens[0] = "int";
        st.Tokens[1] = "foo";
        st.TokenType[0] = VmFScriptParser.TokenName;
        st.TokenType[1] = VmFScriptParser.TokenName;
        Assert.True(VmFScriptParser.SpecVariable(st));
        Assert.NotNull(VmFScriptVariable.VariableForName(script, "foo"));
    }

    [Fact]
    public void EvaluateFunction_InvokesRegisteredHandler()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "foo", VmFScriptVariable.SvtFunction);
        var ran = false;
        fn.FunctionHandler = () => ran = true;
        var st = NewState(script);
        st.GlobalScript = script;
        st.NumTokens = 3;
        st.InvokeSfHandler = action => action();
        st.Tokens[0] = "foo";
        st.Tokens[1] = "(";
        st.Tokens[2] = ")";
        st.TokenType[0] = VmFScriptParser.TokenFunction;
        st.TokenType[1] = VmFScriptParser.TokenOperator;
        st.TokenType[2] = VmFScriptParser.TokenOperator;
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.EvaluateFunction(st, result, 0, 2);
        Assert.True(ran);
    }

    [Fact]
    public void OpStructure_InvokesDotHandler()
    {
        var script = new VmFScriptScript.FsScript();
        var fn = VmFScriptVariable.NewVariable(script, "bar", VmFScriptVariable.SvtFunction);
        var ran = false;
        fn.FunctionHandler = () => ran = true;
        var st = NewState(script);
        st.GlobalScript = script;
        st.NumTokens = 5;
        st.InvokeSfHandler = action => action();
        st.Tokens[0] = "1";
        st.Tokens[1] = ".";
        st.Tokens[2] = "bar";
        st.Tokens[3] = "(";
        st.Tokens[4] = ")";
        st.TokenType[0] = VmFScriptParser.TokenNumber;
        st.TokenType[1] = VmFScriptParser.TokenOperator;
        st.TokenType[2] = VmFScriptParser.TokenName;
        st.TokenType[3] = VmFScriptParser.TokenOperator;
        st.TokenType[4] = VmFScriptParser.TokenOperator;
        var result = new VmFScriptParser.ScriptValue();
        VmFScriptParser.OpStructure(st, result, 0, 1, 4);
        Assert.True(ran);
    }
}
