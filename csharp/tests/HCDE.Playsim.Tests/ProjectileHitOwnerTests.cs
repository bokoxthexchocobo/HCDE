using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileHitOwnerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissileFlagControlsDirectOwnerContact(bool enabled)
    {
        var (sim, missile, owner) = Setup(); missile.HitOwner = enabled; var health = owner.Health;
        sim.Tick(); Assert.Equal(enabled, missile.Destroyed);
        if (enabled) Assert.True(owner.Health < health); else Assert.Equal(health, owner.Health);
    }

    [Fact]
    public void OwnersFlagDoesNotGrantMissilePermission()
    {
        var (sim, missile, owner) = Setup(); owner.HitOwner = true;
        sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void InvulnerableOwnerStillStopsPermittedMissile()
    {
        var (sim, missile, owner) = Setup(); missile.HitOwner = true; owner.Invulnerable = true;
        var health = owner.Health; sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, owner.Health);
    }

    [Theory]
    [InlineData("thruactors")]
    [InlineData("nonshootable")]
    [InlineData("ghost")]
    [InlineData("thrubits")]
    [InlineData("mthruspecies")]
    public void ExistingPassageRulesCanStillSkipOwner(string passage)
    {
        var (sim, missile, owner) = Setup(); missile.HitOwner = true;
        switch (passage)
        {
            case "thruactors": owner.ThruActors = true; break;
            case "nonshootable": owner.NonShootable = true; break;
            case "ghost": owner.Ghost = true; missile.ThruGhost = true; break;
            case "thrubits": owner.ThruBits = missile.ThruBits = 1; owner.AllowThruBits = true; break;
            case "mthruspecies": missile.MThruSpecies = true; break;
        }
        sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void AcsFlagRoundTrips()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "hitowner", true));
        Assert.True(AcsActorFlags.TryGet(actor, "HITOWNER", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "HitOwner", false)); Assert.False(actor.HitOwner);
    }

    private static (AuthoritySimulation, ProjectileActor, PlayerPawn) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var owner = sim.Players.Single(); owner.X = Fixed.FromInt(30);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, owner);
    }
}
