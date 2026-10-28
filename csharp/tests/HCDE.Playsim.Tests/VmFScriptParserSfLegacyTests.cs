using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfLegacyTests
{
    static AuthoritySimulation Room(bool withPlayer = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = withPlayer
            ? [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }]
            : [new LevelThing { Type = 3004 }, new LevelThing { Type = 3004 }],
        Lines = [new LevelLine { Index = 0, Flags = 0 }],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]> fill)
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
            PlayerInGame = _ => true,
            PlayerNum = o => o is PlayerPawn p ? p.PlayerNum : -1,
            PlayerMo = n => n == 0 ? sim.Players.FirstOrDefault() : null,
            ScriptError = _ => { },
        };
    }

    [Fact]
    public void SfSpawnMissile_SpawnsAtTarget()
    {
        var sim = Room();
        var shooter = sim.Actors.First(a => a is not PlayerPawn);
        var target = sim.Actors.Last(a => a is not PlayerPawn);
        shooter.SeeState = ActorStateMachine.Spawn;
        var ctx = Ctx(sim, 3, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = shooter;
            argv[1].Type = VmFScriptParser.SvtMobj;
            argv[1].Mobj = target;
            argv[2].Type = VmFScriptParser.SvtString;
            argv[2].String = "Rocket";
        });
        var before = sim.Actors.Count(a => a is ProjectileActor);
        VmFScriptParser.SfSpawnMissile(ctx);
        Assert.Equal(before + 1, sim.Actors.Count(a => a is ProjectileActor));
    }

    [Fact]
    public void SfMapThingNumExist_ValidIndex()
    {
        var sim = Room();
        var monster = sim.Actors.First(a => a is not PlayerPawn);
        sim.FraggleScriptSpawnedThings.Add(monster);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
        });
        VmFScriptParser.SfMapThingNumExist(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfMapThings_ReturnsCount()
    {
        var sim = Room();
        sim.FraggleScriptSpawnedThings.Add(sim.Actors.First());
        sim.FraggleScriptSpawnedThings.Add(sim.Actors.Last());
        var ctx = Ctx(sim, 0, _ => { });
        VmFScriptParser.SfMapThings(ctx);
        Assert.Equal(2, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfObjState_SetsSeeState()
    {
        var sim = Room();
        var monster = sim.Actors.First(a => a is not PlayerPawn);
        monster.SeeState = ActorStateMachine.Pain;
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 2;
        });
        ctx.Script = new VmFScriptScript.FsScript { Trigger = monster };
        VmFScriptParser.SfObjState(ctx);
        Assert.Equal(ActorStateMachine.Pain, monster.States.Current);
        Assert.Equal(ActorStateMachine.Pain, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfLineFlag_ReadWriteBlocking()
    {
        var sim = Room();
        var ctx = Ctx(sim, 3, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0;
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 1;
        });
        VmFScriptParser.SfLineFlag(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
        Assert.Equal(LevelLine.BlockingFlag, sim.Level.Lines[0].Flags);
    }

    [Fact]
    public void SfPlayerAddFrag_IncrementsFragCount()
    {
        var sim = Room(withPlayer: true);
        var player = sim.Players.First();
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = player.PlayerNum;
        });
        VmFScriptParser.SfPlayerAddFrag(ctx);
        Assert.Equal(1, player.FragCount);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
