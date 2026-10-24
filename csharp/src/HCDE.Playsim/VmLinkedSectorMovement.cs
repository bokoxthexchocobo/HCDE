namespace HCDE.Playsim;

/// <summary><c>sector_t::IsLinked</c> and <c>P_MoveLinkedSectors</c> from <c>p_linkedsectors.cpp</c>.</summary>
public static class VmLinkedSectorMovement
{
    public static bool IsLinked(VmLinkedSectorPlane plane, int otherSectorIndex, bool ceiling)
    {
        var flag = ceiling ? VmLinkedSectorLinkType.Ceiling : VmLinkedSectorLinkType.Floor;
        foreach (var link in plane.Sectors)
        {
            if (link.SectorIndex == otherSectorIndex && (link.Type & flag) != 0)
                return true;
        }

        return false;
    }

    public delegate bool SectorPlaneMove(int sectorIndex, double move);

    public static bool MoveLinkedSectors(VmLinkedSectorPlane plane, double move, SectorPlaneMove moveFloor, SectorPlaneMove moveCeiling)
    {
        var ok = true;
        foreach (var link in plane.Sectors)
        {
            ok &= link.Type switch
            {
                VmLinkedSectorLinkType.Floor => moveFloor(link.SectorIndex, move),
                VmLinkedSectorLinkType.Ceiling => moveCeiling(link.SectorIndex, move),
                VmLinkedSectorLinkType.Both => move < 0
                    ? moveFloor(link.SectorIndex, move) & moveCeiling(link.SectorIndex, move)
                    : moveCeiling(link.SectorIndex, move) & moveFloor(link.SectorIndex, move),
                VmLinkedSectorLinkType.FloorMirror => moveFloor(link.SectorIndex, -move),
                VmLinkedSectorLinkType.CeilingMirror => moveCeiling(link.SectorIndex, -move),
                VmLinkedSectorLinkType.BothMirror => move > 0
                    ? moveFloor(link.SectorIndex, -move) & moveCeiling(link.SectorIndex, -move)
                    : moveCeiling(link.SectorIndex, -move) & moveFloor(link.SectorIndex, -move),
                VmLinkedSectorLinkType.Floor + VmLinkedSectorLinkType.CeilingMirror
                    => moveFloor(link.SectorIndex, move) & moveCeiling(link.SectorIndex, -move),
                VmLinkedSectorLinkType.Ceiling + VmLinkedSectorLinkType.FloorMirror
                    => moveFloor(link.SectorIndex, -move) & moveCeiling(link.SectorIndex, move),
                _ => true,
            };
        }

        return ok;
    }
}
