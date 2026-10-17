using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileBobSpawnTests
{
    [Theory]
    [InlineData(ProjectileKind.ImpBall, 16, 0, 40)]
    [InlineData(ProjectileKind.ImpBall, 48, 0, 24)]
    [InlineData(ProjectileKind.ImpBall, 16, 5, 35)]
    [InlineData(ProjectileKind.RevenantTracer, 16, 5, 51)]
    [InlineData(ProjectileKind.RevenantTracer, 48, 0, 40)]
    public void MonsterSpawnCombinesBobClassHeightAndFloorClip(ProjectileKind kind, int phase, int clip, int height)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0);
        owner.Z = Fixed.FromInt(10); owner.FloatBob = true; owner.FloatBobPhase = phase; owner.FloorClip = clip;
        var missile = sim.SpawnProjectile(owner, kind);
        Assert.Equal(10 + height, missile.Z.ToDouble());
        Assert.Equal(10, owner.Z.ToDouble()); Assert.Same(owner, missile.Owner);
    }

    [Theory]
    [InlineData(ProjectileKind.ImpBall, 0, -4)]
    [InlineData(ProjectileKind.ImpBall, 5, 0)]
    [InlineData(ProjectileKind.RevenantTracer, 0, -4)]
    public void RaisedSpawnUsesNativeShortTargetTrajectory(ProjectileKind kind, int clip, int heightCorrection)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        owner.FloatBob = true; owner.FloatBobPhase = 16; owner.FloorClip = clip;
        target.Height = Fixed.FromInt(36);
        var missile = sim.SpawnProjectile(owner, kind, target);
        // Revenant aims from the temporarily raised source, sixteen units above its feet.
        var sourceZ = kind == ProjectileKind.RevenantTracer ? 16 : 0;
        var dz = heightCorrection - sourceZ;
        var length = Math.Sqrt(10000 + dz * dz);
        Assert.InRange(Math.Abs(missile.VelocityX.ToDouble() - missile.Speed * 100 / length), 0, 1.0 / 65536);
        Assert.InRange(Math.Abs(missile.VelocityZ.ToDouble() - missile.Speed * dz / length), 0, 1.0 / 65536);
    }

    [Fact]
    public void TargetBobDoesNotChangeSpawnMissileTrajectory()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        var first = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        target.FloatBob = true; target.FloatBobPhase = 16;
        var second = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        Assert.Equal(first.Z, second.Z); Assert.Equal(first.VelocityX, second.VelocityX);
        Assert.Equal(first.VelocityY, second.VelocityY); Assert.Equal(first.VelocityZ, second.VelocityZ);
    }

    [Fact]
    public void PlayerWeaponOriginUsesAttackHeightWithoutMonsterBob()
    {
        var sim = Room(); var player = sim.Players.Single();
        var first = sim.SpawnProjectile(player, ProjectileKind.Rocket);
        player.FloatBob = true; player.FloatBobPhase = 16;
        var second = sim.SpawnProjectile(player, ProjectileKind.Rocket);
        Assert.Equal(first.Z, second.Z); Assert.Equal(first.VelocityZ, second.VelocityZ);
    }

    [Fact]
    public void RestoredBobClockAndTuningProduceSameNextShot()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        owner.FloatBob = true; owner.FloatBobFactor = 0.5; owner.FloatBobStrength = 2;
        ActorPropertyActions.SetFloatBobPhase(owner, 12); sim.Thinkers.Clock.Restore(8);
        var saved = SimSavegame.Write(sim, true);
        var first = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        var expected = (first.Z, first.VelocityX, first.VelocityZ);
        SimSavegame.Apply(sim, saved);
        var second = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        Assert.Equal(expected, (second.Z, second.VelocityX, second.VelocityZ));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
