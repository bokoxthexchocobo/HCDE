using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserTests
{
    [Fact]
    public void CheckArgs_Insufficient_ReturnsFalse()
    {
        var ctx = new VmFScriptParser.ParserContext { TArgc = 0, TFunc = "foo" };
        var err = "";
        ctx.ScriptError = s => err = s;
        Assert.False(VmFScriptParser.CheckArgs(ctx, 2));
        Assert.Contains("Insufficient", err);
    }

    [Fact]
    public void GetPlayerNum_NonPlayerMobj_ReturnsMinusOne()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            ActorValue = _ => new object(),
            PlayerNum = _ => -1,
        };
        var arg = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtMobj };
        Assert.Equal(-1, VmFScriptParser.GetPlayerNum(ctx, arg));
    }

    [Fact]
    public void LoopingSection_PicksInnermostLoop()
    {
        var outer = new VmFScriptParser.FsSection { Type = VmFScriptParser.StLoop, StartIndex = 0, EndIndex = 100 };
        var inner = new VmFScriptParser.FsSection { Type = VmFScriptParser.StLoop, StartIndex = 10, EndIndex = 20 };
        var script = new VmFScriptScript.FsScript();
        script.Sections[0] = outer;
        outer.Next = inner;
        var ctx = new VmFScriptParser.ParserContext { Script = script, Rover = 15 };
        Assert.Same(inner, VmFScriptParser.LoopingSection(ctx));
    }

    [Fact]
    public void SfBreak_AdvancesRoverPastLoopEnd()
    {
        var section = new VmFScriptParser.FsSection { Type = VmFScriptParser.StLoop, StartIndex = 1, EndIndex = 10 };
        var script = new VmFScriptScript.FsScript();
        script.Sections[0] = section;
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = script,
            Rover = 5,
            SectionEnd = _ => 10,
        };
        VmFScriptParser.SfBreak(ctx);
        Assert.Equal(11, ctx.Rover);
    }
}
