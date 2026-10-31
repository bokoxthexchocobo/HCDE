namespace HCDE.Client;

/// <summary>
/// Polygon shade, secret-line probe, and subsector debug strokes from <c>am_map.cpp</c>
/// (<b>2251–2400</b>). Chunk <b>1136</b>. Earcut and the red poly-seg loop stay with the host.
/// </summary>
public static class VmAmMapPolyShade1136Slice
{
    public const int Hole = 8;
    public const int LineSecret = 0x20;
    public const int ColorMapCount = 32;

    public readonly record struct MapLine(double Ax, double Ay, double Bx, double By);

    public static (double X, double Y) ZeroOrigin()
    {
        return (0, 0);
    }

    public static bool ShouldDrawPolygon(bool textureValid)
    {
        if (textureValid)
            return true;
        return false;
    }

    public static bool NeedsHoleTriangulation(int subsectorFlags, int numLines)
    {
        if ((subsectorFlags & Hole) != 0 && numLines > 3)
            return true;
        return false;
    }

    public static List<(double X, double Y)> CopyHoleRing(IReadOnlyList<(double X, double Y)> points)
    {
        var ring = new List<(double X, double Y)>(points.Count);
        for (var i = 0; i < points.Count; i++)
            ring.Add(points[i]);
        return ring;
    }

    public static double FadeLevel(bool softLightRamp, int floorLight)
    {
        if (softLightRamp)
            return SoftFade(floorLight);
        return LinearFade(floorLight);
    }

    public static double SoftFade(int floorLight)
    {
        var map = (ColorMapCount * 2.0) - ((floorLight + 12) * (ColorMapCount / 128.0));
        var fade = (map - 12) / ColorMapCount;
        if (fade < 0)
            return 0;
        if (fade > 1)
            return 1;
        return fade;
    }

    public static double LinearFade(int floorLight)
    {
        var light = floorLight;
        if (light < 0)
            light = 0;
        if (light > 255)
            light = 255;
        return 1.0 - light / 255.0;
    }

    public static (double X, double Y) TextureScale(double mapScale, double scaleX, double scaleY)
    {
        return (mapScale / scaleX, mapScale / scaleY);
    }

    public static int CheckSecret(
        bool secretColorValid,
        bool hasFront,
        bool frontWasSecret,
        bool frontIsSecret,
        bool hasBack,
        bool backWasSecret,
        bool backIsSecret,
        int mapSecrets,
        int lineFlags)
    {
        if (!secretColorValid)
            return 0;
        var front = SecretOnSide(hasFront, frontWasSecret, frontIsSecret, mapSecrets, lineFlags);
        if (front != 0)
            return front;
        return SecretOnSide(hasBack, backWasSecret, backIsSecret, mapSecrets, lineFlags);
    }

    public static bool ShouldRotate(int rotateMode, bool viewActive)
    {
        if (rotateMode == 1)
            return true;
        if (rotateMode == 2 && viewActive)
            return true;
        return false;
    }

    public static MapLine OrientedSeg(
        double ax, double ay, double bx, double by, bool rotate, Func<double, double, (double X, double Y)> rotatePoint)
    {
        if (rotate)
        {
            var a = rotatePoint(ax, ay);
            var b = rotatePoint(bx, by);
            return new MapLine(a.X, a.Y, b.X, b.Y);
        }
        return new MapLine(ax, ay, bx, by);
    }

    public static bool ShowSubsector(int index, int count)
    {
        if (index >= 0 && (uint)index < (uint)count)
            return true;
        return false;
    }

    public static (byte R, byte G, byte B) SubsectorYellow()
    {
        return (255, 255, 0);
    }

    public static (byte R, byte G, byte B) PolyNodeRed()
    {
        return (255, 0, 0);
    }

    public static List<int> YellowSegIndexes(int numLines)
    {
        var indexes = new List<int>();
        for (var i = 0; i < numLines; i++)
            indexes.Add(i);
        return indexes;
    }

    public static bool PolyNodeBelongs(int nodeSubsector, int shownSubsector)
    {
        if (nodeSubsector == shownSubsector)
            return true;
        return false;
    }

    private static int SecretOnSide(bool present, bool wasSecret, bool isSecret, int mapSecrets, int lineFlags)
    {
        if (present && wasSecret)
        {
            if (mapSecrets != 0 && !isSecret)
                return 1;
            if (mapSecrets == 2 && (lineFlags & LineSecret) == 0)
                return 2;
        }
        return 0;
    }
}
