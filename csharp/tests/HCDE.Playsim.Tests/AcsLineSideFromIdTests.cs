using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLineSideFromIdTests
{
    [Fact]
    public void LineFromId_UsesTriggerLineWhenIdZero()
    {
        var sim = Room();
        var trigger = sim.Level.Lines[1];
        sim.AcsTriggerLine = trigger;
        Assert.Equal(trigger.Index, AcsLineSideFromId.LineFromId(sim, 0));
        Assert.Equal(1, AcsLineSideFromId.LineFromId(sim, 9));
        Assert.Equal(-1, AcsLineSideFromId.LineFromId(sim, 99));
    }

    [Fact]
    public void SideFromId_ResolvesFrontAndBackByLineId()
    {
        var sim = Room();
        Assert.Equal(10, AcsLineSideFromId.SideFromId(sim, 9, 0));
        Assert.Equal(11, AcsLineSideFromId.SideFromId(sim, 9, 1));
        Assert.Equal(-1, AcsLineSideFromId.SideFromId(sim, 9, 2));
        sim.AcsTriggerLine = sim.Level.Lines[1];
        Assert.Equal(10, AcsLineSideFromId.SideFromId(sim, 0, 0));
    }

    [Fact]
    public void GetSideUdmfInt_UsesLineAndSideIds()
    {
        var sim = Room();
        var stack = new List<int> { 9, 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding(), ["offsetx"], new AcsGlobalStrings(),
            AcsCallFunctions.GetSideUdmfInt, 3, out var result));
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetLineUdmfInt_UsesLineFromId()
    {
        var sim = Room();
        sim.AcsTriggerLine = sim.Level.Lines[1];
        var stack = new List<int> { 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding(), ["arg0"], new AcsGlobalStrings(),
            AcsCallFunctions.GetLineUdmfInt, 2, out var result));
        Assert.Equal(7, result);
    }

    private static AuthoritySimulation Room()
    {
        var front = new LevelSide { Index = 10, Sector = 0, MidTextureOffsetX = 42 };
        var back = new LevelSide { Index = 11, Sector = 1 };
        var line = new LevelLine
        {
            Index = 1,
            Tag = 9,
            SideFront = 0,
            SideBack = 1,
            Arg0 = 7,
        };
        return AuthoritySimulation.Start(new PlayLevel
        {
            Lines = [new LevelLine { Index = 0 }, line],
            Sides = [front, back],
            Sectors = [new LevelSector(), new LevelSector { Index = 1 }],
            Things = [new LevelThing { Type = 1 }],
        });
    }
}
