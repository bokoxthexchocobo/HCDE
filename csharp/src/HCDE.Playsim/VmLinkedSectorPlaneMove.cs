namespace HCDE.Playsim;

/// <summary><c>MoveFloor</c> / <c>MoveCeiling</c> helpers from <c>p_linkedsectors.cpp</c>.</summary>
public static class VmLinkedSectorPlaneMove
{
    public static bool MoveFloor(AuthoritySimulation sim, int sectorIndex, int crush, double move, bool instant)
    {
        sim.ShiftFloorPlane(sectorIndex, move);
        if (LinkedSectorChangeSector(sim, sectorIndex, crush, move, floor: true, instant))
            return false;

        if (!PlaneCrossingAllowed(sim, sectorIndex, movedCeiling: false))
            return false;

        return true;
    }

    public static bool MoveCeiling(AuthoritySimulation sim, int sectorIndex, int crush, double move, bool instant)
    {
        sim.ShiftCeilingPlane(sectorIndex, move);
        if (LinkedSectorChangeSector(sim, sectorIndex, crush, move, floor: false, instant))
            return false;

        if (!PlaneCrossingAllowed(sim, sectorIndex, movedCeiling: true))
            return false;

        return true;
    }

    /// <summary>Native <c>P_ChangeSector</c> returning <c>true</c> when the move must abort.</summary>
    static bool LinkedSectorChangeSector(AuthoritySimulation sim, int sectorIndex, int crush, double move, bool floor, bool instant)
    {
        if (crush < 0)
            return false;

        foreach (var actor in sim.ActorsInSector(sectorIndex))
        {
            if (!actor.Solid || actor.IsDead || actor.Destroyed)
                continue;
            if (crush == 0)
                return true;
        }

        return false;
    }

    static bool PlaneCrossingAllowed(AuthoritySimulation sim, int sectorIndex, bool movedCeiling)
    {
        if (sim.SectorHasSlopedPlane(sectorIndex))
            return true;
        if (movedCeiling && sim.PortalIsLinked(sectorIndex, ceiling: false))
            return true;
        if (!movedCeiling && sim.PortalIsLinked(sectorIndex, ceiling: true))
            return true;

        return sim.FloorOf(sectorIndex) <= sim.CeilingOf(sectorIndex);
    }
}
