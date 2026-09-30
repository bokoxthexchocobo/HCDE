using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native Hexen sector specials 201–224 carry for players. Raven compat speeds are absent.</summary>
public static class HexenSectorScroll
{
    public const int NorthSlow = 201;
    public const int SouthWestFast = 224;

    private static readonly (sbyte X, sbyte Y)[] Scrollies =
    [
        (0, 1), (0, 2), (0, 4),
        (-1, 0), (-2, 0), (-4, 0),
        (0, -1), (0, -2), (0, -4),
        (1, 0), (2, 0), (4, 0),
        (1, 1), (2, 2), (4, 4),
        (-1, 1), (-2, 2), (-4, 4),
        (-1, -1), (-2, -2), (-4, -4),
        (1, -1), (2, -2), (4, -4),
    ];

    public static bool TryGetPlayerCarryDelta(short special, out double dx, out double dy)
    {
        if (special < NorthSlow || special > SouthWestFast)
        {
            dx = dy = 0;
            return false;
        }
        var entry = Scrollies[special - NorthSlow];
        dx = -entry.X * 0.5;
        dy = entry.Y * 0.5;
        return true;
    }
}
