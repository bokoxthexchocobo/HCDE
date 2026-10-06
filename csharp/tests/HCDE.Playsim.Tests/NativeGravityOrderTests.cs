using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeGravityOrderTests
{
    [Theory]
    [InlineData(100, 0, 1, false, 100, -1)]
    [InlineData(100, 4, 1, false, 104, 3)]
    [InlineData(100, -4, 1, false, 96, -5)]
    [InlineData(4, -4, 1, false, 0, 0)]
    [InlineData(4, -8, 1, false, 0, 0)]
    [InlineData(100, 4, 1, true, 104, 4)]
    [InlineData(100, 4, -0.5, false, 104, 4.5)]
    public void CurrentVelocityMovesBeforeGravityAndLandingClearsFall(int z, int velocity, double gravity,
        bool noGravity, double expectedZ, double expectedVelocity)
    {
        var (sim, actor) = Setup(); actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        actor.Gravity = Fixed.FromDouble(gravity); actor.NoGravity = noGravity;
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expectedZ, actor.Z.ToDouble()); Assert.Equal(expectedVelocity, actor.VelocityZ.ToDouble());
        Assert.Equal(expectedZ == 0, actor.OnGround);
    }

    [Fact]
    public void SectorAndActorGravityAffectNextDisplacement()
    {
        var (sim, actor) = Setup(); sim.Level.Sectors[0].Gravity = 0.5;
        actor.Gravity = Fixed.FromDouble(0.25); actor.Z = Fixed.FromInt(100);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(100, actor.Z.ToDouble()); Assert.Equal(-0.125, actor.VelocityZ.ToDouble());
        ActorPhysics.Step(sim, actor);
        Assert.Equal(99.875, actor.Z.ToDouble()); Assert.Equal(-0.25, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void SaveRestoreContinuesTheSameGravitySequence()
    {
        var (sim, actor) = Setup(); actor.Z = Fixed.FromInt(100); actor.VelocityZ = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded) = Setup(); restored.RestoreState(state);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(restored, loaded);
        Assert.Equal(107, actor.Z.ToDouble()); Assert.Equal(2, actor.VelocityZ.ToDouble());
        Assert.Equal(actor.Z, loaded.Z); Assert.Equal(actor.VelocityZ, loaded.VelocityZ);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var actor = sim.AddBot(0, 0); actor.Brain = null; actor.OnGround = false;
        return (sim, actor);
    }
}
