using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportFlyingHeightTests
{
    [Theory]
    [InlineData(true, true, 256, 96)]
    [InlineData(true, true, 128, 72)]
    [InlineData(true, false, 256, 64)]
    [InlineData(false, true, 256, 64)]
    public void OrdinaryTeleportPreservesOnlyFlyingPlayerHeight(bool player, bool noGravity, short ceiling, int expected)
    {
        var geometry = GameplayFoundationTests.TwoRooms(64, ceiling).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 40 }],
        });
        Actor actor = player ? Assert.Single(sim.Players) : sim.AddBot(-40, 0);
        actor.NoGravity = noGravity; actor.Z = Fixed.FromInt(32); actor.OnGround = false;
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        Assert.Equal(1, actor.SectorIndex);
        Assert.Equal(expected, actor.Z.ToDouble());
        Assert.Equal(expected == 64, actor.OnGround);
        Assert.False(actor.OnMobj);
        Assert.Equal(actor.X, actor.PreviousX); Assert.Equal(actor.Y, actor.PreviousY);
        Assert.Equal(actor.Z, actor.PreviousZ);
        Assert.Equal(actor.PosPlusZ(0), actor.InterpolatedPosition(0.5));
    }

    [Fact]
    public void ClearInterpolationResetsPositionHistoryAndPreservesVelocity()
    {
        var actor = new Actor { VelocityX = Fixed.FromInt(7), Angle = BamAngle.FromDegrees(37) };
        actor.SetXYZ(10, 20, 30); actor.RememberPosition(); actor.SetXYZ(-5, 50, 100);
        actor.ClearInterpolation();
        Assert.Equal((-5.0, 50.0, 100.0), actor.InterpolatedPosition(0));
        Assert.Equal(actor.PosPlusZ(0), actor.InterpolatedPosition(0.5));
        Assert.Equal(7, actor.VelocityX.ToDouble()); Assert.Equal(BamAngle.FromDegrees(37), actor.Angle);
    }
}
