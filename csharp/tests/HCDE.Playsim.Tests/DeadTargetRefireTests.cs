using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeadTargetRefireTests
{
    [Theory]
    [InlineData(65, 18, false)]
    [InlineData(65, 18, true)]
    [InlineData(7, 28, false)]
    [InlineData(68, 28, false)]
    [InlineData(7, 28, true)]
    [InlineData(68, 28, true)]
    [InlineData(84, 34, false)]
    [InlineData(84, 34, true)]
    public void DeadTargetReachesRefireAndRollDecidesContinuation(int type, int calls, bool continueFiring)
    {
        var threshold = type is 7 or 68 ? (byte)10 : (byte)40;
        var seed = Enumerable.Range(0, 1000).First(s =>
        {
            var random = NativeStateRandom.Seed((uint)s, 0x59106a10u);
            return ((NativeStateRandom.Next(ref random) & 255) < threshold) == continueFiring;
        });
        var sim = Room(type, seed); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        monster.ReactionTime = 0;
        for (var i = 0; i < calls - 5; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var target = sim.Players.Single(); target.Health = 0;
        for (var i = 0; i < 5; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(target.Id, monster.Brain!.TargetId);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
        var bytes = SimSavegame.Write(sim);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(continueFiring ? MonsterMode.Recovery : MonsterMode.Chase, monster.Brain!.Mode);
        Assert.Equal(target.Id, monster.Brain.TargetId);
        var randomState = sim.CaptureState().CPosRefireRandomState;
        Assert.NotNull(randomState);
        var expected = Room(type, seed); Assert.Equal(continueFiring, expected.CheckCPosRefire(false, threshold));
        Assert.Equal(expected.CaptureState().CPosRefireRandomState, randomState);
        SimSavegame.Apply(sim, bytes);
        monster = sim.Actors.Single(a => a.DoomEdNum == type);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(continueFiring ? MonsterMode.Recovery : MonsterMode.Chase, monster.Brain!.Mode);
        Assert.Equal(randomState, sim.CaptureState().CPosRefireRandomState);
    }

    private static AuthoritySimulation Room(int type, int seed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }] }, rngSeed: seed);
        sim.Players.Single().Invulnerable = true;
        return sim;
    }
}
