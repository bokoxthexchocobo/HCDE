using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DrainHealthSoundTests
{
    [Theory]
    [InlineData(50, 10, 60, true)]
    [InlineData(100, 10, 100, false)]
    [InlineData(0, 10, 0, false)]
    [InlineData(50, 0, 50, false)]
    [InlineData(50, -100, 100, true)]
    public void OnlySuccessfulCallbackHealthGrantEmits(int initial, int grant, int expected, bool plays)
    {
        var sim = Room(); var source = new DrainPlayer
        { Simulation = sim, Health = initial, DrainStrength = 0.5, Grant = grant };
        sim.DrainSoundRequests();
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 20, source);
        Assert.Equal(10, source.Input); Assert.Equal(expected, source.Health);
        var sounds = sim.DrainSoundRequests();
        if (!plays) { Assert.Empty(sounds); return; }
        var sound = Assert.Single(sounds);
        Assert.Equal(source.Id, sound.ActorId); Assert.Equal("*drainhealth", sound.Sound);
        Assert.Equal(3, sound.Channel); Assert.Equal(1, sound.Volume);
        Assert.Equal(1, sound.Attenuation); Assert.Equal(0, sound.Flags);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SelfAndDontDrainExclusionsRemainSilent(bool self)
    {
        var sim = Room(); var source = sim.Players.Single(); source.Health = 50; source.DrainStrength = 1; source.NoPain = true;
        var target = self ? source : new Actor { Health = 100, NoPain = true, DontDrain = true };
        ActorDamage.Apply(target, 20, source);
        Assert.Equal(self ? 30 : 50, source.Health); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void GrantSoundOccursBeforeTargetDeathStateAction()
    {
        var sim = Room(); var source = sim.Players.Single(); source.Health = 50; source.DrainStrength = 1;
        var target = new Actor { Health = 5, NoPain = true }; string? observed = null;
        target.States.Configure(target, [new(-1, 0), new(-1, 1), new(-1, 2, _ =>
        {
            Assert.Equal(60, source.Health);
            observed = Assert.Single(sim.DrainSoundRequests()).Sound;
        })], 0);
        ActorDamage.Apply(target, 10, source);
        Assert.Equal("*drainhealth", observed); Assert.True(target.IsDead);
    }

    private sealed class DrainPlayer : PlayerPawn
    {
        public int Grant { get; init; }
        public int Input { get; private set; }
        public override int OnDrain(Actor target, int amount, string? damageType)
        { Input = amount; return Grant; }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
