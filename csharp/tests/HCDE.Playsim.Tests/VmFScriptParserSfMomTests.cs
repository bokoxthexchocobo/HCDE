using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfMomTests
{
    [Fact]
    public void SfPointToDist_AxisDistance()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 4,
            TArgv =
            [
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
            ],
            FloatValue = v => v.IntValue,
        };
        ctx.TArgv[0].IntValue = 0;
        ctx.TArgv[1].IntValue = 0;
        ctx.TArgv[2].IntValue = 3;
        ctx.TArgv[3].IntValue = 4;
        VmFScriptParser.SfPointToDist(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(5.0, ctx.TReturn.FixedValue, 3);
    }

    [Fact]
    public void SfPointToAngle_East()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 4,
            TArgv =
            [
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
            ],
            FloatValue = v => v.IntValue,
        };
        ctx.TArgv[0].IntValue = 0;
        ctx.TArgv[1].IntValue = 0;
        ctx.TArgv[2].IntValue = 1;
        ctx.TArgv[3].IntValue = 0;
        VmFScriptParser.SfPointToAngle(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(0.0, ctx.TReturn.FixedValue, 3);
    }
}
