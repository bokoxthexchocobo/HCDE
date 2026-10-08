using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileCeilingHuggerTests
{
    [Theory]
    [InlineData(false, "STONE")]
    [InlineData(true, "STONE")]
    [InlineData(false, "F_SKY1")]
    [InlineData(true, "F_SKY1")]
    public void CeilingContactClampsAndContinuesHorizontalTravel(bool gravity, string texture)
    {
        var sim = Room(texture); var missile = Shoot(sim); missile.CeilingHugger = true; missile.NoGravity = !gravity;
        missile.VelocityX = Fixed.FromInt(10); missile.VelocityZ = Fixed.FromInt(10);
        var random = sim.CombatRandomState; sim.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(120, missile.Z.ToDouble()); Assert.Equal(110, missile.X.ToDouble());
        Assert.Equal(gravity ? -1 : 0, missile.VelocityZ.ToDouble());
        sim.Tick(); Assert.Equal(120, missile.X.ToDouble());
        Assert.InRange(missile.Z.ToDouble(), gravity ? 119 : 120, (gravity ? 119 : 120) + 1.0 / 65536);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthCeiling); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void FloorStillExplodes()
    {
        var sim = Room(); var missile = Shoot(sim); missile.CeilingHugger = true;
        // The native vertical missile nudge first makes horizontal movement snap to the ceiling.
        missile.Z = Fixed.FromInt(2); missile.VelocityZ = Fixed.FromInt(-200);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(sim.Level.Sectors[0].HealthFloor < 1000);
    }

    [Fact]
    public void HorizontalTravelFollowsCeilingFromLowerSpawnHeight()
    {
        var sim = Room(); var missile = Shoot(sim); missile.CeilingHugger = true;
        missile.Z = Fixed.FromInt(64); missile.VelocityX = Fixed.FromInt(10);
        sim.Tick(); Assert.False(missile.Destroyed); Assert.Equal(120, missile.Z.ToDouble());
        Assert.Equal(110, missile.X.ToDouble()); Assert.Equal(1000, sim.Level.Sectors[0].HealthCeiling);
    }

    [Fact]
    public void ActorImpactAfterCeilingContactStillDamages()
    {
        var sim = Room(); var target = sim.AddBot(150, 0); target.Brain = null; target.Z = Fixed.FromInt(100); target.NoGravity = true;
        var missile = Shoot(sim); missile.CeilingHugger = true;
        missile.VelocityX = Fixed.FromInt(40); missile.VelocityZ = Fixed.FromInt(10);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(target.Health < 20);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthCeiling);
    }

    [Fact]
    public void ClearingFlagRestoresCeilingExplosion()
    {
        var sim = Room(); var missile = Shoot(sim); missile.CeilingHugger = true;
        missile.VelocityZ = Fixed.FromInt(10); sim.Tick(); Assert.False(missile.Destroyed);
        missile.CeilingHugger = false; missile.VelocityZ = Fixed.FromInt(1);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(sim.Level.Sectors[0].HealthCeiling < 1000);
    }

    [Fact]
    public void AcsCanSetQueryAndClearFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "ceilinghugger", true));
        Assert.True(AcsActorFlags.TryGet(actor, "CEILINGHUGGER", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "CeilingHugger", false)); Assert.False(actor.CeilingHugger);
    }

    [Fact]
    public void FlagChangesChecksumWithoutCeilingContact()
    {
        var first = Room(); var second = Room(); Shoot(first).CeilingHugger = true; Shoot(second);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(string texture = "STONE") => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, CeilingPic = texture, HealthFloor = 1000, HealthCeiling = 1000 }],
        Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(118);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
