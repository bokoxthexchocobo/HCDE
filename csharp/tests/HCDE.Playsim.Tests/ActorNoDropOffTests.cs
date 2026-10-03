namespace HCDE.Playsim.Tests;

public class ActorNoDropOffTests
{
    [Theory]
    [InlineData(24, true, false, true, true)]
    [InlineData(25, true, false, true, false)]
    [InlineData(48, true, false, false, true)]
    [InlineData(48, true, false, true, false)]
    [InlineData(48, false, true, false, true)]
    [InlineData(48, false, true, true, false)]
    [InlineData(48, true, true, true, false)]
    public void FlagOverridesDropOffAndFloatExceptions(int drop, bool allowDropOff, bool floating, bool noDropOff, bool enters)
    {
        var sim = GameplayFoundationTests.TwoRooms((short)-drop, 128); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.AllowDropOff = allowDropOff; actor.Floating = floating; actor.NoDropOff = noDropOff;
        actor.VelocityX = Fixed.FromInt(4); sim.Tick();
        Assert.Equal(enters ? 1 : 0, actor.SectorIndex);
        if (!enters) Assert.Equal(0, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerUsesExplicitNoDropOffFlag(bool flag)
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 128); var player = sim.Players.Single();
        player.X = Fixed.FromInt(-1); player.NoDropOff = flag; player.VelocityX = Fixed.FromInt(4);
        sim.Tick(); Assert.Equal(flag ? 0 : 1, player.SectorIndex);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(48, true)]
    public void FlagRespectsCustomMaxDropOffHeight(int limit, bool enters)
    {
        var sim = GameplayFoundationTests.TwoRooms(-32, 128); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.NoDropOff = true; actor.AllowDropOff = true;
        actor.MaxDropOffHeight = Fixed.FromInt(limit); actor.VelocityX = Fixed.FromInt(4);
        sim.Tick(); Assert.Equal(enters ? 1 : 0, actor.SectorIndex);
    }

    [Fact]
    public void SerializedFlagRestoresOrdinaryActorMovementPolicy()
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 128); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.NoDropOff = true; actor.AllowDropOff = true; actor.VelocityX = Fixed.FromInt(4);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        actor.NoDropOff = false; sim.RestoreState(state); sim.Tick(); Assert.Equal(0, actor.SectorIndex);
        actor.NoDropOff = false; actor.VelocityX = Fixed.FromInt(4); sim.Tick(); Assert.Equal(1, actor.SectorIndex);
    }
}
