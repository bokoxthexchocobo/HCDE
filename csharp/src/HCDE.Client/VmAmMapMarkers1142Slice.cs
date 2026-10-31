namespace HCDE.Client;

/// <summary>
/// Cheat boxes, marker placement, numbered marks, and author-marker textures
/// from <c>am_map.cpp</c> (<b>3151–3300</b>). Chunk <b>1142</b>.
/// </summary>
public static class VmAmMapMarkers1142Slice
{
    public const int MarkCount = 10;
    public const int Dormant = 0x10000000;
    public const int NoRotations = 0xFFFF;
    public const string DefaultMarkFont = "AMMNUMx";

    public readonly record struct Segment(double Ax, double Ay, double Bx, double By);

    public readonly record struct Marker(bool Draw, bool Flip, double ScaleX, double ScreenX, double ScreenY, double DestWidth, double DestHeight);

    public enum MarkDraw
    {
        Skip,
        Texture,
        Text
    }

    public enum AuthorTexture
    {
        Missing,
        Picture,
        RotationFrame,
        SpriteFrame
    }

    public static bool DrawCheatBox(int cheat)
    {
        if (cheat == 3 || cheat == 6)
            return true;
        return false;
    }

    public static double CheatBoxAngle(double drawnAngleDegrees, double actorYawDegrees)
    {
        return drawnAngleDegrees - actorYawDegrees;
    }

    public static List<Segment> CheatBox()
    {
        return new List<Segment>
        {
            new(-1, -1, 1, -1),
            new(1, -1, 1, 1),
            new(1, 1, -1, 1),
            new(-1, 1, -1, -1)
        };
    }

    public static Marker PlaceMarker(
        bool textureValid,
        bool flip,
        double x,
        double y,
        double scaleX,
        double scaleY,
        double displayWidth,
        double displayHeight,
        double cleanX,
        double cleanY,
        int yAdjust,
        double mapX,
        double mapY,
        double mapScale,
        int frameX,
        int frameY,
        int frameHeight)
    {
        if (!textureValid)
            return new Marker(false, false, 0, 0, 0, 0, 0);
        if (scaleX < 0)
        {
            flip = !flip;
            scaleX = -scaleX;
        }
        var screenX = frameX + (int)((x - mapX) * mapScale);
        var screenY = frameY + yAdjust + (int)(frameHeight - (y - mapY) * mapScale);
        return new Marker(true, flip, scaleX, screenX, screenY, displayWidth * cleanX * scaleX, displayHeight * cleanY * scaleY);
    }

    public static bool UseCustomMarkFont(string fontName)
    {
        if (string.Equals(fontName, DefaultMarkFont, StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    public static MarkDraw MarkStyle(double x, bool customFont)
    {
        if (x == -1)
            return MarkDraw.Skip;
        if (!customFont)
            return MarkDraw.Texture;
        return MarkDraw.Text;
    }

    public static string MarkDigit(int index)
    {
        return ((char)('0' + index)).ToString();
    }

    public static (double X, double Y) MarkPoint(double x, double y, bool rotate, Func<double, double, (double X, double Y)> rotatePoint)
    {
        if (rotate)
            return rotatePoint(x, y);
        return (x, y);
    }

    public static (int X, int Y) MarkTextOrigin(double x, double y, double mapX, double mapY, double mapScale, int frameHeight)
    {
        return ((int)((x - mapX) * mapScale), (int)(frameHeight - (y - mapY) * mapScale));
    }

    public static bool SkipDormantMarker(int flags2)
    {
        if ((flags2 & Dormant) != 0)
            return true;
        return false;
    }

    public static AuthorTexture AuthorSource(bool pictureValid, int rotations, int frame, int frameCount)
    {
        if (pictureValid)
        {
            if (rotations != NoRotations)
                return AuthorTexture.RotationFrame;
            return AuthorTexture.Picture;
        }
        if (frame >= frameCount)
            return AuthorTexture.Missing;
        return AuthorTexture.SpriteFrame;
    }

    public static bool AuthorFlip(AuthorTexture source, int flipBits)
    {
        if (source == AuthorTexture.RotationFrame || source == AuthorTexture.SpriteFrame)
        {
            if ((flipBits & 1) != 0)
                return true;
        }
        return false;
    }
}
