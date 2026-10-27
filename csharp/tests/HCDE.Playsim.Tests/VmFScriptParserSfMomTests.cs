using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfMomTests
{
    static VmFScriptParser.ParserContext ActorCtx(Actor actor, int argc = 1, int extraInt = 0,
        AuthoritySimulation? simulation = null)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 2)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        argv[0].Type = VmFScriptParser.SvtMobj;
        argv[0].Mobj = actor;
        if (argc > 1)
        {
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = extraInt;
        }
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            ActorValue = v => v.Mobj,
            IntValue = v => v.IntValue,
            FloatValue = v => v.IntValue,
            Simulation = simulation,
        };
    }

    [Fact]
    public void SfReactionTime_ReadWrite()
    {
        var actor = new Actor { Id = 1 };
        var ctx = ActorCtx(actor, 2, 100);
        VmFScriptParser.SfReactionTime(ctx);
        Assert.Equal(GameTicClock.TicRate, actor.ReactionTime);
        ctx.TArgc = 1;
        VmFScriptParser.SfReactionTime(ctx);
        Assert.Equal(GameTicClock.TicRate, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfMobjTarget_SetsTarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "TEST" });
        var mo = sim.AddBot(0, 0);
        mo.SeeState = ActorStateMachine.Pain;
        var target = sim.AddBot(64, 64);
        var ctx = ActorCtx(mo, 2, simulation: sim);
        ctx.TArgv[1].Type = VmFScriptParser.SvtMobj;
        ctx.TArgv[1].Mobj = target;
        VmFScriptParser.SfMobjTarget(ctx);
        Assert.True(mo.JustHit);
        Assert.Same(target, ctx.TReturn.Mobj);
    }

    [Fact]
    public void SfMobjMomx_ReadWrite()
    {
        var actor = new Actor { Id = 1 };
        var ctx = ActorCtx(actor, 2);
        ctx.TArgv[1].IntValue = 3;
        VmFScriptParser.SfMobjMomx(ctx);
        Assert.Equal(3.0, actor.VelocityX.ToDouble());
        Assert.Equal(3.0, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfMobjMomy_ReadWrite()
    {
        var actor = new Actor { Id = 1 };
        var ctx = ActorCtx(actor, 2);
        ctx.TArgv[1].IntValue = -2;
        VmFScriptParser.SfMobjMomy(ctx);
        Assert.Equal(-2.0, actor.VelocityY.ToDouble());
    }

    [Fact]
    public void SfMobjMomz_ReadWrite()
    {
        var actor = new Actor { Id = 1 };
        var ctx = ActorCtx(actor, 2);
        ctx.TArgv[1].IntValue = 7;
        VmFScriptParser.SfMobjMomz(ctx);
        Assert.Equal(7.0, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void SfPointToDist_AxisDistance()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 4,
            TArgv =
            [
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
            ],
            FloatValue = v => v.IntValue,
        };
        ctx.TArgv[0].IntValue = 0;
        ctx.TArgv[1].IntValue = 0;
        ctx.TArgv[2].IntValue = 3;
        ctx.TArgv[3].IntValue = 4;
        VmFScriptParser.SfPointToDist(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(5.0, ctx.TReturn.FixedValue, 3);
    }

    [Fact]
    public void SfPointToAngle_East()
    {
        var ctx = new VmFScriptParser.ParserContext
        {
            TArgc = 4,
            TArgv =
            [
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
                new VmFScriptParser.ScriptValue(),
            ],
            FloatValue = v => v.IntValue,
        };
        ctx.TArgv[0].IntValue = 0;
        ctx.TArgv[1].IntValue = 0;
        ctx.TArgv[2].IntValue = 1;
        ctx.TArgv[3].IntValue = 0;
        VmFScriptParser.SfPointToAngle(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(0.0, ctx.TReturn.FixedValue, 3);
    }
}
