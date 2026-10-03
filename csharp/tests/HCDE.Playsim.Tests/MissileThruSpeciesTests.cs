using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileThruSpeciesTests
{
    [Theory]
    [InlineData(3004, 3004, true)]
    [InlineData(3002, 58, true)]
    [InlineData(58, 3002, true)]
    [InlineData(3003, 69, true)]
    [InlineData(69, 3003, true)]
    [InlineData(3004, 3001, false)]
    public void MissilePassageUsesOwnersDoomSpecies(int ownerType, int targetType, bool passes)
    {
        var (sim, missile, target) = Setup(ownerType, targetType); missile.MThruSpecies = true;
        var health = target.Health; sim.Tick();
        Assert.Equal(!passes, missile.Destroyed);
        if (passes) { Assert.Equal(30, missile.X.ToDouble()); Assert.Equal(health, target.Health); }
    }

    [Fact]
    public void TargetFlagDoesNotGrantPassage()
    {
        var (sim, missile, target) = Setup(3004, 3004); target.MThruSpecies = true;
        sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Fact]
    public void DisabledFlagRetainsSpeciesImmuneImpact()
    {
        var (sim, missile, target) = Setup(3004, 3004); var health = target.Health;
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, target.Health);
    }

    [Fact]
    public void ShooterSpeciesDeterminesPassageRatherThanMissileClass()
    {
        var (sim, missile, target) = Setup(3004, 3004);
        Assert.False(missile.SharesContactSpecies(target)); missile.MThruSpecies = true;
        sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void SupportedPlayerStartsShareContactSpeciesButNotDamageImmunity()
    {
        var first = new PlayerPawn { DoomEdNum = 1 }; var second = new PlayerPawn { DoomEdNum = 2 };
        Assert.True(first.SharesContactSpecies(second)); Assert.False(first.IsSameSpecies(second));
        Assert.False(first.SharesContactSpecies(new Actor { DoomEdNum = 1 }));
    }

    [Fact]
    public void SpeciesPassageDoesNotBypassFloorImpact()
    {
        var (sim, missile, _) = Setup(3004, 3004); missile.MThruSpecies = true;
        missile.Z = Fixed.FromInt(1); missile.VelocityX = default; missile.VelocityZ = Fixed.FromInt(-2);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(0, missile.Z.Raw);
    }

    [Fact]
    public void AcsFlagCanBeSetQueriedAndCleared()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "mthruspecies", true));
        Assert.True(AcsActorFlags.TryGet(actor, "MTHRUSPECIES", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "MThruSpecies", false)); Assert.False(actor.MThruSpecies);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup(int ownerType, int targetType)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(-200, 0, ownerType); var target = sim.AddBot(30, 0, targetType);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
