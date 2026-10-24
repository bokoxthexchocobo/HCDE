namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfTrigTests
{
    static VmFScriptParser.ParserContext Ctx(int argc, Action<VmFScriptParser.ScriptValue[]> fill)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 2)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        fill(argv);
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            FloatValue = v => v.IntValue,
            IntValue = v => v.IntValue,
        };
    }

    [Fact]
    public void SfSin_ReturnsSin()
    {
        var ctx = Ctx(1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
        });
        VmFScriptParser.SfSin(ctx);
        Assert.Equal(0, ctx.TReturn.FixedValue, 3);
    }

    [Fact]
    public void SfPow_Squares()
    {
        var ctx = Ctx(2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 3;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 2;
        });
        VmFScriptParser.SfPow(ctx);
        Assert.Equal(9, ctx.TReturn.FixedValue, 3);
    }

    [Fact]
    public void SfFloor_TruncatesFixed()
    {
        var ctx = Ctx(1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtFixed;
            argv[0].FixedValue = (1 << 16) + (1 << 15);
        });
        VmFScriptParser.SfFloor(ctx);
        Assert.Equal(1 << 16, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfSetCorona_WrongArgCountErrors()
    {
        var errors = 0;
        var argv = new VmFScriptParser.ScriptValue[2];
        argv[0] = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt };
        argv[1] = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt };
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 2,
            TArgv = argv,
            IntValue = v => v.IntValue,
            ScriptError = _ => errors++,
        };
        VmFScriptParser.SfSetCorona(ctx);
        Assert.Equal(1, errors);
    }

    [Fact]
    public void SfSetCorona_ThreeArgsReturnsZero()
    {
        var ctx = Ctx(3, argv =>
        {
            for (var i = 0; i < 3; i++)
                argv[i].Type = VmFScriptParser.SvtInt;
        });
        VmFScriptParser.SfSetCorona(ctx);
        Assert.Equal(0, ctx.TReturn.IntValue);
    }
}
