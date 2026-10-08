using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ElectricForcedPainRandomTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 10)]
    [InlineData(true, 0)]
    [InlineData(true, 10)]
    public void ForcedPainStillRollsLightningAndMonsterHowlDraw(bool monster, int damage)
    {
        var flinches = 0; var shines = 0;
        for (var seed = 0; seed < 64; seed++)
        {
            var sim = Room(seed * 7919); var prediction = Room(seed * 7919);
            var victim = sim.AddBot(100, 0); victim.Brain = null; victim.IsMonster = monster;
            victim.PainChance = 0; victim.PainThreshold = 1000;
            var initialState = victim.States.Current; var health = victim.Health;
            var flinch = prediction.NextFlickerRandom() < 96;
            if (!flinch && monster) prediction.NextFlickerRandom();
            var combat = sim.CombatRandomState;
            ActorDamage.Apply(victim, damage, damageType: "Electric", inflictor: new Actor { ForcePain = true });
            Assert.Equal(health - damage, victim.Health);
            Assert.Equal(flinch ? victim.PainStateFor("Electric") : initialState, victim.States.Current);
            Assert.Equal(!flinch, victim.FullBright);
            Assert.Equal(combat, sim.CombatRandomState);
            Assert.Equal(prediction.NextFlickerRandom(), sim.NextFlickerRandom());
            if (flinch) flinches++; else shines++;
        }
        Assert.True(flinches > 0); Assert.True(shines > 0);
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: seed);
}
