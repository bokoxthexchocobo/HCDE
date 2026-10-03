namespace HCDE.Playsim.Tests;

public class ActorBlastedTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public void BlastedBypassesDropoffEvenWithNoDropOff(bool blasted, bool noDropOff, bool enters)
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 128); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.AllowDropOff = false; actor.NoDropOff = noDropOff; actor.Blasted = blasted;
        actor.VelocityX = Fixed.FromInt(4); sim.Tick();
        Assert.Equal(enters ? 1 : 0, actor.SectorIndex);
        if (blasted) Assert.True(actor.Blasted);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(1, 0, true)]
    [InlineData(0, 1, true)]
    public void BlastedClearsOnlyWhenBothHorizontalVelocitiesStop(int x, int y, bool retained)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); var actor = sim.AddBot(-64, 80);
        actor.Brain = null; actor.Blasted = true;
        actor.VelocityX = Fixed.FromInt(x); actor.VelocityY = Fixed.FromInt(y);
        sim.Tick(); Assert.Equal(retained, actor.Blasted);
    }

    [Fact]
    public void FrictionStoppingMotionClearsBlastedOnSameTic()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); var actor = sim.AddBot(-64, 80);
        actor.Brain = null; actor.Blasted = true; actor.VelocityX = Fixed.FromDouble(0.05);
        sim.Tick(); Assert.Equal(0, actor.VelocityX.Raw); Assert.False(actor.Blasted);
    }

    [Fact]
    public void BlastedDoesNotBypassWallCollision()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); var actor = sim.AddBot(-64, 80);
        actor.Brain = null; actor.Blasted = true;
        Assert.False(ActorPhysics.TryMove(sim, actor, -140, 80, out _));
        Assert.Equal(-64, actor.X.ToDouble());
    }

    [Fact]
    public void AcsSetsQueriesAndClearsBlasted()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "blasted", true));
        Assert.True(AcsActorFlags.TryGet(actor, "BLASTED", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "Blasted", false)); Assert.False(actor.Blasted);
    }

    [Fact]
    public void MovingBlastedFlagChangesChecksum()
    {
        var first = GameplayFoundationTests.TwoRooms(0, 128); var second = GameplayFoundationTests.TwoRooms(0, 128);
        foreach (var sim in new[] { first, second })
        {
            var actor = sim.AddBot(-64, 80); actor.Brain = null; actor.VelocityX = Fixed.FromInt(1);
            actor.Blasted = ReferenceEquals(sim, first); sim.Tick();
        }
        Assert.NotEqual(first.Checksum, second.Checksum);
    }
}
