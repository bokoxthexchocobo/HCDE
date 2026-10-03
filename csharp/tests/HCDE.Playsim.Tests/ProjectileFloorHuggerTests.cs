using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFloorHuggerTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void NoDropOffExceptionAndNoExplodeFloorPrecedence(bool noExplode, bool noDropOff, bool destroyed)
    {
        var sim = Room(); var missile = Shoot(sim); missile.FloorHugger = true;
        missile.NoExplodeFloor = noExplode; missile.NoDropOff = noDropOff;
        missile.VelocityX = Fixed.FromInt(10); missile.VelocityZ = Fixed.FromInt(-2);
        sim.Tick(); Assert.Equal(destroyed, missile.Destroyed); Assert.Equal(0, missile.Z.ToDouble());
        Assert.Equal(destroyed, sim.Level.Sectors[0].HealthFloor < 1000);
        if (!destroyed) Assert.Equal(noExplode ? 0 : -2, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void FloorHuggingPrecedesCeilingHuggingDuringTravel()
    {
        var sim = Room(); var missile = Shoot(sim); missile.FloorHugger = missile.CeilingHugger = true;
        missile.VelocityX = Fixed.FromInt(10); sim.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(0, missile.Z.ToDouble()); Assert.Equal(110, missile.X.ToDouble());
    }

    [Fact]
    public void FloorSurvivorStillHitsActorDuringTravel()
    {
        var sim = Room(); var target = sim.AddBot(150, 0); target.Brain = null;
        var missile = Shoot(sim); missile.FloorHugger = true; missile.VelocityX = Fixed.FromInt(40);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(target.Health < 20);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Theory]
    [InlineData("floorhugger")]
    [InlineData("nodropoff")]
    public void AcsFlagsCanBeSetQueriedAndCleared(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToUpperInvariant(), out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false));
        Assert.True(AcsActorFlags.TryGet(actor, flag, out value)); Assert.False(value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagsParticipateInChecksum(bool noDropOff)
    {
        var first = Room(); var second = Room(); var actor = Shoot(first); Shoot(second);
        if (noDropOff) actor.NoDropOff = true; else actor.FloorHugger = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 1000 }], Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(64);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
