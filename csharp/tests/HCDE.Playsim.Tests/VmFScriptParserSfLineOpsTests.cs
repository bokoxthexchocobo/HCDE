using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfLineOpsTests
{
    static AuthoritySimulation LineRoom() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
        Sectors = [new LevelSector { Index = 0, Tag = 1, FloorHeight = 0, CeilingHeight = 128 }],
        Sides = [new LevelSide { Index = 0, Sector = 0, TopTexture = "A", MidTexture = "B", BottomTexture = "C" }],
        Lines =
        [
            new LevelLine
            {
                Index = 0,
                AdditionalIds = [5],
                SideFront = 0,
                SideBack = -1,
                Flags = 0,
            },
        ],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]> fill)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 5)];
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
            Script = new VmFScriptScript.FsScript { Trigger = sim.Actors.First() },
        };
    }

    [Fact]
    public void SfSetLineBlocking_SetsBlockingFlag()
    {
        var sim = LineRoom();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 1;
        });
        VmFScriptParser.SfSetLineBlocking(ctx);
        Assert.Equal(LevelLine.BlockingFlag, sim.Level.Lines[0].Flags);
    }

    [Fact]
    public void SfSetLineMonsterBlocking_SetsMonsterBlock()
    {
        var sim = LineRoom();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 1;
        });
        VmFScriptParser.SfSetLineMonsterBlocking(ctx);
        Assert.Equal(LevelLine.BlockMonstersFlag, sim.Level.Lines[0].Flags);
    }

    [Fact]
    public void SfSetLineTexture_EternityLayout_UpdatesMid()
    {
        var sim = LineRoom();
        var ctx = Ctx(sim, 4, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0;
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 2;
            argv[3].Type = VmFScriptParser.SvtString;
            argv[3].String = "WALL1";
        });
        VmFScriptParser.SfSetLineTexture(ctx);
        Assert.Equal("WALL1", sim.Level.Sides[0].MidTexture);
    }

    [Fact]
    public void SfSetLineTexture_LegacyLayout_UpdatesTopAndBottom()
    {
        var sim = LineRoom();
        var ctx = Ctx(sim, 4, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "BRICK";
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 0;
            argv[3].Type = VmFScriptParser.SvtInt;
            argv[3].IntValue = 5;
        });
        VmFScriptParser.SfSetLineTexture(ctx);
        Assert.Equal("BRICK", sim.Level.Sides[0].TopTexture);
        Assert.Equal("BRICK", sim.Level.Sides[0].BottomTexture);
        Assert.Equal("B", sim.Level.Sides[0].MidTexture);
    }

    [Fact]
    public void SfLineTrigger_CallsFloorSpecial()
    {
        var sim = LineRoom();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 279;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 1;
        });
        VmFScriptParser.SfLineTrigger(ctx);
        Assert.Contains(sim.Motions, m => m.SectorIndex == 0);
    }
}
