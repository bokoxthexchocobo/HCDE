namespace HCDE.Gamedata;

/// <summary><c>skyrotate</c> / <c>skyrotate2</c> vector assignment from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoSkyRotateParser
{
    public static bool TryParseVector(IReadOnlyList<string> tokens, ref int index, out VmVec3 vector)
    {
        vector = new VmVec3();
        if (index + 2 >= tokens.Count) return false;
        if (!double.TryParse(tokens[index], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var x)) return false;
        if (!double.TryParse(tokens[index + 1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var y)) return false;
        if (!double.TryParse(tokens[index + 2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var z)) return false;
        vector = VmVec3.FromDoubles(x, y, z).Normalized();
        index += 3;
        return true;
    }
}

public readonly struct VmVec3
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public static VmVec3 FromDoubles(double x, double y, double z) =>
        new() { X = (float)x, Y = (float)y, Z = (float)z };

    public VmVec3 Normalized()
    {
        var len = MathF.Sqrt(X * X + Y * Y + Z * Z);
        if (len <= 0) return this;
        return new VmVec3 { X = X / len, Y = Y / len, Z = Z / len };
    }
}
