using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfObjStateTests
{
    [Fact]
    public void SfObjHealth_Trigger_ReturnsHealth()
    {
        var mo = new object();
        var ctx = new VmFScriptParser.ParserContext
        {
            Script = new VmFScriptScript.FsScript { Trigger = mo },
            ActorHealth = o => o == mo ? 42 : 0,
        };
        VmFScriptParser.SfObjHealth(ctx);
        Assert.Equal(42, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfDamageObj_Trigger_PassesDamage()
    {
        var mo = new object();
        int? dmg = null;
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = [new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 5 }],
            IntValue = v => v.IntValue,
            Script = new VmFScriptScript.FsScript { Trigger = mo },
            DamageActor = (_, _, _, d) => dmg = d,
        };
        VmFScriptParser.SfDamageObj(ctx);
        Assert.Equal(5, dmg);
    }

    [Fact]
    public void SfObjFlag_ReadSetBit()
    {
        var mo = new object();
        uint flags = 0;
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 3,
            TArgv =
            [
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtMobj, Mobj = mo },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 2 },
                new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 1 },
            ],
            IntValue = v => v.IntValue,
            ActorValue = v => v.Mobj,
            GetActorFlags = _ => flags,
            SetActorFlags = (_, f) => flags = f,
        };
        VmFScriptParser.SfObjFlag(ctx);
        Assert.Equal(4u, flags);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
