using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfTexDoorTests
{
    static AuthoritySimulation Room(int sectorTag = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Things = [new LevelThing { Type = 3004 }],
        Sectors =
        [
            new LevelSector { Index = 0, Tag = sectorTag, FloorHeight = 0, CeilingHeight = 100 },
            new LevelSector { Index = 1, FloorHeight = 0, CeilingHeight = 108 },
        ],
        Sides = [new LevelSide { Index = 0, Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
        Lines = [new LevelLine { Index = 0, SideFront = 0, SideBack = 1 }],
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
            Simulation = sim,
            FraggleThinker = new VmFScriptThinker.ThinkerState
            {
                SectorsByTag = tag => sim.Level.Sectors.Where(s => s.MatchesTag(tag)).Select(s => s.Index).ToList(),
            },
        };
    }

    [Fact]
    public void SfSectorColormap_IsNoOp()
    {
        var sim = Room();
        VmFScriptParser.SfSectorColormap(Ctx(sim, 0, _ => { }));
    }

    [Fact]
    public void SfCeilingTexture_SetsAndReturnsName()
    {
        var sim = Room(3);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 3;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "CEIL1";
        });
        VmFScriptParser.SfCeilingTexture(ctx);
        Assert.Equal("CEIL1", sim.Level.Sectors[0].CeilingPic);
        Assert.Equal("CEIL1", ctx.TReturn.String);
    }

    [Fact]
    public void SfOpenDoor_StartsDoorMotion()
    {
        var sim = Room(9);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 9;
        });
        VmFScriptParser.SfOpenDoor(ctx);
        Assert.Contains(sim.Motions, m => m.Kind == MotionKind.Door && m.SectorIndex == 0);
    }

    [Fact]
    public void SfCloseDoor_StartsClosingDoor()
    {
        var sim = Room(11);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 11;
        });
        VmFScriptParser.SfCloseDoor(ctx);
        Assert.Contains(sim.Motions, m => m.Kind == MotionKind.Door && m.SectorIndex == 0 && m.Closing);
    }
}
