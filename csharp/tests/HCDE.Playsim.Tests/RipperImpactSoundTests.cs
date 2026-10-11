using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RipperImpactSoundTests
{
    [Theory]
    [InlineData(true, 7, false, true)]
    [InlineData(false, 7, false, false)]
    [InlineData(true, 0, false, false)]
    [InlineData(true, -7, false, false)]
    [InlineData(true, 0, true, true)]
    public void DamageBranchControlsRipSound(bool ripper, int amount, bool forcePain, bool plays)
    {
        var sim = Room(); var target = sim.Players.Single(); target.NoPain = true; target.Health = 50;
        var missile = sim.SpawnProjectile(target, ProjectileKind.Plasma);
        sim.DrainSoundRequests();
        missile.DamageExpression = _ => amount; missile.ForcePain = forcePain;
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        missile.DoMissileDamage(target, ripper);
        var sounds = sim.DrainSoundRequests();
        if (plays)
        {
            var sound = Assert.Single(sounds);
            Assert.Equal(missile.Id, sound.ActorId); Assert.Equal("custom/rip", sound.Sound);
            Assert.Equal(4, sound.Channel); Assert.Equal(1.001f, sound.Attenuation);
            Assert.Equal(1, sound.Volume); Assert.Equal(0, sound.Flags);
        }
        else Assert.Empty(sounds);
        Assert.Equal(50 - amount, target.Health);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void SoundPrecedesDamageCallbackAndAllSoundSlotsRoundTrip()
    {
        var sim = Room(); var owner = sim.Players.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.ActorSounds = Enum.GetValues<ActorSoundType>().ToDictionary(type => type, type => "custom/" + type);
        var bytes = SimSavegame.Write(sim);
        missile.ActorSounds = new Dictionary<ActorSoundType, string>();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(Enum.GetValues<ActorSoundType>().Length, missile.ActorSounds.Count);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        var target = new SoundObserver { Simulation = sim, Health = 100, NoPain = true };
        missile.DamageExpression = _ => 7;
        missile.DoMissileDamage(target, true);
        Assert.Equal("custom/Rip", Assert.Single(target.Sounds).Sound);
        Assert.Equal(93, target.Health);
    }

    private sealed class SoundObserver : Actor
    {
        public IReadOnlyList<SimulationSoundRequest> Sounds { get; private set; } = [];
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        {
            Sounds = Simulation!.DrainSoundRequests().ToArray();
            return damage;
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
