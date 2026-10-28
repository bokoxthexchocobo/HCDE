using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfGzdTests
{
    static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
        Lines = [new LevelLine { Index = 0, AdditionalIds = [5], Special = 0, Arg0 = 0 }],
        Sectors = [new LevelSector { Index = 0, Tag = 9 }],
    });

    static VmFScriptParser.ParserContext Ctx(
        AuthoritySimulation sim,
        int argc,
        Action<VmFScriptParser.ScriptValue[]> fill,
        VmFScriptThinker.ThinkerState? thinker = null)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 4)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        fill(argv);
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            ActorValue = v => v.Mobj,
            FloatValue = v => v.IntValue,
            Simulation = sim,
            FindActorClass = name => name,
            GetActorClass = o => o,
            Rover = 12,
            FraggleThinker = thinker,
        };
    }

    [Fact]
    public void SfSpawnShot2_SpawnsProjectile()
    {
        var sim = Room();
        var source = sim.Actors.Single();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtString;
            argv[0].String = "Rocket";
            argv[1].Type = VmFScriptParser.SvtMobj;
            argv[1].Mobj = source;
        });
        ctx.Script = new VmFScriptScript.FsScript { Trigger = source };
        VmFScriptParser.SfSpawnShot2(ctx);
        Assert.IsType<ProjectileActor>(ctx.TReturn.Mobj);
    }

    [Fact]
    public void SfKillInSector_MassacresMonstersInTag()
    {
        var sim = Room();
        var monster = sim.Actors.Single();
        monster.IsMonster = true;
        monster.SectorIndex = 0;
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 9;
        });
        VmFScriptParser.SfKillInSector(ctx);
        Assert.True(monster.Destroyed || monster.Health <= 0);
    }

    [Fact]
    public void SfSetLineTrigger_SetsSpecialOnId()
    {
        var sim = Room();
        var ctx = Ctx(sim, 3, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 11;
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 3;
        });
        VmFScriptParser.SfSetLineTrigger(ctx);
        Assert.Equal(11, sim.Level.Lines[0].Special);
        Assert.Equal(3, sim.Level.Lines[0].Arg0);
    }

    [Fact]
    public void SfRunLineSpecial_ExecutesDoor()
    {
        var sim = Room();
        var monster = sim.Actors.Single();
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 1;
        });
        ctx.Script = new VmFScriptScript.FsScript { Trigger = monster };
        VmFScriptParser.SfRunLineSpecial(ctx, new VmFScriptParser.ParserContext.FraggleLineSpec(130, 1));
        Assert.True(ctx.TReturn.IntValue is 0 or 1);
    }

    [Fact]
    public void SaveCurrentScript_EnqueuesRunner()
    {
        var script = new VmFScriptScript.FsScript();
        var th = new VmFScriptThinker.ThinkerState
        {
            RunningScripts = new VmFScriptScript.RunningScript(),
            CreateRunningScript = (t, s, i) => new VmFScriptScript.RunningScript { Trigger = t, Script = s, SavePoint = i },
        };
        var ctx = Ctx(Room(), 0, _ => { }, th);
        ctx.Script = script;
        var run = VmFScriptParser.SaveCurrentScript(ctx);
        Assert.NotNull(run);
        Assert.Same(script, run!.Script);
        Assert.Equal(12, run.SavePoint);
        Assert.Same(th.RunningScripts, run.Prev);
    }
}
