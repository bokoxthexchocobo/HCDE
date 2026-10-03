namespace HCDE.Playsim.Tests;

public class DropOffSupportTests
{
    [Theory]
    [InlineData(24, 0, true, true)]
    [InlineData(24, 1, true, false)]
    [InlineData(0, 24, true, true)]
    [InlineData(0, 25, true, false)]
    [InlineData(24, 64, false, true)]
    [InlineData(24, -10, true, true)]
    public void StandingOnActorUsesSupportHeightForDrop(int drop, int z, bool onMobj, bool enters)
    {
        var sim = GameplayFoundationTests.TwoRooms((short)-drop, 256); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.AllowDropOff = false; actor.Floating = false;
        actor.Z = Fixed.FromInt(z); actor.OnMobj = onMobj;
        Assert.Equal(enters, ActorPhysics.TryMove(sim, actor, 3, 80, out _));
        Assert.Equal(enters ? 3 : -1, actor.X.ToDouble());
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void MissileExceptionIsIndependentOfDropOffFlag(bool allowDropOff, bool noDropOff, bool enters)
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 256);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(-1); missile.Y = Fixed.FromInt(80); missile.Z = Fixed.FromInt(64); missile.SectorIndex = 0;
        missile.AllowDropOff = allowDropOff; missile.NoDropOff = noDropOff;
        Assert.Equal(enters, ActorPhysics.TryMove(sim, missile, 3, 80, out _));
    }
}
