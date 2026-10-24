using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfMathTests
{
    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation? sim = null, int argc = 0) =>
        new()
        {
            TArgc = argc,
            TArgv = [new(), new()],
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            ActorValue = v => v.Mobj,
            PlayerNum = o => o is PlayerPawn p ? p.PlayerNum : -1,
            Simulation = sim,
        };

    [Fact]
    public void SfMax_PicksLargerFixed()
    {
        var ctx = Ctx(argc: 2);
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 2;
        ctx.TArgv[1].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[1].IntValue = 5;
        VmFScriptParser.SfMax(ctx);
        Assert.Equal(VmFScriptParser.SvtFixed, ctx.TReturn.Type);
        Assert.Equal(5 * 65536.0, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfMin_PicksSmallerFixed()
    {
        var ctx = Ctx(argc: 2);
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = 2;
        ctx.TArgv[1].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[1].IntValue = 5;
        VmFScriptParser.SfMin(ctx);
        Assert.Equal(2 * 65536.0, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfAbs_NegatesNegative()
    {
        var ctx = Ctx(argc: 1);
        ctx.TArgv[0].Type = VmFScriptParser.SvtInt;
        ctx.TArgv[0].IntValue = -3;
        VmFScriptParser.SfAbs(ctx);
        Assert.Equal(3 * 65536.0, ctx.TReturn.FixedValue);
    }

    [Fact]
    public void SfGameskill_ReturnsSkillPlusOne()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel(), spawnOptions: new SpawnOptions(Skill: 3));
        var ctx = Ctx(sim);
        VmFScriptParser.SfGameskill(ctx);
        Assert.Equal(4, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfGamemode_DeathmatchIsTwo()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel(), spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch));
        var ctx = Ctx(sim);
        VmFScriptParser.SfGamemode(ctx);
        Assert.Equal(2, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfIsPlayerObj_TriggerPlayer()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single();
        var ctx = Ctx(sim);
        ctx.Script = new VmFScriptScript.FsScript { Trigger = player };
        VmFScriptParser.SfIsPlayerObj(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
