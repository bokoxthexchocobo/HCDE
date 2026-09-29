using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UseTraceTests
{
    [Fact]
    public void FrontUseWithinRangeChangesTheTaggedLight()
    {
        var sim = Facing(32, 180, Switch(138, 1));
        Press(sim);
        Assert.Equal(255, sim.LightOf(0));
    }

    [Theory]
    [InlineData(80, 64, 128)]
    [InlineData(80, 96, 255)]
    [InlineData(40, 32, 128)]
    public void UseRangeLimitsTheTrace(double x, double range, int light)
    {
        var sim = Facing(x, 180, Switch(138, 1));
        sim.Players.Single().UseRange = range;
        Press(sim);
        Assert.Equal(light, sim.LightOf(0));
    }

    [Fact]
    public void BackSideDoesNotActivateAFrontOnlySwitch()
    {
        var sim = Facing(-32, 0, Switch(138, 1));
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void BackSideActivatesWhenTheLineAllowsIt()
    {
        var sim = Facing(-32, 0, Switch(138, 1, useBack: true));
        Press(sim);
        Assert.Equal(255, sim.LightOf(0));
    }

    [Fact]
    public void UseBackOnlyLineIgnoresTheFront()
    {
        var sim = Facing(32, 180, Switch(138, 1, useBack: true));
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void OpenFrontOnlyLineDoesNotEatTheSwitchBeyondIt()
    {
        var blocking = Switch(138, 1);
        var beyond = new LevelLine
        {
            X1 = 16, Y1 = 64, X2 = 16, Y2 = -64,
            SideFront = 1, SideBack = 0,
            Special = 139, Tag = 2,
        };
        var sim = Facing(-32, 0, blocking, beyond);
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.Equal(35, sim.LightOf(1));
    }

    private static void Press(AuthoritySimulation sim)
    {
        Assert.True(sim.QueueCommand(0, new PlayerCommand { Use = true }));
        sim.Tick();
    }

    private static AuthoritySimulation Facing(double x, double angle, params LevelLine[] lines)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.DoomBinary,
            Things = [new LevelThing { Type = 1, X = x }],
            Sectors =
            [
                new LevelSector { Tag = 1, LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { Index = 1, Tag = 2, LightLevel = 160, CeilingHeight = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = lines,
        });
        sim.Players.Single().Angle = BamAngle.FromDegrees(angle);
        return sim;
    }

    private static LevelLine Switch(int special, int tag, bool useBack = false) => new()
    {
        X1 = 0, Y1 = -64, X2 = 0, Y2 = 64,
        SideFront = 0, SideBack = 1,
        Special = special, Tag = tag, PlayerUseBack = useBack,
    };
}
