using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomMarineDecorationTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(15)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(24)]
    public void CorpseDecorationsUseBaseCollisionDefaults(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.Equal(20, actor.Radius.ToDouble()); Assert.Equal(16, actor.Height.ToDouble());
        Assert.False(actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.NoBlockmap);
        Assert.False(actor.IsMonster); Assert.Null(actor.Brain); Assert.Equal(default, actor.ProjectilePassHeight);
        Assert.False(Impact(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StalagmiteUsesFullHeightEvenWithMissileClipping(bool clip)
    {
        var sim = Room(5050, clip); var actor = sim.Actors.Single();
        Assert.Equal(16, actor.Radius.ToDouble()); Assert.Equal(48, actor.Height.ToDouble());
        Assert.True(actor.Solid); Assert.False(actor.Shootable); Assert.Equal(default, actor.ProjectilePassHeight);
        Assert.True(Impact(sim));
    }

    [Fact]
    public void MapGibsDoNotInheritRealGibsMovementFlags()
    {
        var actor = Room(24).Actors.Single();
        Assert.False(actor.NoTeleport); Assert.False(actor.AllowDropOff); Assert.False(actor.NoGravity);
        Assert.False(actor.Solid); Assert.False(actor.Shootable);
    }

    [Fact]
    public void MonsterDefaultsRemainDistinctFromDeadDecorationDefaults()
    {
        var living = Room(3001).Actors.Single(); var dead = Room(20).Actors.Single();
        Assert.True(living.IsMonster); Assert.True(living.Shootable); Assert.True(living.Solid);
        Assert.NotNull(living.Brain); Assert.False(dead.IsMonster); Assert.Null(dead.Brain);
        Assert.Equal(56, living.Height.ToDouble()); Assert.Equal(16, dead.Height.ToDouble());
    }

    private static bool Impact(AuthoritySimulation sim)
    {
        sim.Actors.Single().X = Fixed.FromInt(30);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); return missile.Destroyed;
    }

    private static AuthoritySimulation Room(int type, bool clip = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }],
    }, compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
}
