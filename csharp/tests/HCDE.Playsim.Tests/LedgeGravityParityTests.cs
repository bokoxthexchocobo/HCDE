namespace HCDE.Playsim.Tests;

public class LedgeGravityParityTests
{
    [Theory]
    [InlineData(0, 0, false, 0, -2)]
    [InlineData(1, 0, false, 1, -1)]
    [InlineData(0, 1, false, 1, 0)]
    [InlineData(0, -1, false, -1, -2)]
    [InlineData(0, 0, true, 0, 0)]
    public void OnlyRestingAtOldFloorGetsDoubleGravity(int z, int velocity, bool noGravity,
        int expectedZ, int expectedVelocity)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(1); player.X = Fixed.FromInt(-1); player.Z = Fixed.FromInt(z);
        player.VelocityX = Fixed.FromInt(4); player.VelocityZ = Fixed.FromInt(velocity); player.NoGravity = noGravity;
        ActorPhysics.Step(sim, player);
        Assert.Equal(1, player.SectorIndex); Assert.Equal(expectedZ, player.Z.ToDouble());
        Assert.Equal(expectedVelocity, player.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(0.25, 0.5, -0.25)]
    [InlineData(2, 1, -4)]
    [InlineData(-0.5, 1, 1)]
    public void DoubledAccelerationUsesBothGravityFactors(double actorGravity, double sectorGravity, double expected)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(1); player.X = Fixed.FromInt(-1); player.VelocityX = Fixed.FromInt(4);
        player.Gravity = Fixed.FromDouble(actorGravity); sim.Level.Sectors[1].Gravity = sectorGravity;
        ActorPhysics.Step(sim, player);
        Assert.Equal(0, player.Z.ToDouble()); Assert.Equal(expected, player.VelocityZ.ToDouble());
    }

    [Fact]
    public void NextTicUsesNormalGravityAndSaveRestoresContinuation()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(1); player.X = Fixed.FromInt(-1); player.VelocityX = Fixed.FromInt(4);
        ActorPhysics.Step(sim, player);
        Assert.Equal(-2, player.VelocityZ.ToDouble());
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = GameplayFoundationTests.TwoRooms(-24, 128); restored.RestoreState(state);
        var loaded = restored.Players.Single();
        ActorPhysics.Step(sim, player); ActorPhysics.Step(restored, loaded);
        Assert.Equal(-2, player.Z.ToDouble()); Assert.Equal(-3, player.VelocityZ.ToDouble());
        Assert.Equal(player.Z, loaded.Z); Assert.Equal(player.VelocityZ, loaded.VelocityZ);
    }

    [Fact]
    public void NonplayerWithDropoffPermissionUsesSameLedgeRule()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.AddBot(-1, 0);
        actor.Radius = Fixed.FromInt(1); actor.AllowDropOff = true; actor.VelocityX = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(1, actor.SectorIndex); Assert.Equal(0, actor.Z.ToDouble());
        Assert.Equal(-2, actor.VelocityZ.ToDouble());
    }
}
