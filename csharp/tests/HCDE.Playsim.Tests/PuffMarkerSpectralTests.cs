using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffMarkerSpectralTests
{
    [Fact]
    public void HostileSpectralTargetCanBeHurtByNonPuffSpectralInflictor()
    {
        var target = new Actor { Health = 100, Spectral = true };
        var inflictor = new Actor { Spectral = true, Friendly = true };
        Assert.True(target.IsHostile(inflictor));
        Assert.Equal(20, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }

    [Fact]
    public void SpawnPathMarksPuffButClassConstructionDoesNot()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        Assert.True(puff.IsPuff); Assert.False(new PuffActor().IsPuff); Assert.False(sim.Players.Single().IsPuff);
    }

    [Fact]
    public void MarkerSurvivesIdentityReconstructionAndReplay()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24); var before = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        Assert.True(puff.Destroyed); SimSavegame.Apply(sim, before);
        Assert.True(sim.Actors.OfType<PuffActor>().Single().IsPuff); Assert.Equal(before, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 20)]
    public void SpectralPuffBypassesSpectralAllyFilter(bool puff, int expected)
    {
        var sim = Room(); var inflictor = puff ? sim.SpawnHitscanPuff(100, 0, 24) : new Actor();
        inflictor.Spectral = true;
        var target = new Actor { Health = 100, Spectral = true };
        Assert.Equal(expected, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 20)]
    public void NonPuffSpectralInflictorUsesFriendPlayerForSpectralTarget(int friend, int expected)
    {
        var target = new Actor { Health = 100, Spectral = true };
        Assert.Equal(expected, ActorDamage.Apply(target, 20,
            inflictor: new Actor { Spectral = true, FriendPlayer = friend }).HealthLost);
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(false, true, 20)]
    [InlineData(true, false, 20)]
    [InlineData(true, true, 20)]
    public void SpectralPlayerProtectionRespectsDeathmatchAndPuff(bool deathmatch, bool puff, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        }, spawnOptions: new SpawnOptions(Mode: deathmatch ? SpawnGameMode.Deathmatch : SpawnGameMode.Single));
        var target = sim.Players.Single();
        var inflictor = puff ? sim.SpawnHitscanPuff(100, 0, 24) : new Actor();
        inflictor.Spectral = true; inflictor.FriendPlayer = 1;
        Assert.Equal(expected, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }

    [Theory]
    [InlineData(DamageFlags.Forced)]
    [InlineData(DamageFlags.None)]
    public void ForcedAndTelefragDamageBypassSpectralAllyFilter(DamageFlags flags)
    {
        var target = new Actor { Health = 100, Spectral = true };
        var damage = flags == DamageFlags.Forced ? 20 : ActorDamage.TelefragDamage;
        Assert.True(ActorDamage.Apply(target, damage, flags: flags,
            inflictor: new Actor { Spectral = true }).HealthLost > 0);
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
