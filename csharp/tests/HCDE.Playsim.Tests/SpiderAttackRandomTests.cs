using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpiderAttackRandomTests
{
    [Theory]
    [InlineData(21, 1)]
    [InlineData(25, 2)]
    public void SpiderVolleysUseSharedShotgunStreamAndConfiguredSound(int calls, int volleys)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 7);
        monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/spider" };
        var combat = sim.CombatRandomState;
        for (var i = 0; i < calls; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room();
        for (var i = 0; i < volleys * 3; i++) expected.NextSPosAttackPellet();
        Assert.Equal(expected.CaptureState().SPosAttackRandomState, sim.CaptureState().SPosAttackRandomState);
        var sounds = sim.DrainSoundRequests().Where(r => r.Sound == "custom/spider").ToArray();
        Assert.Equal(volleys, sounds.Length); Assert.All(sounds, r => Assert.Equal(1, r.Channel));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Null(sim.CaptureState().CPosAttackRandomState);
    }

    [Fact]
    public void SpiderRefireUsesThresholdTenAndSharedRefireStream()
    {
        var sim = Room(); var random = NativeStateRandom.Seed(0, 0x59106a10u);
        var continued = 0; var stopped = 0;
        for (var i = 0; i < 256; i++)
        {
            var expected = (NativeStateRandom.Next(ref random) & 255) < 10;
            Assert.Equal(expected, sim.CheckCPosRefire(false, 10));
            if (expected) continued++; else stopped++;
        }
        Assert.True(continued > 0); Assert.True(stopped > 0);
        Assert.Equal(random, sim.CaptureState().CPosRefireRandomState);
    }

    [Fact]
    public void SpiderRepeatBoundaryConsumesSharedRefireAndSurvivesSave()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 7);
        monster.ReactionTime = 0;
        for (var i = 0; i < 28; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room(); expected.CheckCPosRefire(true, 10);
        Assert.Equal(expected.CaptureState().CPosRefireRandomState, sim.CaptureState().CPosRefireRandomState);
        var bytes = SimSavegame.Write(sim);
        var shot = sim.NextSPosAttackPellet(); var repeat = sim.CheckCPosRefire(false, 10);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(shot, sim.NextSPosAttackPellet()); Assert.Equal(repeat, sim.CheckCPosRefire(false, 10));
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 7 }] }, rngSeed: 0);
        sim.Players.Single().Invulnerable = true;
        return sim;
    }
}
