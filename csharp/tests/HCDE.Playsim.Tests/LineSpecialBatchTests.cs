using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineSpecialBatchTests
{
    [Fact]
    public void PlatDownWaitUpStayLowersThenReturns()
    {
        var sim = TwoSectorLiftRoom();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(62, 7, speed: 16, delay: 8), true));
        sim.Tick();
        Assert.True(sim.Floors[0] < 0);
        for (var i = 0; i < 200; i++) sim.Tick();
        Assert.InRange(sim.Floors[0], -0.5, 0.5);
    }

    [Fact]
    public void PlatStopRemovesActiveLift()
    {
        var sim = TwoSectorLiftRoom();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(62, 7, speed: 16, delay: 8), true));
        Assert.NotEmpty(sim.Motions);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(61, 7, speed: 0, delay: 2), true));
        Assert.DoesNotContain(sim.Motions, motion => motion.Kind == MotionKind.Lift);
    }

    [Fact]
    public void GenericFloorRaiseByValueMovesTaggedSector()
    {
        var sim = TwoSectorLiftRoom();
        var line = Trigger(200, 7, speed: 16, a2: 16, a3: 0, a4: 8);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.InRange(sim.Floors[0], 15.5, 16.5);
    }

    [Fact]
    public void ElevatorRaiseToNearestMatchesNeighborFloor()
    {
        var sim = TwoSectorLiftRoom(neighborFloor: 24);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(245, 7, speed: 16), true));
        for (var i = 0; i < 80; i++) sim.Tick();
        Assert.InRange(sim.Floors[0], 23.5, 24.5);
        Assert.InRange(sim.Ceilings[0], 151.5, 152.5);
    }

    [Fact]
    public void PillarBuildRaisesFloor()
    {
        var sim = TwoSectorLiftRoom();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Trigger(23, 7, speed: 16, a2: 12, a3: 0, a4: 0), true));
        for (var i = 0; i < 12; i++) sim.Tick();
        Assert.True(sim.Floors[0] >= 8);
    }

    private static AuthoritySimulation TwoSectorLiftRoom(double neighborFloor = -32)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors =
            [
                new LevelSector { Tag = 7, CeilingHeight = 128, FloorHeight = 0 },
                new LevelSector { Tag = 8, CeilingHeight = 128, FloorHeight = neighborFloor },
            ],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40, Y = 0 }],
        });
    }

    private static LevelLine Trigger(int special, int tag, int speed = 8, int delay = 0, int a2 = 0, int a3 = 0, int a4 = 0) =>
        new()
        {
            Special = special,
            Tag = tag,
            Arg0 = tag,
            Arg1 = speed,
            Arg2 = delay != 0 ? delay : a2,
            Arg3 = delay != 0 ? a2 : a3,
            Arg4 = a4,
            PlayerUse = true,
            Repeat = true,
            SideFront = 0,
        };
}
