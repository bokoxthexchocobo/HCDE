using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ElectricForcedPainRandomTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ElectricHowlUsesExistingLightningDrawAndConfiguredBodySound(bool configured)
    {
        var howls = 0;
        for (var seed = 0; seed < 64; seed++)
        {
            var sim = Room(seed * 7919); var prediction = Room(seed * 7919);
            var actor = sim.AddBot(100, 0); actor.Brain = null; actor.IsMonster = true;
            if (configured) actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Howl] = "howl" };
            var flinch = prediction.NextFlickerRandom() < 96;
            var howl = !flinch && prediction.NextFlickerRandom() < 128;
            ActorDamage.Apply(actor, 1, damageType: "Electric", inflictor: new Actor { ForcePain = true });
            Assert.Equal(prediction.NextFlickerRandom(), sim.NextFlickerRandom());
            if (!howl || !configured) { Assert.Empty(sim.PendingSoundRequests); continue; }
            howls++;
            var request = Assert.Single(sim.PendingSoundRequests);
            Assert.Equal(actor.Id, request.ActorId); Assert.Equal("howl", request.Sound); Assert.Equal(4, request.Channel);
            Assert.Equal(0, request.Flags);
            Assert.True(request.OnlyIfSoundNotPlaying);
            Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation); Assert.False(request.Loop);
        }
        if (configured) Assert.True(howls > 0);
    }

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
