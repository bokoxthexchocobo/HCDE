using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissingTargetRefireTests
{
    [Theory]
    [InlineData(65, 18, false, false)]
    [InlineData(65, 18, false, true)]
    [InlineData(65, 18, true, false)]
    [InlineData(65, 18, true, true)]
    [InlineData(7, 28, false, false)]
    [InlineData(68, 28, false, false)]
    [InlineData(7, 28, false, true)]
    [InlineData(68, 28, false, true)]
    [InlineData(7, 28, true, false)]
    [InlineData(68, 28, true, false)]
    [InlineData(7, 28, true, true)]
    [InlineData(68, 28, true, true)]
    [InlineData(84, 34, false, false)]
    [InlineData(84, 34, false, true)]
    [InlineData(84, 34, true, false)]
    [InlineData(84, 34, true, true)]
    public void MissingTargetSkipsShotAndReachesNativeRefireGate(int type, int calls, bool continueFiring, bool destroyTarget)
    {
        var threshold = type is 7 or 68 ? (byte)10 : (byte)40;
        var seed = Enumerable.Range(0, 1000).First(s =>
        {
            var random = NativeStateRandom.Seed((uint)s, 0x59106a10u);
            return ((NativeStateRandom.Next(ref random) & 255) < threshold) == continueFiring;
        });
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }] }, rngSeed: seed);
        var monster = sim.Actors.Single(a => a.DoomEdNum == type); monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "test/attack" };
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        if (destroyTarget) sim.Players.Single().Destroy();
        else monster.Brain!.SetSpecialTarget(null);
        Assert.Null(monster.Brain!.TargetId);
        for (var i = 1; i < calls; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
        Assert.Null(sim.CaptureState().CPosAttackRandomState);
        Assert.Null(sim.CaptureState().SPosAttackRandomState);
        Assert.DoesNotContain(sim.DrainSoundRequests(), r => r.Sound == "test/attack");
        var bytes = SimSavegame.Write(sim);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(continueFiring ? MonsterMode.Windup : MonsterMode.Chase, monster.Brain.Mode);
        Assert.Null(monster.Brain.TargetId);
        var expected = NativeStateRandom.Seed((uint)seed, 0x59106a10u); NativeStateRandom.Next(ref expected);
        Assert.Equal(expected, sim.CaptureState().CPosRefireRandomState);
        SimSavegame.Apply(sim, bytes);
        monster = sim.Actors.Single(a => a.DoomEdNum == type);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(continueFiring ? MonsterMode.Windup : MonsterMode.Chase, monster.Brain!.Mode);
        Assert.Equal(expected, sim.CaptureState().CPosRefireRandomState);
    }
}
