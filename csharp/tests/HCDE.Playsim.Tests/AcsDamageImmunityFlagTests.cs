using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDamageImmunityFlagTests
{
    [Theory]
    [InlineData("dontdrain")]
    [InlineData("noradiusdmg")]
    public void FlagsSupportNativeCaseInsensitiveNames(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name.ToUpperInvariant(), out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, name.ToUpperInvariant(), false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled));
        Assert.False(enabled);
    }

    [Fact]
    public void ScriptDontDrainBlocksHealingWithoutBlockingDamage()
    {
        var player = new PlayerPawn { Health = 50, DrainStrength = 1 };
        var target = new Actor { Health = 100 };
        Assert.True(AcsActorFlags.TrySet(target, "DONTDRAIN", true));
        Assert.Equal(20, ActorDamage.Apply(target, 20, player).HealthLost);
        Assert.Equal(50, player.Health);
        Assert.True(AcsActorFlags.TrySet(target, "DONTDRAIN", false));
        ActorDamage.Apply(target, 20, player);
        Assert.Equal(70, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptNoRadiusDamageControlsSplashButNotDirectDamage(bool immune)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004);
        var direct = sim.AddBot(30, 0, 3001);
        var nearby = sim.AddBot(30, 60, 3001);
        owner.Brain = direct.Brain = nearby.Brain = null;
        direct.Health = nearby.Health = 1000;
        direct.PainChance = nearby.PainChance = 0;
        Assert.True(AcsActorFlags.TrySet(direct, "NORADIUSDMG", true));
        Assert.True(AcsActorFlags.TrySet(nearby, "NORADIUSDMG", immune));
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick();
        Assert.True(missile.Destroyed);
        Assert.True(direct.Health < 1000);
        Assert.Equal(immune, nearby.Health == 1000);
    }
}
