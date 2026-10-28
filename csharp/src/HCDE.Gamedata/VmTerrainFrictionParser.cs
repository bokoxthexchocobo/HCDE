namespace HCDE.Gamedata;

/// <summary><c>ParseFriction</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainFrictionParser
{
    private const double OrigFriction = 0.90625; // ORIG_FRICTION

    public static void ApplyFrictionInput(double input, out double friction, out double moveFactor)
    {
        var raw = (0x1EB8 * (input * 100)) / 0x80 + 0xD001;
        raw = Math.Clamp(raw, 0, 65536);
        if (raw > OrigFriction * 65536)
            moveFactor = ((0x10092 - raw) * 1024) / 4352 + 568;
        else
            moveFactor = ((raw - 0xDB34) * 0xA) / 0x80;
        if (moveFactor < 32)
            moveFactor = 32;
        friction = raw / 65536.0;
        moveFactor /= 65536.0;
    }
}
