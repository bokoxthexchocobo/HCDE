using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfLevelTests
{
    static AuthoritySimulation Room(int? levelNum = 5) => AuthoritySimulation.Start(new PlayLevel
    {
        LevelNumber = levelNum,
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001 }],
        Sectors = [new LevelSector { Index = 0, Tag = 7 }],
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
            GetActorClass = o => o is Actor a ? a.DoomEdNum : o,
        };
    }

    [Fact]
    public void SfLevelNum_ReadsMapLevelNumber()
    {
        var ctx = Ctx(Room(12), 0, _ => { });
        VmFScriptParser.SfLevelNum(ctx);
        Assert.Equal(12, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfMobjRadius_ReadWrite()
    {
        var sim = Room();
        var monster = sim.Actors.First(a => a.DoomEdNum == 3004);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = monster;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 32;
        });
        VmFScriptParser.SfMobjRadius(ctx);
        Assert.Equal(32, monster.Radius.ToDouble());
        Assert.Equal(32, ctx.TReturn.FixedValue, 3);
    }

    [Fact]
    public void SfThingCount_CountsZombiemen()
    {
        var sim = Room();
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtString;
            argv[0].String = "Zombieman";
        });
        VmFScriptParser.SfThingCount(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfSetColor_TaggedSector()
    {
        var sim = Room();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 7;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0x112233;
        });
        VmFScriptParser.SfSetColor(ctx);
        Assert.Equal(0x112233, sim.Level.Sectors[0].ColormapLightColor);
    }
}
