namespace HCDE.Client;

/// <summary>
/// Glyph completion and player arrows from <c>am_map.cpp</c> (<b>2851–3000</b>). Chunk <b>1140</b>.
/// Key filtering begins at <b>3002</b>.
/// </summary>
public static class VmAmMapPlayerArrow1140Slice
{
    public const int NoAutomapAllies = 1 << 19;

    public readonly record struct RemotePlayer(
        bool InGame,
        bool HasActor,
        bool Teammate,
        bool CameraPlayer,
        double Alpha,
        float Hue,
        float Saturation,
        float Value,
        double X,
        double Y,
        double OffsetX,
        double OffsetY,
        double YawDegrees);

    public readonly record struct RemoteDraw(
        bool Draw,
        bool AlmostBackground,
        byte R,
        byte G,
        byte B,
        double X,
        double Y,
        double AngleDegrees);

    public static (double Ax, double Ay, double Bx, double By) FinishGlyph(
        double ax, double ay, double bx, double by,
        double scale, bool angled, VmAmMapWallColor1139Slice.Spin spin,
        double originX, double originY)
    {
        var a = Place(ax, ay, scale, angled, spin, originX, originY);
        var b = Place(bx, by, scale, angled, spin, originX, originY);
        return (a.X, a.Y, b.X, b.Y);
    }

    public static bool PreferThingSprites(int cheat, int showThingSprites)
    {
        if (cheat >= 2 && cheat != 4 && showThingSprites > 0)
            return true;
        return false;
    }

    public static (double X, double Y, double AngleDegrees, bool CheatArrow, double Scale) SinglePlayerMark(
        int cheat,
        int cheatArrowLines,
        int rotateMode,
        bool viewActive,
        double x,
        double y,
        double yawDegrees,
        Func<double, double, (double X, double Y)> rotatePoint)
    {
        var rotate = rotateMode == 1 || (rotateMode == 2 && viewActive);
        var angle = yawDegrees;
        if (rotate)
        {
            var turned = rotatePoint(x, y);
            x = turned.X;
            y = turned.Y;
            angle = 90;
        }
        var cheatArrow = cheat != 0 && cheatArrowLines > 0;
        return (x, y, angle, cheatArrow, 0);
    }

    public static RemoteDraw RemoteMark(
        int index,
        int consolePlayer,
        int dmFlags2,
        bool deathmatch,
        bool demoPlayback,
        bool rotate,
        double cameraYawDegrees,
        RemotePlayer player,
        Func<double, double, (double X, double Y)> rotatePoint)
    {
        if (!player.InGame || !player.HasActor)
            return Hidden();
        if ((dmFlags2 & NoAutomapAllies) != 0 && index != consolePlayer)
            return Hidden();
        if (deathmatch && !demoPlayback && !player.Teammate && !player.CameraPlayer)
            return Hidden();

        var x = player.X + player.OffsetX;
        var y = player.Y + player.OffsetY;
        var angle = player.YawDegrees;
        if (rotate)
        {
            var turned = rotatePoint(x, y);
            x = turned.X;
            y = turned.Y;
            angle = player.YawDegrees - (cameraYawDegrees - 90);
        }

        if (player.Alpha < 1)
            return new RemoteDraw(true, true, 0, 0, 0, x, y, angle);

        var rgb = HsvToRgb(player.Hue, player.Saturation, player.Value);
        return new RemoteDraw(true, false, ScaleByte(rgb.R), ScaleByte(rgb.G), ScaleByte(rgb.B), x, y, angle);
    }

    public static (float R, float G, float B) HsvToRgb(float h, float s, float v)
    {
        if (s == 0)
            return (v, v, v);
        h /= 60f;
        var sector = (int)Math.Floor(h);
        var fraction = h - sector;
        var p = v * (1f - s);
        var q = v * (1f - s * fraction);
        var t = v * (1f - s * (1f - fraction));
        if (sector == 0)
            return (v, t, p);
        if (sector == 1)
            return (q, v, p);
        if (sector == 2)
            return (p, v, t);
        if (sector == 3)
            return (p, q, v);
        if (sector == 4)
            return (t, p, v);
        return (v, p, q);
    }

    public static byte ScaleByte(float channel)
    {
        var scaled = (int)(channel * 255f);
        if (scaled < 0)
            return 0;
        if (scaled > 255)
            return 255;
        return (byte)scaled;
    }

    private static (double X, double Y) Place(
        double x, double y, double scale, bool angled, VmAmMapWallColor1139Slice.Spin spin, double originX, double originY)
    {
        var scaled = VmAmMapWallColor1139Slice.ScaleGlyphPoint(x, y, scale);
        if (angled)
        {
            var spun = VmAmMapWallColor1139Slice.ApplySpin(scaled.X, scaled.Y, spin);
            scaled = (spun.X, spun.Y);
        }
        return (scaled.X + originX, scaled.Y + originY);
    }

    private static RemoteDraw Hidden()
    {
        return new RemoteDraw(false, false, 0, 0, 0, 0, 0, 0);
    }
}
