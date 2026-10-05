using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileClassDamagePatchTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(655360)]
    public void SpeedAndDimensionsApplyBeforeAim(int speed)
    {
        var sim = Room($"Thing 34\nSpeed = {speed}\nWidth = 196608\nHeight = 786432\n");
        var rocket = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(10, rocket.Speed); Assert.Equal(10, rocket.VelocityX.ToDouble());
        Assert.Equal(3, rocket.Radius.ToDouble()); Assert.Equal(12, rocket.Height.ToDouble());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public void PatchedBaseDamageControlsDirectImpact(int damage)
    {
        var sim = Room($"Thing 35\nMissile damage = {damage}\n");
        var player = sim.Players.Single(); player.X = Fixed.FromInt(-200);
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000;
        var missile = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        Assert.Equal(damage, missile.Damage);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick();
        Assert.True(missile.Destroyed);
        var lost = 1000 - target.Health;
        if (damage <= 0) Assert.Equal(0, lost);
        else { Assert.InRange(lost, damage, damage * 8); Assert.Equal(0, lost % damage); }
    }

    [Fact]
    public void GroupOnlyPatchPreservesMissileDefaults()
    {
        var sim = Room("Thing 34\nSplash group = 7\n");
        var rocket = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(20, rocket.Speed); Assert.Equal(20, rocket.Damage);
        Assert.Equal(6, rocket.Radius.ToDouble()); Assert.Equal(8, rocket.Height.ToDouble());
    }

    private static AuthoritySimulation Room(string patch) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] }, dehacked: DehackedPatch.Apply(patch));
}
