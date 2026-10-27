using HCDE.Gamedata;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfObjTests
{
    [Fact]
    public void SfObjX_Trigger_ReturnsCoord()
    {
        var mo = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = mo },
            ActorX = o => o == mo ? 12.5 : 0,
        };
        VmFScriptParser.SfObjX(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(12.5, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfPushThing_ThrustsActor()
    {
        var actor = new Actor { Id = 1 };
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 3,
            TArgv =
            [
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtMobj, Mobj = actor },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 0 },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 4 },
            ],
            ActorValue = v => v.Mobj,
            FloatValue = v => v.IntValue,
        };
        VmFScriptParser.SfPushThing(ctx);
        Assert.NotEqual(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void SfTeleport_OneArg_UsesTrigger()
    {
        var mo = new object();
        int? tag = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 7 }],
            IntValue = v => v.IntValue,
            Script = new VmFScriptScript.FsScript { Trigger = mo },
            EvTeleportFog = (t, m) => { tag = t; Assert.Same(mo, m); },
        };
        VmFScriptParser.SfTeleport(ctx);
        Assert.Equal(7, tag);
    }
}
