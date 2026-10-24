using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfCamFloorTests
{
    static AuthoritySimulation Room(int sectorTag = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
        Sectors = [new LevelSector { Index = 0, Tag = sectorTag, FloorHeight = 0, CeilingHeight = 128 }],
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
            ActorValue = v => v.Mobj,
            Simulation = sim,
            FraggleThinker = thinker,
            Script = new VmFScriptScript.FsScript(),
        };
    }

    [Fact]
    public void SfSetCamera_SetsCameraThingId()
    {
        var sim = Room();
        var actor = sim.Actors.Single();
        actor.Angle = BamAngle.FromDegrees(45);
        actor.Z = Fixed.FromDouble(8);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = actor;
        });
        VmFScriptParser.SfSetCamera(ctx);
        Assert.Equal((int)actor.Id, sim.CameraThingId);
        Assert.Equal(45, actor.FraggleScriptSpecialF1, 3);
        Assert.Equal(8, actor.FraggleScriptSpecialF2, 3);
    }

    [Fact]
    public void SfClearCamera_RestoresSavedAngleAndZ()
    {
        var sim = Room();
        var cam = sim.Actors.Single();
        cam.FraggleScriptSpecialF1 = 90;
        cam.FraggleScriptSpecialF2 = 12;
        cam.Angle = BamAngle.FromDegrees(0);
        cam.Z = Fixed.FromDouble(0);
        sim.CameraThingId = (int)cam.Id;
        var ctx = Ctx(sim, 0, _ => { });
        VmFScriptParser.SfClearCamera(ctx);
        Assert.Equal((int)cam.Id, sim.CameraThingId);
        Assert.Equal(90, cam.Angle.ToDegrees(), 3);
        Assert.Equal(12, cam.Z.ToDouble(), 3);
    }

    [Fact]
    public void SfFloorHeight_ReadsSectorFloor()
    {
        var sim = Room(3);
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 3;
        });
        VmFScriptParser.SfFloorHeight(ctx);
        Assert.Equal(0, ctx.TReturn.FixedValue / 65536.0, 3);
    }

    [Fact]
    public void SfFloorHeight_WriteUpdatesPlaysimFloor()
    {
        var sim = Room(2);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 2;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 64;
        });
        VmFScriptParser.SfFloorHeight(ctx);
        Assert.Equal(64, sim.FloorOf(0), 3);
    }

    [Fact]
    public void SfMoveFloor_StartsSectorMotion()
    {
        var sim = Room(5);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 5;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 32;
        });
        VmFScriptParser.SfMoveFloor(ctx);
        Assert.Contains(sim.Motions, m => m.SectorIndex == 0 && m.TargetFloor == 32);
    }

    [Fact]
    public void SfStartSound_OnActor_DoesNotThrow()
    {
        var sim = Room();
        var actor = sim.Actors.Single();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtMobj;
            argv[0].Mobj = actor;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "weapons/pistol";
        });
        VmFScriptParser.SfStartSound(ctx);
        Assert.Contains(sim.DrainSoundRequests(), r => r.Sound == "weapons/pistol");
    }
}
