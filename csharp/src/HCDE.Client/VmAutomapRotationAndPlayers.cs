namespace HCDE.Client;

/// <summary>
/// <c>DAutomap::rotate</c>, <c>rotatePoint</c>, <c>drawLineCharacter</c>, and <c>drawPlayers</c> policy from <c>am_map.cpp</c> (2794–2983).
/// </summary>
public static class VmAutomapRotationAndPlayers
{
    public readonly struct RotationCache
    {
        public RotationCache(double angleRadians, double sin, double cos)
        {
            AngleRadians = angleRadians;
            Sin = sin;
            Cos = cos;
        }

        public double AngleRadians { get; }
        public double Sin { get; }
        public double Cos { get; }
    }

    public static RotationCache UpdateRotationCache(RotationCache? previous, double angleRadians)
    {
        if (previous is { } p && Math.Abs(p.AngleRadians - angleRadians) < 1e-9)
            return p;
        return new RotationCache(angleRadians, Math.Sin(angleRadians), Math.Cos(angleRadians));
    }

    public static (double x, double y) Rotate(double x, double y, RotationCache cache)
    {
        var tmpx = x * cache.Cos - y * cache.Sin;
        var y2 = x * cache.Sin + y * cache.Cos;
        return (tmpx, y2);
    }

    public static (double x, double y) RotateAboutPivot(
        double x,
        double y,
        double pivotX,
        double pivotY,
        double yawRadians,
        double viewYawRadians)
    {
        x -= pivotX;
        y -= pivotY;
        var cache = UpdateRotationCache(null, -viewYawRadians + Math.PI / 2);
        var rotated = Rotate(x, y, cache);
        return (rotated.x + pivotX, rotated.y + pivotY);
    }

    public readonly struct LineGuyVertex
    {
        public LineGuyVertex(double ax, double ay, double bx, double by)
        {
            Ax = ax;
            Ay = ay;
            Bx = bx;
            By = by;
        }

        public double Ax { get; }
        public double Ay { get; }
        public double Bx { get; }
        public double By { get; }
    }

    public static IEnumerable<(double ax, double ay, double bx, double by)> TransformLineCharacter(
        IReadOnlyList<LineGuyVertex> lineGuy,
        double scale,
        double angleRadians,
        double originX,
        double originY,
        RotationCache? angleCache)
    {
        RotationCache? cache = null;
        if (angleRadians != 0)
            cache = UpdateRotationCache(angleCache, angleRadians);

        for (var i = 0; i < lineGuy.Count; i++)
        {
            var seg = lineGuy[i];
            var ax = seg.Ax;
            var ay = seg.Ay;
            var bx = seg.Bx;
            var by = seg.By;

            if (scale != 0)
            {
                ax *= scale;
                ay *= scale;
                bx *= scale;
                by *= scale;
            }

            if (cache is { } rot)
            {
                (ax, ay) = Rotate(ax, ay, rot);
                (bx, by) = Rotate(bx, by, rot);
            }

            yield return (ax + originX, ay + originY, bx + originX, by + originY);
        }
    }

    public static bool ShouldDeferSinglePlayerArrowToThingSprites(int amCheat, int amShowThingSprites) =>
        amCheat >= 2 && amCheat != 4 && amShowThingSprites > 0;

    public static double ResolveSoloArrowAngleRadians(bool mapRotated, double playerYawRadians) =>
        mapRotated ? Math.PI / 2 : playerYawRadians;

    public static bool ShouldSkipPlayerIcon(
        bool inGame,
        bool hasMobj,
        int playerIndex,
        int consolePlayer,
        int dmflags2,
        bool deathmatch,
        bool demoPlayback,
        bool isTeammate,
        bool isCameraPlayer)
    {
        if (!inGame || !hasMobj)
            return true;
        if ((dmflags2 & Df2NoAutomapAllies) != 0 && playerIndex != consolePlayer)
            return true;
        if (deathmatch && !demoPlayback && !isTeammate && !isCameraPlayer)
            return true;
        return false;
    }

    public const int Df2NoAutomapAllies = 1 << 6;

    public static (byte r, byte g, byte b) ResolvePlayerColorFromHsv(float h, float s, float v)
    {
        HsvToRgb(h, s, v, out var r, out var g, out var b);
        return (
            (byte)Math.Clamp((int)(r * 255f), 0, 255),
            (byte)Math.Clamp((int)(g * 255f), 0, 255),
            (byte)Math.Clamp((int)(b * 255f), 0, 255));
    }

    public static bool UseFadedPlayerColor(double alpha) => alpha < 1.0;

    public static double AdjustMultiplayerArrowAngle(
        double mobjYawRadians,
        bool mapRotated,
        double cameraYawRadians) =>
        mapRotated ? mobjYawRadians - cameraYawRadians + Math.PI / 2 : mobjYawRadians;

    private static void HsvToRgb(float h, float s, float v, out float r, out float g, out float b)
    {
        if (s <= 0)
        {
            r = g = b = v;
            return;
        }

        h = (h % 360f + 360f) % 360f;
        var hi = (int)(h / 60f) % 6;
        var f = h / 60f - hi;
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);
        switch (hi)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }
    }
}
