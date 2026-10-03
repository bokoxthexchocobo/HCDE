using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedSlideTests
{
    [Theory]
    [InlineData(false, 20)]
    [InlineData(false, -20)]
    [InlineData(true, 20)]
    [InlineData(true, -20)]
    public void BlastedSelectsSupportedWallSlideWithoutCanSlide(bool reverse, int tangent)
    {
        var sim = Room(reverse); var control = Room(reverse);
        var actor = Assert.Single(sim.Actors); var slider = Assert.Single(control.Actors);
        actor.Blasted = true; slider.CanSlide = true;
        foreach (var moving in new[] { actor, slider })
        { moving.VelocityX = Fixed.FromInt(reverse ? -1000 : 1000); moving.VelocityY = Fixed.FromInt(tangent); }
        sim.Tick(); control.Tick();
        Assert.False(actor.CanSlide); Assert.True(actor.Blasted);
        Assert.Equal(slider.X, actor.X); Assert.Equal(slider.Y, actor.Y);
        Assert.Equal(slider.VelocityX, actor.VelocityX); Assert.Equal(slider.VelocityY, actor.VelocityY);
        Assert.InRange(Math.Abs(actor.X.ToDouble()), 0.01, 4);
        Assert.InRange(actor.Y.ToDouble(), tangent - 0.01, tangent + 0.01);
        Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void HeadOnWallStopClearsBlastedAfterSlide()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Blasted = true;
        actor.VelocityX = Fixed.FromInt(1000); sim.Tick();
        Assert.Equal(0, actor.VelocityX.Raw); Assert.Equal(0, actor.VelocityY.Raw); Assert.False(actor.Blasted);
        Assert.InRange(actor.X.ToDouble(), 0.01, 4);
    }

    [Fact]
    public void RestoredBlastedFlagResumesSlide()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Blasted = true;
        actor.VelocityX = Fixed.FromInt(1000); actor.VelocityY = Fixed.FromInt(20);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        actor.Blasted = false; sim.RestoreState(state); sim.Tick();
        Assert.InRange(actor.X.ToDouble(), 0.01, 4); Assert.InRange(actor.Y.ToDouble(), 19.99, 20.01);
        Assert.True(actor.Blasted);
    }

    private static AuthoritySimulation Room(bool reverse = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
            Lines = [new LevelLine { X1 = reverse ? -24 : 24, X2 = reverse ? -24 : 24,
                Y1 = -128, Y2 = 128, SideBack = -1 }] });
        Assert.Single(sim.Actors).Brain = null; return sim;
    }
}
