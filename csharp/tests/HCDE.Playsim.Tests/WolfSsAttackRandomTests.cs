using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WolfSsAttackRandomTests
{
    [Theory]
    [InlineData(20, 0)]
    [InlineData(21, 1)]
    [InlineData(30, 1)]
    [InlineData(31, 2)]
    public void SsShotsUseSharedChaingunnerStreamAndConfiguredSound(int calls, int shots)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 84);
        monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/ss" };
        var combat = sim.CombatRandomState;
        for (var i = 0; i < calls; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room();
        for (var i = 0; i < shots; i++) expected.NextCPosAttackShot();
        Assert.Equal(expected.CaptureState().CPosAttackRandomState, sim.CaptureState().CPosAttackRandomState);
        var sounds = sim.DrainSoundRequests().Where(r => r.Sound == "custom/ss").ToArray();
        Assert.Equal(shots, sounds.Length); Assert.All(sounds, r => Assert.Equal(1, r.Channel));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Null(sim.CaptureState().SPosAttackRandomState);
        Assert.Null(sim.CaptureState().PosAttackRandomState);
    }

    [Fact]
    public void SsRepeatBoundaryUsesSharedRefireStreamAndSavePreservesContinuation()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 84);
        monster.ReactionTime = 0;
        for (var i = 0; i < 34; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room(); expected.CheckCPosRefire(true);
        Assert.Equal(expected.CaptureState().CPosRefireRandomState, sim.CaptureState().CPosRefireRandomState);
        var bytes = SimSavegame.Write(sim);
        var shot = sim.NextCPosAttackShot(); var repeat = sim.CheckCPosRefire(false);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(shot, sim.NextCPosAttackShot()); Assert.Equal(repeat, sim.CheckCPosRefire(false));
    }

    [Fact]
    public void DisabledMissileAdmissionConsumesNeitherStream()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 84);
        monster.ReactionTime = 0;
        for (var i = 0; i < 40; i++) ActorChaseActions.ExtChase(monster, false, false, playActive: false);
        Assert.Null(sim.CaptureState().CPosAttackRandomState);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 84 }] }, rngSeed: 0);
        sim.Players.Single().Invulnerable = true;
        return sim;
    }
}
