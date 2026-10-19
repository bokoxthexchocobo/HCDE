using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineSpecialClosureTests
{
    [Fact]
    public void SectorFrictionSpecialUpdatesTaggedSector()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            Trigger(219, 7, arg1: 2, arg2: 50), true));
        Assert.InRange(sim.Level.Sectors[0].Friction, 2.49, 2.51);
    }

    [Fact]
    public void FloorAndCeilingLowerByValueMovesBothPlanes()
    {
        var sim = Room();
        sim.Floors[0] = 32;
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(95, 7, arg1: 16, arg2: 8), true));
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.True(sim.Floors[0] < 32);
        Assert.True(sim.Ceilings[0] < 128);
    }

    [Fact]
    public void TranslucentLineSetsAlpha()
    {
        var sim = Room();
        var line = Trigger(208, 0, arg0: 128);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.InRange(line.Alpha, 0.49, 0.51);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Namespace = "ZDoom",
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Lines = [new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0 }],
        Sides = [new LevelSide { Sector = 0 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static LevelLine Trigger(int special, int tag, int arg0 = -1, int arg1 = 8, int arg2 = 0) => new()
    {
        Special = special,
        Tag = tag,
        Arg0 = arg0 >= 0 ? arg0 : tag,
        Arg1 = arg1,
        Arg2 = arg2,
        PlayerUse = true,
        Repeat = true,
        SideFront = 0,
    };
}
