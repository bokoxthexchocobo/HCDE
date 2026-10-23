using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfCameraTests
{
    [Fact]
    public void SfMoveCamera_MovesTowardTarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3004 }],
        });
        var cam = sim.Actors.First();
        var target = sim.Actors.Last();
        cam.X = Fixed.FromInt(0);
        cam.Y = Fixed.FromInt(0);
        cam.Z = Fixed.FromInt(0);
        target.X = Fixed.FromInt(100);
        target.Y = Fixed.FromInt(0);
        target.Z = Fixed.FromInt(32);
        var argv = new VmFScriptParser.ScriptValue[6];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        argv[0].Type = VmFScriptParser.SvtMobj;
        argv[0].Mobj = cam;
        argv[1].Type = VmFScriptParser.SvtMobj;
        argv[1].Mobj = target;
        argv[2].Type = VmFScriptParser.SvtInt;
        argv[2].IntValue = 32;
        argv[3].Type = VmFScriptParser.SvtInt;
        argv[3].IntValue = 50;
        argv[4].Type = VmFScriptParser.SvtInt;
        argv[4].IntValue = 0;
        argv[5].Type = VmFScriptParser.SvtInt;
        argv[5].IntValue = 0;
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 6,
            TArgv = argv,
            IntValue = v => v.IntValue,
            ActorValue = v => v.Mobj,
            FloatValue = v => v.IntValue,
            ScriptError = _ => { },
        };
        VmFScriptParser.SfMoveCamera(ctx);
        Assert.True(cam.X.ToDouble() > 0);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
