using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomFloorDebrisTests
{
    [Theory]
    [InlineData(79, 4)]
    [InlineData(80, 1)]
    [InlineData(81, 4)]
    public void NativeDefaultsExcludeDebrisFromCollision(int type, int height)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.Equal(20, actor.Radius.ToDouble()); Assert.Equal(height, actor.Height.ToDouble());
        Assert.True(actor.NoBlockmap); Assert.False(actor.Solid); Assert.False(actor.Shootable);
        Assert.Equal(default, actor.ProjectilePassHeight);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null; actor.X = Fixed.FromInt(30);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(1);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.False(missile.Destroyed);
    }

    [Theory]
    [InlineData(79)]
    [InlineData(80)]
    [InlineData(81)]
    public void GroundedDebrisFollowsRaisedAndLoweredFloor(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        sim.Floors[0] = 24; ActorPhysics.FitToSector(sim, actor); Assert.Equal(24, actor.Z.ToDouble());
        sim.Floors[0] = 8; ActorPhysics.FitToSector(sim, actor); Assert.Equal(8, actor.Z.ToDouble());
    }

    [Fact]
    public void ExplicitPrimaryPatchCanEnableBlockmapAndSolidity()
    {
        var actor = new Actor { NoBlockmap = false, Solid = true, Shootable = true };
        DoomDecorationDefaults.Apply(actor, 80, new DehackedActor { BitsPatched = true });
        Assert.False(actor.NoBlockmap); Assert.True(actor.Solid); Assert.True(actor.Shootable);
        Assert.Equal(1, actor.Height.ToDouble());
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }],
    });
}
