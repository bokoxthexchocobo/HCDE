using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProfileAttackSaveTests
{
    [Theory]
    [InlineData(3002)]
    [InlineData(58)]
    [InlineData(3001)]
    [InlineData(3003)]
    [InlineData(69)]
    [InlineData(3005)]
    [InlineData(3004)]
    [InlineData(9)]
    [InlineData(16)]
    [InlineData(68)]
    [InlineData(67)]
    [InlineData(71)]
    public void SaveResumesAttackPositionDamageAndRandomState(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
        var monster = sim.Actors.Single(a => a.DoomEdNum == type); monster.ReactionTime = 0;
        for (var i = 0; i < 5; i++) ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        Assert.NotNull(monster.Brain!.CaptureProfileAttack());
        var bytes = SimSavegame.Write(sim); sim.DrainSoundRequests();
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        var expected = sim.CaptureState(); var health = sim.Players.Single().Health;
        var attack = monster.Brain.CaptureProfileAttack(); var sounds = sim.DrainSoundRequests();
        SimSavegame.Apply(sim, bytes); sim.DrainSoundRequests();
        monster = sim.Actors.Single(a => a.DoomEdNum == type);
        for (var i = 0; i < 12; i++) ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        var actual = sim.CaptureState();
        Assert.Equal(attack, monster.Brain!.CaptureProfileAttack()); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(expected.CombatRandomState, actual.CombatRandomState);
        Assert.Equal(expected.SargAttackRandomState, actual.SargAttackRandomState);
        Assert.Equal(expected.TroopAttackRandomState, actual.TroopAttackRandomState);
        Assert.Equal(expected.BruisAttackRandomState, actual.BruisAttackRandomState);
        Assert.Equal(expected.HeadAttackRandomState, actual.HeadAttackRandomState);
        Assert.Equal(expected.PosAttackRandomState, actual.PosAttackRandomState);
        Assert.Equal(expected.SPosAttackRandomState, actual.SPosAttackRandomState);
        Assert.Equal(sounds, sim.DrainSoundRequests());
    }
}
