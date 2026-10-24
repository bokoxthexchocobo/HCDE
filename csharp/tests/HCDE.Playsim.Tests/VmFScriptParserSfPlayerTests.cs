using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfPlayerTests
{
    [Fact]
    public void SfPlayerName_NoTrigger_ScriptError()
    {
        var err = "";
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript(),
            ScriptError = s => err = s,
        };
        VmFScriptParser.SfPlayerName(ctx);
        Assert.Contains("player", err);
    }

    [Fact]
    public void SfPlayerName_ValidPlayer_ReturnsName()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 0 }],
            IntValue = v => v.IntValue,
            PlayerInGame = _ => true,
            PlayerDisplayName = _ => "Alice",
        };
        VmFScriptParser.SfPlayerName(ctx);
        Assert.Equal(VmFScriptParser.SvtString, ctx.TReturn.Type);
        Assert.Equal("Alice", ctx.TReturn.String);
    }

    [Fact]
    public void SfPlayer_FromMobj_ReturnsPlayerNum()
    {
        var mo = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtMobj, Mobj = mo }],
            ActorValue = v => v.Mobj,
            PlayerNum = o => o == mo ? 2 : -1,
        };
        VmFScriptParser.SfPlayer(ctx);
        Assert.Equal(2, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfSpawn_FailedLocation_ClearsReturn()
    {
        object? destroyed = null;
        var spawned = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            TFunc = "spawn",
            TArgc = 3,
            TArgv =
            [
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtString, String = "Imp" },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 1 },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 2 },
            ],
            IntValue = v => v.IntValue,
            FloatValue = v => v.IntValue,
            FindActorClass = _ => new object(),
            SpawnActor = (_, _, _, _) => spawned,
            TestMobjLocation = _ => false,
            DestroyFailedSpawn = m => destroyed = m,
        };
        VmFScriptParser.SfSpawn(ctx);
        Assert.Null(ctx.TReturn.Mobj);
        Assert.Same(spawned, destroyed);
    }
}
