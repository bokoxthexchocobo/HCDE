namespace HCDE.MapLoader;

/// <summary><c>MapLoader::GetPolySpots</c> from <c>maploader.cpp</c> (2877–2901).</summary>
public static class VmMapLoaderGetPolySpots
{
    public const int SpecialPolyAnchor = 1;
    public const int SpecialPolySpawn = 2;
    public const int SpecialPolySpawnCrush = 3;
    public const int SpecialPolySpawnHurt = 4;

    public static bool IsPolySpotSpecial(int special) =>
        special >= SpecialPolyAnchor && special <= SpecialPolySpawnHurt;

    public static bool IsPolyAnchor(int special) => special == SpecialPolyAnchor;

    public static int ToFixedCoord(double worldUnits) => (int)(worldUnits * 65536.0);

    public static void CollectPolyStart(
        IReadOnlyList<MapThingPolyCandidate> things,
        ICollection<PolyStart> anchors,
        ICollection<PolyStart> spots)
    {
        foreach (var thing in things)
        {
            if (thing.InfoType != null)
                continue;
            if (!IsPolySpotSpecial(thing.Special))
                continue;

            var vert = new PolyStart(
                ToFixedCoord(thing.PosX),
                ToFixedCoord(thing.PosY),
                thing.Angle);
            if (IsPolyAnchor(thing.Special))
                anchors.Add(vert);
            else
                spots.Add(vert);
        }
    }

    public readonly record struct MapThingPolyCandidate(
        string? InfoType,
        int Special,
        double PosX,
        double PosY,
        int Angle);

    public readonly record struct PolyStart(int X, int Y, int PolyNum);
}
