using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomDecorationDefaultsTests
{
    [Theory]
    [InlineData(85, 80)]
    [InlineData(86, 60)]
    [InlineData(2028, 48)]
    [InlineData(30, 52)]
    [InlineData(32, 52)]
    [InlineData(31, 40)]
    [InlineData(33, 40)]
    [InlineData(36, 40)]
    [InlineData(37, 40)]
    public void NativeLampAndColumnDefaultsReachMapActors(int type, int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }],
        });
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(16, actor.Radius.ToDouble());
        Assert.Equal(-16, actor.ProjectilePassHeight.ToDouble()); Assert.True(actor.Solid); Assert.False(actor.Shootable);
        Assert.False(actor.IsMonster); Assert.Null(actor.Brain);
    }

    [Theory]
    [InlineData(25, 16, 64, true)]
    [InlineData(26, 16, 64, true)]
    [InlineData(27, 16, 56, true)]
    [InlineData(28, 16, 64, true)]
    [InlineData(29, 16, 42, true)]
    [InlineData(70, 16, 32, true)]
    [InlineData(34, 20, 14, false)]
    [InlineData(35, 16, 60, true)]
    [InlineData(41, 16, 54, true)]
    [InlineData(42, 16, 26, true)]
    [InlineData(43, 16, 56, true)]
    [InlineData(44, 16, 68, true)]
    [InlineData(45, 16, 68, true)]
    [InlineData(46, 16, 68, true)]
    [InlineData(47, 16, 40, true)]
    [InlineData(48, 16, 128, true)]
    [InlineData(54, 32, 108, true)]
    [InlineData(55, 16, 37, true)]
    [InlineData(56, 16, 37, true)]
    [InlineData(57, 16, 37, true)]
    public void NativeGroundDecorationDefaultsReachMapActors(int type, int radius, int height, bool solid)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
        });
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(radius, actor.Radius.ToDouble()); Assert.Equal(height, actor.Height.ToDouble());
        Assert.Equal(solid, actor.Solid); Assert.False(actor.Shootable);
        Assert.Equal(-16, actor.ProjectilePassHeight.ToDouble()); Assert.False(actor.IsMonster);
    }

    [Theory]
    [InlineData(34, false)]
    [InlineData(35, true)]
    public void CandleSolidityControlsLowMissileCollision(int type, bool stops)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type, X = 30 }],
        });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(8);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stops, missile.Destroyed);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void SpawnedLampUsesCompatibilityPassHeight(bool clip, bool stops)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 85, X = 30 }],
        }, compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stops, missile.Destroyed);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void BurningBarrelBlocksOrPassesWithoutExploding(bool clip, bool stops)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 70, X = 30 }],
        }, compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var barrel = sim.Actors.Single(); var health = barrel.Health;
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stops, missile.Destroyed);
        Assert.False(barrel.Destroyed); Assert.Equal(health, barrel.Health);
    }

    [Fact]
    public void ExplicitPatchDimensionsAndFlagsTakePrecedence()
    {
        var actor = new Actor { Radius = Fixed.FromInt(24), Height = Fixed.FromInt(72), Solid = false, Shootable = true };
        var patch = new DehackedActor { WidthPatched = true, HeightPatched = true, BitsPatched = true };
        DoomDecorationDefaults.Apply(actor, 85, patch);
        Assert.Equal(24, actor.Radius.ToDouble()); Assert.Equal(72, actor.Height.ToDouble());
        Assert.False(actor.Solid); Assert.True(actor.Shootable); Assert.Equal(-16, actor.ProjectilePassHeight.ToDouble());
    }

    [Fact]
    public void UnrelatedPatchDoesNotReplaceNativeDimensions()
    {
        var actor = new Actor(); var patch = new DehackedActor { Health = 100 };
        DoomDecorationDefaults.Apply(actor, 86, patch);
        Assert.Equal(16, actor.Radius.ToDouble()); Assert.Equal(60, actor.Height.ToDouble()); Assert.False(actor.Shootable);
    }
}
