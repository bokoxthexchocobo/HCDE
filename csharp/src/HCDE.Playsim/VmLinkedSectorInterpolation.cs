namespace HCDE.Playsim;

/// <summary><c>P_StartLinkedSectorInterpolations</c> from <c>p_linkedsectors.cpp</c>.</summary>
public static class VmLinkedSectorInterpolation
{
    public readonly record struct Marker(int SectorIndex, bool Ceiling);

    public static void StartLinkedSectorInterpolations(List<Marker> list, VmLinkedSectorPlane plane)
    {
        foreach (var link in plane.Sectors)
        {
            if ((link.Type & VmLinkedSectorLinkType.Floor) != 0)
                list.Add(new Marker(link.SectorIndex, Ceiling: false));
            if ((link.Type & VmLinkedSectorLinkType.Ceiling) != 0)
                list.Add(new Marker(link.SectorIndex, Ceiling: true));
        }
    }
}
