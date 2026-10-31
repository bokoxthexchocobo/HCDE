namespace HCDE.Client;

/// <summary>
/// Key marks and thing triangles from <c>am_map.cpp</c> (<b>3001–3150</b>). Chunk <b>1141</b>.
/// The cheat bounding box begins at <b>3153</b>.
/// </summary>
public static class VmAmMapKeysThings1141Slice
{
    public const int Special = 0x1;
    public const int Corpse = 0x00100000;
    public const int CountKill = 0x00400000;
    public const int CountItem = 0x00800000;
    public const int Friendly = 0x08000000;
    public const int IsMonster = 0x2000;
    public const int NotOnAutomap = 0x40000000;
    public const int Invisible = 0x8000;
    public const double TriangleScale = 16;

    public enum ThingColor
    {
        Thing,
        Friend,
        NocountMonster,
        Monster,
        CountItem,
        Item,
        KeyFromMap,
        Suppressed
    }

    public readonly record struct ThingMark(ThingColor Color, bool CheatKey, byte R, byte G, byte B, bool Triangle, double Scale);

    public static bool AcceptKey(bool dehackedPickup, bool replaceeIsKey, bool classIsKey)
    {
        if (dehackedPickup)
            return replaceeIsKey;
        if (classIsKey)
            return true;
        return false;
    }

    public static double KeyAngle(double yawDegrees, bool rotate, double cameraYawDegrees)
    {
        if (rotate)
            return yawDegrees + (-cameraYawDegrees + 90);
        return yawDegrees;
    }

    public static (bool Draw, bool FromMap, byte R, byte G, byte B, double Scale) KeyMark(int flags, int mapColor)
    {
        if ((flags & Special) == 0)
            return (false, false, 0, 0, 0, 0);
        if (mapColor >= 0)
        {
            return (true, true,
                (byte)((mapColor >> 16) & 0xff),
                (byte)((mapColor >> 8) & 0xff),
                (byte)(mapColor & 0xff),
                0);
        }
        return (true, false, 0, 0, 0, 0);
    }

    public static bool ShowThing(int cheat, int flags6, bool thingRenderStyles, int renderFlags)
    {
        if (cheat > 0)
            return true;
        if ((flags6 & NotOnAutomap) == 0)
            return true;
        if (thingRenderStyles && (renderFlags & Invisible) == 0 && (flags6 & NotOnAutomap) == 0)
            return true;
        return false;
    }

    public static (double X, double Y) ThingPoint(
        double x, double y, double portalX, double portalY, double automapX, double automapY)
    {
        var sx = (float)x + (float)portalX + (float)automapX;
        var sy = (float)y + (float)portalY + (float)automapY;
        return (sx, sy);
    }

    public static List<int> SpritePasses(int showSprites)
    {
        var passes = new List<int>();
        for (var show = showSprites; show > 0; show--)
            passes.Add(show);
        return passes;
    }

    public static int SpriteFrameBase(int spriteFrames, int show, int actorFrame)
    {
        if (show > 1)
            return spriteFrames + actorFrame;
        return spriteFrames;
    }

    public static double SpriteAngle(
        double yawDegrees, double spriteRotationDegrees, bool texturesDiffer, bool rotate, double cameraYawDegrees)
    {
        var angle = 270 - yawDegrees - spriteRotationDegrees;
        if (texturesDiffer)
            angle += 180.0 / 16.0;
        if (rotate)
            angle += cameraYawDegrees - 90;
        return angle;
    }

    public static int RotationIndex(double normalizedDegrees)
    {
        return (int)(normalizedDegrees * (16.0 / 360.0));
    }

    public static int TextureSlot(int show, int rotation)
    {
        if (show > 2)
            return rotation;
        return 0;
    }

    public static bool FlipRotation(int flipBits, int rotation)
    {
        if ((flipBits & (1 << rotation)) != 0)
            return true;
        return false;
    }

    public static (double X, double Y) SpriteScale(double scaleX, double scaleY, double scaleMapToFrame)
    {
        var factor = 10.0 / 16.0 * scaleMapToFrame;
        return (scaleX * factor, scaleY * factor);
    }

    public static ThingMark ClassifyThing(
        int flags, int flags3, bool isKey, bool easyKey, bool showKeysAlways, bool showKeys, int keyMapColor)
    {
        if ((flags3 & IsMonster) != 0 && (flags & Corpse) == 0)
        {
            if ((flags & Friendly) != 0)
                return Mark(ThingColor.Friend);
            if ((flags & CountKill) == 0)
                return Mark(ThingColor.NocountMonster);
            return Mark(ThingColor.Monster);
        }
        if ((flags & Special) != 0)
        {
            if (isKey)
            {
                if (easyKey || showKeysAlways)
                    return Mark(ThingColor.Suppressed);
                if (showKeys)
                    return KeyThing(keyMapColor);
                return Mark(ThingColor.Item);
            }
            if ((flags & CountItem) != 0)
                return Mark(ThingColor.CountItem);
            return Mark(ThingColor.Item);
        }
        return Mark(ThingColor.Thing);
    }

    private static ThingMark KeyThing(int keyMapColor)
    {
        if (keyMapColor >= 0)
        {
            return new ThingMark(
                ThingColor.KeyFromMap, true,
                (byte)((keyMapColor >> 16) & 0xff),
                (byte)((keyMapColor >> 8) & 0xff),
                (byte)(keyMapColor & 0xff),
                false, 0);
        }
        return new ThingMark(ThingColor.CountItem, true, 0, 0, 0, false, 0);
    }

    private static ThingMark Mark(ThingColor color)
    {
        var triangle = color != ThingColor.Suppressed;
        return new ThingMark(color, false, 0, 0, 0, triangle, triangle ? TriangleScale : 0);
    }
}
