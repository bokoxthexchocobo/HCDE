namespace HCDE.Playsim.Tests;

public class FallingFlagTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(0.25, 0.2265625)]
    public void FallingFlagPreservesLedgeMomentumAboveThreshold(double speed, double expected)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.AddBot(1, 80);
        actor.Brain = null; actor.AllowDropOff = true; actor.SectorIndex = 1; actor.Z = default;
        ActorPropertyActions.ChangeFlag(actor, "FALLING", true); actor.VelocityX = Fixed.FromDouble(speed);
        ActorPhysics.Step(sim, actor);
        Assert.True(actor.Falling); Assert.False(actor.IsDead); Assert.False(actor.Corpse);
        Assert.Equal(expected, actor.VelocityX.ToDouble());
    }

    [Fact]
    public void FallingStillReceivesFrictionAwayFromLedge()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var actor = sim.AddBot(-40, 80);
        actor.Brain = null; actor.Falling = true; actor.VelocityX = Fixed.FromInt(1);
        ActorPhysics.Step(sim, actor); Assert.Equal(ActorPhysics.GroundFriction, actor.VelocityX.ToDouble());
    }

    [Fact]
    public void ArchivePreservesFlagAndLegacySaveClearsIt()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128); var player = sim.Players.Single();
        var legacy = SimSavegame.Write(sim); player.Falling = true; var bytes = SimSavegame.Write(sim);
        player.Falling = false; SimSavegame.Apply(sim, bytes);
        Assert.True(player.Falling); Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.False(player.Falling);
    }

    [Fact]
    public void FlagParticipatesInChecksum()
    {
        var first = GameplayFoundationTests.TwoRooms(-24, 128); var second = GameplayFoundationTests.TwoRooms(-24, 128);
        second.Players.Single().Falling = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }
}
