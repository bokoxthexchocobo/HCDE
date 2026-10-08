using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterHitscanDamageContextTests
{
    [Theory]
    [InlineData(3004, 10)]
    [InlineData(9, 10)]
    [InlineData(65, 10)]
    [InlineData(84, 20)]
    [InlineData(7, 20)]
    public void MonsterBulletsUseHitscanFactorsAfterSave(int type, int delay)
    {
        foreach (var factor in new[] { 0d, 2d })
        {
            var (baseline, first) = Room(type); var (sim, attacker) = Room(type);
            var target = sim.Players.Single(); target.SetDamageFactor("Hitscan", factor);
            target.SetDamageFactor("Melee", 0); attacker.DamageType = "Melee";
            foreach (var pair in new[] { (baseline, first), (sim, attacker) })
            {
                for (var tic = 0; tic < 20 && pair.Item2.Brain!.Mode != MonsterMode.Windup; tic++) pair.Item1.Tick();
                Assert.Equal(MonsterMode.Windup, pair.Item2.Brain!.Mode);
            }
            SimSavegame.Apply(sim, SimSavegame.Write(sim));
            for (var tic = 0; tic < delay - 1; tic++) { baseline.Tick(); sim.Tick(); }
            Assert.Equal(1000, target.Health);
            baseline.Tick(); sim.Tick();
            var ordinaryDamage = 1000 - baseline.Players.Single().Health;
            Assert.True(ordinaryDamage > 0);
            Assert.Equal(1000 - (int)(ordinaryDamage * factor), target.Health);
            Assert.Equal(baseline.CombatRandomState, sim.CombatRandomState);
            Assert.Empty(sim.Actors.OfType<ProjectileActor>());
        }
    }

    private static (AuthoritySimulation Sim, Actor Attacker) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 250 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        var target = sim.Players.Single(); target.Health = 1000;
        target.Radius = Fixed.FromInt(100); target.NoPain = true;
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == type));
    }
}
