using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfTests
{
    [Fact]
    public void SfGoto_NonLabel_ScriptError()
    {
        var err = "";
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt }],
            ScriptError = s => err = s,
        };
        VmFScriptParser.SfGoto(ctx);
        Assert.Contains("label", err);
    }

    [Fact]
    public void SfGoto_Label_SetsRover()
    {
        var label = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtLabel };
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [label],
            LabelValue = _ => 42,
        };
        VmFScriptParser.SfGoto(ctx);
        Assert.Equal(42, ctx.Rover);
    }

    [Fact]
    public void SfReturn_ThrowsTerminator()
    {
        Assert.Throws<VmFScriptParser.FsTerminatorException>(VmFScriptParser.SfReturn);
    }

    [Fact]
    public void SfInclude_String_TruncatesToEightChars()
    {
        string? checkedName = null;
        var script = new VmFScriptScript.FsScript();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = script,
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtString, String = "abcdefghi" }],
            StringValue = v => v.String,
            CheckLumpForName = name =>
            {
                checkedName = name;
                return 7;
            },
        };
        VmFScriptParser.SfInclude(ctx);
        Assert.Equal("abcdefgh", checkedName);
        Assert.Single(ctx.FsPendingIncludes);
        Assert.Equal(7, ctx.FsPendingIncludes[0].LumpNum);
        Assert.Equal("abcdefgh", ctx.FsPendingIncludes[0].LumpName);
    }

    [Fact]
    public void SfPlayerInGame_ValidPlayer_ReturnsOne()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 0 }],
            IntValue = v => v.IntValue,
            PlayerInGame = _ => true,
        };
        VmFScriptParser.SfPlayerInGame(ctx);
        Assert.Equal(VmFScriptParser.SvtInt, ctx.TReturn.Type);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
