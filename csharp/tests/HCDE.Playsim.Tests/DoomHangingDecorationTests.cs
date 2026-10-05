using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomHangingDecorationTests
{
    [Theory]
    [InlineData(49, 68, 16, true)]
    [InlineData(50, 84, 16, true)]
    [InlineData(51, 84, 16, true)]
    [InlineData(52, 68, 16, true)]
    [InlineData(53, 52, 16, true)]
    [InlineData(59, 84, 20, false)]
    [InlineData(60, 68, 20, false)]
    [InlineData(61, 84, 20, false)]
    [InlineData(62, 52, 20, false)]
    [InlineData(63, 68, 20, false)]
    [InlineData(73, 88, 16, true)]
    [InlineData(74, 88, 16, true)]
    [InlineData(75, 64, 16, true)]
    [InlineData(76, 64, 16, true)]
    [InlineData(77, 64, 16, true)]
    [InlineData(78, 64, 16, true)]
    public void NativeInheritedDefaultsAndCeilingPlacement(int type, int height, int radius, bool solid)
    {
        var sim = Room(type); var actor = Assert.Single(sim.Actors);
        Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(radius, actor.Radius.ToDouble());
        Assert.Equal(solid, actor.Solid); Assert.False(actor.Shootable);
        Assert.True(actor.NoGravity); Assert.True(actor.SpawnCeiling); Assert.False(actor.OnGround);
        Assert.Equal(160 - height, actor.Z.ToDouble()); Assert.Equal(default, actor.ProjectilePassHeight);
        actor.Tick(); Assert.Equal(160 - height, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(49, true)]
    [InlineData(63, false)]
    public void HangingSolidityControlsMissileCollision(int type, bool stops)
    {
        var sim = Room(type); var actor = sim.Actors.Single(); actor.X = Fixed.FromInt(30);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(100);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stops, missile.Destroyed);
    }

    [Fact]
    public void PrimaryPatchFlagsCanDisableCeilingAndGravityDefaults()
    {
        var actor = new Actor { NoGravity = false, SpawnCeiling = false, Solid = false, Shootable = true };
        DoomDecorationDefaults.Apply(actor, 49, new DehackedActor { BitsPatched = true });
        Assert.False(actor.NoGravity); Assert.False(actor.SpawnCeiling); Assert.False(actor.Solid); Assert.True(actor.Shootable);
        Assert.Equal(68, actor.Height.ToDouble());
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
