namespace HCDE.Client;

/// <summary>
/// Subsector point projection, textured-floor setup, 3D-floor selection, unseen tint,
/// and the sector-fill fallback from <c>am_map.cpp</c> (<b>2101–2250</b>). Chunk <b>1135</b>.
/// </summary>
public static class VmAmMapSubsectorFill1135Slice
{
    public const int FloorPlane = 0;
    public const int CeilingPlane = 1;
    public const int SubsectorDrawn = 2;
    public const int FloorExists = 0x1;
    public const int FloorRenderPlanes = 0x8;
    public const int FloorFog = 0x1000;
    public const int FloorInvertPlanes = 0x2000;
    public const int FloorThisInside = 0x40000000;
    public const int LevelHasFadeTable = 0x8;
    public const uint FadeGray = 0x00808080;

    public readonly record struct Rover(int Flags, int Alpha, double TopZ);

    public static bool UseTexturedFloor(bool textured, bool viewActive)
    {
        if (textured && !viewActive)
            return true;
        return false;
    }

    public static bool ShouldRotate(int rotateMode, bool viewActive)
    {
        if (rotateMode == 1)
            return true;
        if (rotateMode == 2 && viewActive)
            return true;
        return false;
    }

    public static (float X, float Y) Project(
        double x, double y, double mapX, double mapY, double scale, int frameX, int frameY, int frameHeight)
    {
        var sx = (float)(frameX + (x - mapX) * scale);
        var sy = (float)(frameY + (frameHeight - (y - mapY) * scale));
        return (sx, sy);
    }

    public static (double X, double Y) SamplePoint(bool cameraSector, double viewX, double viewY, double centerX, double centerY)
    {
        if (cameraSector)
            return (viewX, viewY);
        return (centerX, centerY);
    }

    public static int SelectRover(IReadOnlyList<Rover> rovers, double floorZ, double ceilingZ, double compareZ)
    {
        for (var i = 0; i < rovers.Count; i++)
        {
            var rover = rovers[i];
            if ((rover.Flags & FloorExists) == 0)
                continue;
            if ((rover.Flags & (FloorFog | FloorThisInside)) != 0)
                continue;
            if ((rover.Flags & FloorRenderPlanes) == 0)
                continue;
            if (rover.Alpha == 0)
                continue;
            if (rover.TopZ > ceilingZ)
                continue;
            if (rover.TopZ < floorZ)
                return -1;
            if (rover.TopZ < compareZ)
                return i;
        }

        return -1;
    }

    public static int PlaneSelector(int flags)
    {
        if ((flags & FloorInvertPlanes) != 0)
            return FloorPlane;
        return CeilingPlane;
    }

    public static bool ShouldSkipSky(bool textureIsSky)
    {
        if (textureIsSky)
            return true;
        return false;
    }

    public static bool ShouldTintUnseen(int subsectorFlags, int cheat)
    {
        if ((subsectorFlags & SubsectorDrawn) == 0 && cheat < 4)
            return true;
        return false;
    }

    public static (byte R, byte G, byte B, byte Desaturation) TintUnseen(byte r, byte g, byte b, byte desaturation)
    {
        return (
            (byte)((r + 255) / 2),
            (byte)((g + 200) / 2),
            (byte)((b + 160) / 2),
            (byte)(255 - (255 - desaturation) / 4));
    }

    public static uint FadeColor(int levelFlags)
    {
        if ((levelFlags & LevelHasFadeTable) != 0)
            return FadeGray;
        return 0;
    }

    public static double AutomapTextureAngle(double floorAngleDegrees, double cameraYawDegrees)
    {
        return floorAngleDegrees + 90 - cameraYawDegrees;
    }

    public readonly record struct SolidFill(double Alpha, int FloorLight, double ScaleX, double ScaleY, double OriginX, double OriginY);

    public static SolidFill SectorFill(double fillAlpha)
    {
        return new SolidFill(fillAlpha, 255, 1, 1, 0, 0);
    }
}
