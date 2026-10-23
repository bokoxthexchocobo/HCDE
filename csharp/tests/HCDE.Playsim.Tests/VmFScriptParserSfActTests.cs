using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfActTests
{
    static AuthoritySimulation Room(int thingType = 3004) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = thingType }],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]>? fill = null)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 4)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        fill?.Invoke(argv);
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            ActorValue = v => v.Mobj,
            FloatValue = v => v.IntValue,
            Simulation = sim,
            GetActorClass = o => o,
        };
    }

    [Fact]
    public void SfObjAwaken_ActivatesTrigger()
    {
        var sim = Room();
        var monster = sim.Actors.Single(a => a is not PlayerPawn);
        monster.IsMonster = true;
        monster.Dormant = true;
        var ctx = Ctx(sim, 0);
        ctx.Script = new VmFScriptScript.FsScript { Trigger = monster };
        VmFScriptParser.SfObjAwaken(ctx);
        Assert.False(monster.Dormant);
    }

    [Fact]
    public void SfAmbientSound_QueuesGlobal()
    {
        var sim = Room();
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtString;
            argv[0].String = "world/spark";
        });
        VmFScriptParser.SfAmbientSound(ctx);
        Assert.Contains(sim.PendingSoundRequests, r => r.Sound == "world/spark");
    }

    [Fact]
    public void SfExitSecret_MarksSecretExit()
    {
        var sim = Room();
        VmFScriptParser.SfExitSecret(Ctx(sim, 0));
        Assert.True(sim.SecretExit);
    }

    [Fact]
    public void SfMobjValue_ReturnsMobj()
    {
        var sim = Room();
        var monster = sim.Actors.Single(a => a is not PlayerPawn);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = monster;
        });
        VmFScriptParser.SfMobjValue(ctx);
        Assert.Equal(VmFScriptParser.SvtMobj, ctx.TReturn.Type);
        Assert.Same(monster, ctx.TReturn.Mobj);
    }

    [Fact]
    public void SfStringValue_CoercesInt()
    {
        var sim = Room();
        var argv = new VmFScriptParser.ScriptValue[1];
        argv[0] = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtInt, IntValue = 42 };
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = argv,
            IntValue = v => v.IntValue,
            StringValue = v => v.IntValue.ToString(),
            Simulation = sim,
        };
        VmFScriptParser.SfStringValue(ctx);
        Assert.Equal("42", ctx.TReturn.String);
    }

    [Fact]
    public void SfIntValue_FromInt()
    {
        var ctx = Ctx(Room(), 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 9;
        });
        VmFScriptParser.SfIntValue(ctx);
        Assert.Equal(9, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfFixedValue_FromInt()
    {
        var ctx = Ctx(Room(), 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 3;
        });
        VmFScriptParser.SfFixedValue(ctx);
        Assert.Equal(3 * 65536.0, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfSetObjPosition_MovesActor()
    {
        var sim = Room();
        var monster = sim.Actors.Single(a => a is not PlayerPawn);
        var ctx = Ctx(sim, 3, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = monster;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 10;
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 20;
        });
        VmFScriptParser.SfSetObjPosition(ctx);
        Assert.Equal(10, monster.X.ToDouble());
        Assert.Equal(20, monster.Y.ToDouble());
    }

    [Fact]
    public void SfObjDead_DeadActor()
    {
        var sim = Room();
        var monster = sim.Actors.Single(a => a is not PlayerPawn);
        monster.Health = 0;
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = monster;
        });
        VmFScriptParser.SfObjDead(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfCheckCVar_ReturnsZero()
    {
        var ctx = Ctx(Room(), 0);
        VmFScriptParser.SfCheckCVar(ctx);
        Assert.Equal(0, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfObjType_MatchesActorTypes()
    {
        var sim = Room();
        var monster = sim.Actors.Single(a => a is not PlayerPawn);
        var table = new object?[] { null, monster };
        var argv = new VmFScriptParser.ScriptValue[1];
        argv[0] = new VmFScriptParser.ScriptValue { Type = VmFScriptParser.SvtMobj, Mobj = monster };
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 1,
            TArgv = argv,
            ActorValue = v => v.Mobj,
            Simulation = sim,
            ActorTypes = table,
            GetActorClass = o => o,
        };
        VmFScriptParser.SfObjType(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
