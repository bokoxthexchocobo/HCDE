using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfTexDoorTests
{
    static AuthoritySimulation Room(int sectorTag = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
        Sectors = [new LevelSector { Index = 0, Tag = sectorTag, FloorHeight = 0, CeilingHeight = 128, CeilingPic = "CEIL1" }],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]> fill)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 3)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        fill(argv);
        var thinker = new VmFScriptThinker.ThinkerState
        {
            SectorsByTag = tag =>
            {
                var list = new List<int>();
                for (var i = 0; i < sim.Level.Sectors.Count; i++)
                    if (sim.Level.Sectors[i].MatchesTag(tag))
                        list.Add(i);
                return list;
            },
        };
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            ActorValue = v => v.Mobj,
            Simulation = sim,
            FraggleThinker = thinker,
            ScriptError = _ => { },
        };
    }

    [Fact]
    public void SfCeilingTexture_SetsAndReturnsName()
    {
        var sim = Room();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].IntValue = 1;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "FLAT1";
        });
        VmFScriptParser.SfCeilingTexture(ctx);
        Assert.Equal("FLAT1", ctx.TReturn.String);
    }

    [Fact]
    public void SfOpenDoor_StartsDoorMotion()
    {
        var sim = Room();
        var ctx = Ctx(sim, 1, argv => argv[0].IntValue = 1);
        VmFScriptParser.SfOpenDoor(ctx);
        Assert.True(sim.Motions.Count > 0);
    }

    [Fact]
    public void SfCloseDoor_StartsCloseMotion()
    {
        var sim = Room();
        var ctx = Ctx(sim, 1, argv => argv[0].IntValue = 1);
        VmFScriptParser.SfCloseDoor(ctx);
        Assert.True(sim.Motions.Count > 0);
    }
}
