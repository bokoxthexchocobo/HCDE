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

    public static bool MoveLinkedSectors(AuthoritySimulation sim, VmLinkedSectorPlane plane, int crush, double move, bool instant)
    {
        var ok = true;
        foreach (var link in plane.Sectors)
        {
            ok &= link.Type switch
            {
                VmLinkedSectorLinkType.Floor => VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, move, instant),
                VmLinkedSectorLinkType.Ceiling => VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, move, instant),
                VmLinkedSectorLinkType.Both => move < 0
                    ? VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, move, instant)
                      & VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, move, instant)
                    : VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, move, instant)
                      & VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, move, instant),
                VmLinkedSectorLinkType.FloorMirror => VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, -move, instant),
                VmLinkedSectorLinkType.CeilingMirror => VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, -move, instant),
                VmLinkedSectorLinkType.BothMirror => move > 0
                    ? VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, -move, instant)
                      & VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, -move, instant)
                    : VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, -move, instant)
                      & VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, -move, instant),
                VmLinkedSectorLinkType.Floor + VmLinkedSectorLinkType.CeilingMirror
                    => VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, move, instant)
                       & VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, -move, instant),
                VmLinkedSectorLinkType.Ceiling + VmLinkedSectorLinkType.FloorMirror
                    => VmLinkedSectorPlaneMove.MoveFloor(sim, link.SectorIndex, crush, -move, instant)
                       & VmLinkedSectorPlaneMove.MoveCeiling(sim, link.SectorIndex, crush, move, instant),
                _ => true,
            };
        }

        return ok;
    }
}
