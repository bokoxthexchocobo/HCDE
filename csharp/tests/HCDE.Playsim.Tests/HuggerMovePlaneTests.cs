using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HuggerMovePlaneTests
{
    [Theory]
    [InlineData(true, false, 48, 128, 32, true, 48)]
    [InlineData(true, false, -48, 128, 32, true, -48)]
    [InlineData(false, true, 0, 96, 100, true, 40)]
    [InlineData(false, true, 0, 160, 0, true, 104)]
    [InlineData(true, true, 48, 128, 32, true, 48)]
    [InlineData(true, false, 48, 96, 32, false, 32)]
    [InlineData(false, true, 48, 96, 32, false, 32)]
    public void HuggerMoveUsesDestinationPlane(bool floorHugger, bool ceilingHugger,
        short floor, short ceiling, int startZ, bool passes, int endZ)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, ceiling);
        var actor = new Actor
        {
            X = Fixed.FromInt(-40), Y = Fixed.FromInt(80), Z = Fixed.FromInt(startZ),
            SectorIndex = 0, FloorHugger = floorHugger, CeilingHugger = ceilingHugger,
            AllowDropOff = true, Height = Fixed.FromInt(56)
        };
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, 40, 80, out _));
        Assert.Equal(passes ? 40 : -40, actor.X.ToDouble());
        Assert.Equal(endZ, actor.Z.ToDouble());
        Assert.Equal(passes ? 1 : 0, actor.SectorIndex);
    }

    [Fact]
    public void CeilingHuggingMissileCanAlignUpward()
    {
        var sim = GameplayFoundationTests.TwoRooms(24, 128);
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            X = Fixed.FromInt(-40), Y = Fixed.FromInt(80), SectorIndex = 0, CeilingHugger = true
        };
        Assert.True(ActorPhysics.TryMove(sim, missile, 40, 80, out _));
        Assert.Equal(128 - missile.Height.ToDouble(), missile.Z.ToDouble());
    }

    [Fact]
    public void HuggerStillRespectsBlockingWall()
    {
        var geometry = GameplayFoundationTests.TwoRooms(48, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags = LevelLine.BlockingFlag
            }).ToArray()
        });
        var actor = new Actor { X = Fixed.FromInt(-40), SectorIndex = 0, FloorHugger = true };
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.Equal(0, actor.Z.ToDouble());
    }
}
