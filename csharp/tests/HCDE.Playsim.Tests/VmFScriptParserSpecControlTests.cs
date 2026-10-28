using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSpecControlTests
{
    [Fact]
    public void SpecIf_FalseWithBraces_SkipsToSectionEnd()
    {
        var section = new VmFScriptParser.FsSection { StartIndex = 0, EndIndex = 50 };
        var st = new VmFScriptParser.SpecControlState();
        st.Expr.Script = new VmFScriptScript.FsScript();
        st.Section = section;
        st.BraceType = VmFScriptParser.BracketOpen;
        st.Expr.NumTokens = 4;
        st.Rover = 5;
        st.Expr.Tokens[0] = "if";
        st.Expr.Tokens[1] = "(";
        st.Expr.Tokens[2] = "0";
        st.Expr.Tokens[3] = ")";
        st.Expr.TokenType[0] = VmFScriptParser.TokenNameExpr;
        st.Expr.TokenType[1] = VmFScriptParser.TokenOperatorExpr;
        st.Expr.TokenType[2] = VmFScriptParser.TokenNumberExpr;
        st.Expr.TokenType[3] = VmFScriptParser.TokenOperatorExpr;
        VmFScriptParser.SpecIf(st);
        Assert.Equal(51, st.Rover);
    }
}
