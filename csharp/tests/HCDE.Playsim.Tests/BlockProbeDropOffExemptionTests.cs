namespace HCDE.Playsim.Tests;

public class BlockProbeDropOffExemptionTests
{
    [Theory]
    [InlineData(false, false, false, false, false, true)]
    [InlineData(false, true, false, false, false, false)]
    [InlineData(false, false, true, false, false, false)]
    [InlineData(false, true, false, true, false, false)]
    [InlineData(false, false, true, true, false, false)]
    [InlineData(false, false, false, false, true, true)]
    [InlineData(false, true, true, true, true, false)]
    [InlineData(true, false, false, false, false, true)]
    [InlineData(true, true, false, true, false, false)]
    [InlineData(true, false, false, true, true, true)]
    public void DropOffProbeUsesNativeExemptions(bool missile, bool allowDropOff, bool floating,
        bool noDropOff, bool blasted, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128);
        Actor actor = missile ? sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma)
            : sim.AddBot(-40, 80);
        actor.X = Fixed.FromInt(-40); actor.Y = Fixed.FromInt(80); actor.Z = default;
        actor.AllowDropOff = allowDropOff;
        actor.Floating = floating;
        actor.NoDropOff = noDropOff;
        actor.Blasted = blasted;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32, zOffset: 48));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-24, false)]
    [InlineData(-25, true)]
    public void DropOffProbeIncludesAdjacentFloor(short adjacentFloor, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(adjacentFloor, 128);
        var actor = sim.AddBot(-1, 80);
        actor.AllowDropOff = false;
        actor.Floating = false;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32));
        Assert.Equal(before, SimSavegame.Write(sim));
    }
}
