using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargePlaneTests
{
    [Theory]
    [InlineData(2, -2, 0, 0)]
    [InlineData(2, -4, 0, 0)]
    [InlineData(70, 4, 72, -4)]
    [InlineData(75, -2, 72, 0)]
    [InlineData(-4, 2, 0, -2)]
    public void ChargeUsesNativeSignedPlaneResponse(int z, int velocity, int expectedZ, int expectedVelocity)
    {
        var (sim, actor) = Setup(); actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expectedZ, actor.Z.ToDouble()); Assert.Equal(expectedVelocity, actor.VelocityZ.ToDouble());
        Assert.True(actor.Brain!.Charging);
    }

    [Fact]
    public void CeilingContactRemainsStrict()
    {
        var (sim, actor) = Setup(); actor.Z = Fixed.FromInt(70); actor.VelocityZ = Fixed.FromInt(2);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(72, actor.Z.ToDouble()); Assert.Equal(2, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void SaveRestoresCeilingReflectionBeforeNextMovement()
    {
        var (sim, actor) = Setup(); actor.Z = Fixed.FromInt(70); actor.VelocityZ = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded) = Setup(); restored.RestoreState(state);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(restored, loaded);
        Assert.Equal(68, actor.Z.ToDouble()); Assert.Equal(actor.Z, loaded.Z);
        Assert.Equal(actor.VelocityZ, loaded.VelocityZ); Assert.True(loaded.Brain!.Charging);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0, 3006); var target = sim.AddBot(1000, 0);
        actor.Brain!.StartCharge(actor, target);
        actor.VelocityX = Fixed.FromInt(1); actor.VelocityY = default;
        return (sim, actor);
    }
}
