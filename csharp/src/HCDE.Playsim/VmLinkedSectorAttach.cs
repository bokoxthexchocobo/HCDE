namespace HCDE.Playsim;

/// <summary><c>AddSingleSector</c> / <c>P_AddSectorLinks</c> from <c>p_linkedsectors.cpp</c>.</summary>
public static class VmLinkedSectorAttach
{
    public static void AddSingleSector(VmLinkedSectorPlane plane, int sectorIndex, int moveType)
    {
        for (var i = 0; i < plane.Sectors.Count; i++)
        {
            if (plane.Sectors[i].SectorIndex != sectorIndex) continue;
            var merged = plane.Sectors[i].Type;
            if ((moveType & VmLinkedSectorLinkType.FloorBits) != 0)
                merged &= ~VmLinkedSectorLinkType.FloorBits;
            if ((moveType & VmLinkedSectorLinkType.CeilingBits) != 0)
                merged &= ~VmLinkedSectorLinkType.CeilingBits;
            merged |= moveType;
            plane.Sectors[i] = new VmLinkedSectorEntry { SectorIndex = sectorIndex, Type = merged };
            return;
        }

        plane.Sectors.Add(new VmLinkedSectorEntry { SectorIndex = sectorIndex, Type = moveType });
    }

    public static int SanitizeMoveType(int moveType)
    {
        moveType &= VmLinkedSectorLinkType.FlagMask;
        if ((moveType & VmLinkedSectorLinkType.FloorMirror) == VmLinkedSectorLinkType.FloorMirrorFlag)
            moveType &= ~VmLinkedSectorLinkType.FloorMirrorFlag;
        if ((moveType & VmLinkedSectorLinkType.CeilingMirror) == VmLinkedSectorLinkType.CeilingMirrorFlag)
            moveType &= ~VmLinkedSectorLinkType.CeilingMirrorFlag;
        return moveType;
    }

    public static void RemoveTaggedSectors(VmLinkedSectorPlane plane, int tag, Func<int, int, bool> sectorHasTag)
    {
        for (var i = plane.Sectors.Count - 1; i >= 0; i--)
        {
            if (sectorHasTag(plane.Sectors[i].SectorIndex, tag))
                plane.Sectors.RemoveAt(i);
        }
    }

    public static bool TryAddSectorLinks(VmLinkedSectorPlane plane, int controlSectorIndex, int tag, int rawParam, bool ceiling,
        bool controlCeilingMoving, bool controlFloorMoving, Func<int, int, bool> sectorHasTag, IReadOnlyList<int> sectorsWithTag)
    {
        if ((ceiling && controlCeilingMoving) || (!ceiling && controlFloorMoving))
            return false;

        var moveType = SanitizeMoveType(rawParam);
        if (rawParam != 0 && moveType == 0)
            return false;

        if (moveType > 0)
        {
            foreach (var sec in sectorsWithTag)
            {
                if (sec == controlSectorIndex)
                {
                    if (ceiling && (moveType & VmLinkedSectorLinkType.Floor) != 0)
                        continue;
                    if (!ceiling && (moveType & VmLinkedSectorLinkType.Ceiling) != 0)
                        continue;
                }

                AddSingleSector(plane, sec, moveType);
            }
        }
        else
        {
            RemoveTaggedSectors(plane, tag, sectorHasTag);
        }

        return true;
    }
}
