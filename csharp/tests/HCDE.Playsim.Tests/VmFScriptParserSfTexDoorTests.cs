using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfTexDoorTests
{
    /// <summary>Tagged door sector plus lower neighbor — required for <see cref="LineSpecials"/> door target height.</summary>
    static AuthoritySimulation DoorRoom(int tag = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors =
        [
            new LevelSector { Index = 0, Tag = tag, FloorHeight = 0, CeilingHeight = 100, CeilingPic = "CEIL1" },
            new LevelSector { Index = 1, CeilingHeight = 108 },
        ],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
        Things = [new LevelThing { Type = 3004 }],
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
        var sim = DoorRoom();
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
        var sim = DoorRoom();
        var ctx = Ctx(sim, 1, argv => argv[0].IntValue = 1);
        VmFScriptParser.SfOpenDoor(ctx);
        var motion = Assert.Single(sim.Motions);
        Assert.Equal(0, motion.SectorIndex);
        Assert.True(motion.MovesCeiling);
        Assert.Equal(100, sim.CeilingOf(0));
        sim.Tick();
        Assert.True(sim.CeilingOf(0) > 100);
    }

    [Fact]
    public void SfOpenDoor_TagZeroIsNoOp()
    {
        var sim = DoorRoom();
        var ctx = Ctx(sim, 1, argv => argv[0].IntValue = 0);
        VmFScriptParser.SfOpenDoor(ctx);
        Assert.Empty(sim.Motions);
    }

    [Fact]
    public void SfCloseDoor_StartsClosingMotion()
    {
        var sim = DoorRoom();
        sim.Ceilings[0] = 104;
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].IntValue = 1;
            argv[1].IntValue = 2;
        });
        VmFScriptParser.SfCloseDoor(ctx);
        var motion = Assert.Single(sim.Motions);
        Assert.Equal(0, motion.SectorIndex);
        Assert.True(motion.Closing);
        Assert.Equal(MotionKind.Door, motion.Kind);
    }
}
