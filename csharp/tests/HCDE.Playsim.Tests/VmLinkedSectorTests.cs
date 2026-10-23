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
        var plane = new VmLinkedSectorPlane();
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 1, Type = VmLinkedSectorLinkType.FloorMirror });
        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = 2, Type = VmLinkedSectorLinkType.Both });
        var log = new List<string>();
        var ok = VmLinkedSectorMovement.MoveLinkedSectors(plane, 8,
            (s, m) => { log.Add($"F{s}:{m}"); return true; },
            (s, m) => { log.Add($"C{s}:{m}"); return true; });
        Assert.True(ok);
        Assert.Contains("F1:-8", log);
        Assert.Contains("C2:8", log);
        Assert.Contains("F2:8", log);
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
}
