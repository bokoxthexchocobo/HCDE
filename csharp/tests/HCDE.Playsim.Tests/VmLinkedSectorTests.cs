using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmLinkedSectorTests
{
    [Fact]
    public void IsLinked_MatchesFloorOrCeilingFlag()
    {
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 3, Type = VmLinkedSectorLinkType.Floor });
        Assert.True(VmLinkedSectorMovement.IsLinked(plane, 3, ceiling: false));
        Assert.False(VmLinkedSectorMovement.IsLinked(plane, 3, ceiling: true));
    }

    [Fact]
    public void MoveLinkedSectors_AppliesMirrorAndBothOrdering()
    {
        var sim = TwoSectorRoom();
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 1, Type = VmLinkedSectorLinkType.FloorMirror });
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 0, Type = VmLinkedSectorLinkType.Both });
        var floor0 = sim.FloorOf(0);
        var ceil0 = sim.CeilingOf(0);
        var floor1 = sim.FloorOf(1);
        Assert.True(VmLinkedSectorMovement.MoveLinkedSectors(sim, plane, crush: 0, move: 8, instant: false));
        Assert.Equal(floor0 + 8, sim.FloorOf(0));
        Assert.Equal(ceil0 + 8, sim.CeilingOf(0));
        Assert.Equal(floor1 - 8, sim.FloorOf(1));
    }

    [Fact]
    public void MoveLinkedSectors_RunsAllLinksEvenWhenOneFails()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors =
            [
                new LevelSector { FloorHeight = 100, CeilingHeight = 110 },
                new LevelSector { FloorHeight = 0, CeilingHeight = 128 },
            ],
        });
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 0, Type = VmLinkedSectorLinkType.Floor });
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 1, Type = VmLinkedSectorLinkType.Floor });
        var floor1Before = sim.FloorOf(1);
        Assert.False(VmLinkedSectorMovement.MoveLinkedSectors(sim, plane, crush: -1, move: 20, instant: false));
        Assert.Equal(floor1Before + 20, sim.FloorOf(1));
    }

    [Fact]
    public void MoveFloor_BlocksWhenSolidActorAndNoCrush()
    {
        var sim = TwoSectorRoom();
        sim.AddBot(0, 0);
        var bot = sim.Actors.Last();
        bot.SectorIndex = 1;
        Assert.False(VmLinkedSectorPlaneMove.MoveFloor(sim, 1, crush: 0, move: 4, instant: false));
    }

    [Fact]
    public void MoveFloor_RejectsFloorAboveCeiling()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors =
            [
                new LevelSector { FloorHeight = 100, CeilingHeight = 120 },
            ],
        });
        Assert.False(VmLinkedSectorPlaneMove.MoveFloor(sim, 0, crush: -1, move: 30, instant: false));
    }

    [Fact]
    public void StartLinkedSectorInterpolations_RegistersFloorAndCeilingPlanes()
    {
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 2, Type = VmLinkedSectorLinkType.Both });
        var list = new List<VmLinkedSectorInterpolation.Marker>();
        VmLinkedSectorInterpolation.StartLinkedSectorInterpolations(list, plane);
        Assert.Contains(new VmLinkedSectorInterpolation.Marker(2, false), list);
        Assert.Contains(new VmLinkedSectorInterpolation.Marker(2, true), list);
    }

    [Fact]
    public void AddSectorLinks_MergesDuplicateSectorTypes()
    {
        var plane = new VmLinkedSectorPlane();
        VmLinkedSectorAttach.AddSingleSector(plane, 5, VmLinkedSectorLinkType.Floor);
        VmLinkedSectorAttach.AddSingleSector(plane, 5, VmLinkedSectorLinkType.Ceiling);
        var entry = Assert.Single(plane.Sectors);
        Assert.Equal(VmLinkedSectorLinkType.Both, entry.Type);
    }

    [Fact]
    public void TryAddSectorLinks_SkipsSelfPlaneConflict()
    {
        var plane = new VmLinkedSectorPlane();
        Assert.True(VmLinkedSectorAttach.TryAddSectorLinks(plane, 0, 1, VmLinkedSectorLinkType.Floor, ceiling: true,
            controlCeilingMoving: false, controlFloorMoving: false, (_, t) => t == 1, [0, 2]));
        Assert.DoesNotContain(plane.Sectors, s => s.SectorIndex == 0);
        Assert.Contains(plane.Sectors, s => s.SectorIndex == 2);
    }

    [Fact]
    public void TryAddSectorLinks_RemovesTaggedSectorsWhenParamZero()
    {
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 4, Type = VmLinkedSectorLinkType.Floor });
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 5, Type = VmLinkedSectorLinkType.Ceiling });
        Assert.True(VmLinkedSectorAttach.TryAddSectorLinks(plane, 0, 7, 0, ceiling: false,
            controlCeilingMoving: false, controlFloorMoving: false, (sec, tag) => sec == 4 && tag == 7, []));
        Assert.DoesNotContain(plane.Sectors, s => s.SectorIndex == 4);
        Assert.Contains(plane.Sectors, s => s.SectorIndex == 5);
    }

    [Fact]
    public void AddSectorLinksById_AttachesFromStaticInitLines()
    {
        var sides = new[]
        {
            new LevelSide { Sector = 1 },
            new LevelSide { Sector = 0 },
        };
        var lines = new[]
        {
            new LevelLine
            {
                SideFront = 0,
                Special = VmLinkedSectorAttach.StaticInitSpecial,
                Arg1 = VmLinkedSectorAttach.InitSectorLink,
                Arg3 = VmLinkedSectorLinkType.Floor,
            },
        };
        var plane = new VmLinkedSectorPlane();
        VmLinkedSectorAttach.AddSectorLinksById(plane, 0, ceiling: false, [0], lines, sides);
        var entry = Assert.Single(plane.Sectors);
        Assert.Equal(1, entry.SectorIndex);
        Assert.Equal(VmLinkedSectorLinkType.Floor, entry.Type);
    }

    static AuthoritySimulation TwoSectorRoom() =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors =
            [
                new LevelSector { FloorHeight = 0, CeilingHeight = 128 },
                new LevelSector { FloorHeight = 0, CeilingHeight = 128 },
            ],
        });
}
