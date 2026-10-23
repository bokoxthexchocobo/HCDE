namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfCtrlTests
{
    static VmFScriptParser.ParserContext NewCtx(
        Action<VmFScriptParser.ParserContext>? extra = null)
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = new object() },
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            LabelValue = v => v.IntValue,
            FormatStringFromArgs = (_, _) => "fmt",
            PlayerInGame = i => i == 0,
            PlayerMo = i => i == 0 ? new object() : null,
            TriggerLocalView = _ => true,
        };
        extra?.Invoke(ctx);
        return ctx;
    }

    [Fact]
    public void SfGoto_JumpsToLabel()
    {
        var ctx = NewCtx();
        ctx.TArgc = 1;
        ctx.TArgv[0].Type = VmFScriptParser.SvtLabel;
        ctx.TArgv[0].IntValue = 42;
        VmFScriptParser.SfGoto(ctx);
        Assert.Equal(42, ctx.Rover);
    }

    [Fact]
    public void SfReturn_ThrowsTerminator() =>
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfReturn(NewCtx()));

    [Fact]
    public void SfInclude_CallsParseInclude()
    {
        string? seen = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            ParseInclude = s => seen = s,
            TArgv = [new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
        };
        ctx.TArgc = 1;
        ctx.TArgv[0].Type = VmFScriptParser.SvtString;
        ctx.TArgv[0].String = "map01";
        VmFScriptParser.SfInclude(ctx);
        Assert.Equal("map01", seen);
    }

    [Fact]
    public void SfInput_PrintsBoldNotice()
    {
        string? msg = null;
        var ctx = new VmFScriptParser.ParserContext { PrintBold = s => msg = s };
        VmFScriptParser.SfInput(ctx);
        Assert.Contains("not available", msg);
    }

    [Fact]
    public void SfBeep_StartsChatSound()
    {
        var beeped = false;
        var ctx = new VmFScriptParser.ParserContext { PlayChatBeep = () => beeped = true };
        VmFScriptParser.SfBeep(ctx);
        Assert.True(beeped);
    }

    [Fact]
    public void SfClock_ReturnsCentiseconds()
    {
        var ctx = new VmFScriptParser.ParserContext { GameTic = () => GameTicClock.TicRate };
        VmFScriptParser.SfClock(ctx);
        Assert.Equal(100, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfExitLevel_InvokesHost()
    {
        var exited = false;
        var ctx = new VmFScriptParser.ParserContext { ExitLevel = () => exited = true };
        VmFScriptParser.SfExitLevel(ctx);
        Assert.True(exited);
    }

    [Fact]
    public void SfTip_MidPrintsWhenLocalView()
    {
        string? printed = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = new object() },
            MidPrint = s => printed = s,
            FormatStringFromArgs = (_, _) => "fmt",
            TriggerLocalView = _ => true,
            TArgv = [new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 1;
        VmFScriptParser.SfTip(ctx);
        Assert.Equal("fmt", printed);
    }

    [Fact]
    public void SfTimedTip_AdjustsMidTime()
    {
        double mid = 1;
        var ctx = new VmFScriptParser.ParserContext
        {
            ConMidTime = () => mid,
            SetConMidTime = v => mid = v,
            FormatStringFromArgs = (_, _) => "fmt",
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
        };
        ctx.TArgc = 2;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 250;
        VmFScriptParser.SfTimedTip(ctx);
        Assert.Equal(1, mid);
    }

    [Fact]
    public void SfPlayerTip_MidPrintsForPlayer()
    {
        string? printed = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            MidPrint = s => printed = s,
            FormatStringFromArgs = (_, _) => "fmt",
            PlayerInGame = _ => true,
            PlayerMo = _ => new object(),
            TriggerLocalView = _ => true,
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
        };
        ctx.TArgc = 2;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 0;
        VmFScriptParser.SfPlayerTip(ctx);
        Assert.Equal("fmt", printed);
    }

    [Fact]
    public void SfMessage_PrintsHigh()
    {
        string? printed = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = new object() },
            PrintHigh = s => printed = s,
            FormatStringFromArgs = (_, _) => "fmt",
            TriggerLocalView = _ => true,
            TArgv = [new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 1;
        VmFScriptParser.SfMessage(ctx);
        Assert.Equal("fmt", printed);
    }

    [Fact]
    public void SfPlayerMsg_PrintsHighForPlayer()
    {
        string? printed = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            PrintHigh = s => printed = s,
            FormatStringFromArgs = (_, _) => "fmt",
            PlayerInGame = _ => true,
            PlayerMo = _ => new object(),
            TriggerLocalView = _ => true,
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
        };
        ctx.TArgc = 2;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 0;
        VmFScriptParser.SfPlayerMsg(ctx);
        Assert.Equal("fmt", printed);
    }

    [Fact]
    public void SfPlayerInGame_ReturnsSlotState()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgv = [new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
            PlayerInGame = _ => true,
        };
        ctx.TArgc = 1;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 0;
        VmFScriptParser.SfPlayerInGame(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
