using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfCeilLightTests
{
    static AuthoritySimulation Room(int sectorTag = 1, short light = 160) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
        Sectors = [new LevelSector { Index = 0, Tag = sectorTag, FloorHeight = 0, CeilingHeight = 128, LightLevel = light }],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]> fill)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 4)];
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
            FloatValue = v => v.IntValue,
            Simulation = sim,
            FraggleThinker = thinker,
            Script = new VmFScriptScript.FsScript(),
        };
    }

    [Fact]
    public void SfCeilingHeight_ReadsSectorCeiling()
    {
        var sim = Room(4);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 4;
        });
        VmFScriptParser.SfCeilingHeight(ctx);
        Assert.Equal(128, ctx.TReturn.FixedValue / 65536.0, 3);
    }

    [Fact]
    public void SfCeilingHeight_WriteUpdatesPlaysimCeiling()
    {
        var sim = Room(2);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 2;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 96;
        });
        VmFScriptParser.SfCeilingHeight(ctx);
        Assert.Equal(96, sim.CeilingOf(0), 3);
    }

    [Fact]
    public void SfMoveCeiling_StartsSectorMotion()
    {
        var sim = Room(6);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 6;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 64;
        });
        VmFScriptParser.SfMoveCeiling(ctx);
        Assert.Contains(sim.Motions, m => m.SectorIndex == 0 && m.MovesCeiling && m.TargetCeiling == 64);
    }

    [Fact]
    public void SfLightLevel_ReadAndWrite()
    {
        var sim = Room(7, 120);
        var read = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 7;
        });
        VmFScriptParser.SfLightLevel(read);
        Assert.Equal(120, read.TReturn.IntValue);
        var write = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 7;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 200;
        });
        VmFScriptParser.SfLightLevel(write);
        Assert.Equal(200, sim.LightOf(0));
    }

    [Fact]
    public void SfFloorTexture_SetsAndReturnsName()
    {
        var sim = Room(8);
        sim.Level.Sectors[0].FloorPic = "FLOOR0_1";
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 8;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "FLAT1";
        });
        VmFScriptParser.SfFloorTexture(ctx);
        Assert.Equal("FLAT1", sim.Level.Sectors[0].FloorPic);
        Assert.Equal("FLAT1", ctx.TReturn.String);
    }
}
