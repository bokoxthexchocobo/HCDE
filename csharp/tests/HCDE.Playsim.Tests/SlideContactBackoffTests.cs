using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SlideContactBackoffTests
{
    [Theory]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(4)]
    public void SlideBacksOffBeforeWallContact(int tangentVelocity)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = -1 }],
            Things = [new LevelThing { Type = 1, X = 17 }]
        });
        var actor = sim.Players.Single();
        actor.Radius = Fixed.FromInt(16);
        actor.VelocityX = Fixed.FromInt(-4);
        actor.VelocityY = Fixed.FromInt(tangentVelocity);
        ActorPhysics.Step(sim, actor);
        Assert.InRange(actor.X.ToDouble(), 16.06, 16.063);
        Assert.Equal(0, actor.VelocityX.ToDouble());
        Assert.Equal(Math.Sign(tangentVelocity), Math.Sign(actor.Y.ToDouble()));
    }
}
