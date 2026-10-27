using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfCtrlTests
{
    [Fact]
    public void SfGoto_JumpsToLabel()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgv = [new VmFScriptParser.ScriptValue()],
            IntValue = v => v.IntValue,
            LabelValue = v => v.IntValue,
        };
        ctx.TArgc = 1;
        ctx.TArgv[0].Type = VmFScriptParser.SvtLabel;
        ctx.TArgv[0].IntValue = 42;
        VmFScriptParser.SfGoto(ctx);
        Assert.Equal(42, ctx.Rover);
    }

    [Fact]
    public void SfReturn_ThrowsTerminator() =>
        Assert.Throws<FsTerminatorException>(() => VmFScriptParser.SfReturn(new VmFScriptParser.ParserContext()));

    [Fact]
    public void SfBeep_StartsChatSound()
    {
        StartSoundContext? captured = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            FindSound = _ => 7,
            StartSoundEngine = s =>
            {
                captured = s;
                return new object();
            },
        };
        VmFScriptParser.SfBeep(ctx);
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.SoundId);
        Assert.Equal(1f, captured.Volume);
        Assert.Equal(1.001f, captured.Attenuation);
    }

    [Fact]
    public void SfClock_ReturnsCentiseconds()
    {
        var ctx = new VmFScriptParser.ParserContext { GameTic = () => GameTicClock.TicRate };
        VmFScriptParser.SfClock(ctx);
        Assert.Equal(100, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfMessage_PrintsHigh()
    {
        string? printed = null;
        var trigger = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = trigger },
            PrintHigh = s => printed = s,
            LocalView = new VmPMobjLocalViewTouch.LocalViewQuery { ConsoleCamera = trigger },
            FormatStringFromArgs = (_, _) => "fmt",
            TArgv = [new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 1;
        VmFScriptParser.SfMessage(ctx);
        Assert.Equal("fmt\n", printed);
    }

    [Fact]
    public void SfPlayerMsg_PrintsHighForPlayer()
    {
        string? printed = null;
        var mo = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            PrintHigh = s => printed = s,
            PlayerInGame = _ => true,
            PlayerMo = _ => mo,
            LocalView = new VmPMobjLocalViewTouch.LocalViewQuery { ConsoleCamera = mo },
            FormatStringFromArgs = (_, _) => "fmt",
            IntValue = v => v.IntValue,
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 2;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 0;
        VmFScriptParser.SfPlayerMsg(ctx);
        Assert.Equal("fmt\n", printed);
    }

    [Fact]
    public void SfExitLevel_MarksExited()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var ctx = new VmFScriptParser.ParserContext { Simulation = sim };
        VmFScriptParser.SfExitLevel(ctx);
        Assert.True(sim.Exited);
    }

    [Fact]
    public void SfTip_MidPrintsForLocalView()
    {
        string? printed = null;
        var trigger = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = trigger },
            MidPrint = s => printed = s,
            LocalView = new VmPMobjLocalViewTouch.LocalViewQuery { ConsoleCamera = trigger },
            FormatStringFromArgs = (_, _) => "tip",
            TArgv = [new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 1;
        VmFScriptParser.SfTip(ctx);
        Assert.Equal("tip", printed);
    }

    [Fact]
    public void SfTimedTip_RestoresConMidTime()
    {
        float midtime = 2.5f;
        string? printed = null;
        var trigger = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = trigger },
            MidPrint = s => printed = s,
            GetConMidTime = () => midtime,
            SetConMidTime = v => midtime = v,
            LocalView = new VmPMobjLocalViewTouch.LocalViewQuery { ConsoleCamera = trigger },
            FormatStringFromArgs = (_, _) => "timed",
            IntValue = v => v.IntValue,
            TArgv = [new VmFScriptParser.ScriptValue(), new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 2;
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 500;
        VmFScriptParser.SfTimedTip(ctx);
        Assert.Equal("timed", printed);
        Assert.Equal(2.5f, midtime);
    }

    [Fact]
    public void SfInclude_ParsesStringLumpName()
    {
        string? included = null;
        var script = new VmFScriptScript.FsScript();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = script,
            ParseInclude = (s, lump) => included = lump,
            StringValue = v => v.String,
            TArgv = [new VmFScriptParser.ScriptValue()],
        };
        ctx.TArgc = 1;
        ctx.TArgv[0].Type = VmFScriptParser.SvtString;
        ctx.TArgv[0].String = "MYINCLUDE";
        VmFScriptParser.SfInclude(ctx);
        Assert.Equal("MYINCLUD", included);
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
