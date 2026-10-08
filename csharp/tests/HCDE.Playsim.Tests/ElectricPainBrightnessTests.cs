using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ElectricPainBrightnessTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SuccessfulElectricReactionClearsBrightnessOnlyThroughAnimation(bool missingState, bool directAction)
    {
        var successfulRolls = 0;
        for (var seed = 0; seed < 64; seed++)
        {
            var sim = AuthoritySimulation.Start(new PlayLevel
            { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: seed * 7919);
            var prediction = AuthoritySimulation.Start(new PlayLevel(), rngSeed: seed * 7919);
            var actor = sim.AddBot(100, 0); actor.Brain = null;
            actor.FullBright = true; actor.PainState = missingState ? -1 : 1;
            var succeeds = prediction.NextFlickerRandom() < 96;
            if (!succeeds) prediction.NextFlickerRandom();
            var original = actor.States.Current;
            if (directAction)
                Assert.Equal(succeeds && !missingState, actor.TriggerPainChance("Electric", true));
            else
                ActorDamage.Apply(actor, 0, inflictor: new Actor { ForcePain = true }, damageType: "Electric");
            Assert.Equal(missingState || !succeeds, actor.FullBright);
            Assert.Equal(succeeds && !missingState ? 1 : original, actor.States.Current);
            Assert.Equal(prediction.NextFlickerRandom(), sim.NextFlickerRandom());
            if (succeeds) successfulRolls++;
        }
        Assert.True(successfulRolls > 0);
    }
}
