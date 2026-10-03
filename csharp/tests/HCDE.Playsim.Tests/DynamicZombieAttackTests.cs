using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicZombieAttackTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ZombieUsesTenTicShotAndTwentySixTicSequence(bool dynamicSpawn, bool remap)
    {
        var (sim, actor) = Range(dynamicSpawn, remap);
        for (var tic = 0; tic < 20 && actor.Brain!.Mode != MonsterMode.Windup; tic++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, actor.Brain!.Mode); Assert.Equal(10, actor.Brain.WindupTics);
        var random = sim.CombatRandomState;
        for (var tic = 1; tic < 10; tic++) sim.Tick();
        Assert.Equal(random, sim.CombatRandomState);
        sim.Tick(); Assert.NotEqual(random, sim.CombatRandomState);
        Assert.Equal(MonsterMode.Recovery, actor.Brain.Mode);
        random = sim.CombatRandomState;
        for (var tic = 11; tic < 26; tic++) sim.Tick();
        Assert.Equal(random, sim.CombatRandomState); // Only one shot per attack.
        sim.Tick(); Assert.Equal(MonsterMode.Chase, actor.Brain.Mode);
    }
    [Fact]
    public void MapAndDynamicZombieAttackLoopsMatchAcrossMultipleAttacks()
    {
        var (map, mapped) = Range(false, false); var (dynamicSim, spawned) = Range(true, false);
        for (var tic = 0; tic < 100; tic++)
        {
            map.Tick(); dynamicSim.Tick();
            Assert.Equal(mapped.Brain!.Mode, spawned.Brain!.Mode);
            Assert.Equal(mapped.Brain.WindupTics, spawned.Brain.WindupTics);
            Assert.Equal(map.CombatRandomState, dynamicSim.CombatRandomState);
            Assert.Equal(map.Players.Single().Health, dynamicSim.Players.Single().Health);
        }
    }
    private static (AuthoritySimulation, Actor) Range(bool dynamicSpawn, bool remap)
    {
        var type = remap ? 4000 : 3004;
        var patch = remap ? DehackedPatch.Apply("Thing 2\nID # = 4000\n") : null;
        var things = new List<LevelThing> { new() { Type = 1, X = 800 } };
        if (!dynamicSpawn) things.Add(new LevelThing { Type = type });
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = things, Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }] }, rngSeed: 42, dehacked: patch);
        sim.Players.Single().Invulnerable = true;
        return (sim, dynamicSpawn ? sim.AddBot(0, 0, type) : sim.Actors.Single(a => a.DoomEdNum == type));
    }
}