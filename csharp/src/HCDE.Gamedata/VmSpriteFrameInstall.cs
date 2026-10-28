namespace HCDE.Gamedata;

public sealed class VmSpriteTempFrame
{
    public int[] TextureIds { get; } = Enumerable.Repeat(-1, 16).ToArray();
    public ushort FlipMask { get; set; }
    public int RotateMode { get; set; } = -1;
}

/// <summary><c>GetSpriteFrame</c>, <c>R_InstallSpriteLump</c>, <c>R_InstallSprite</c> rotation logic from <c>sprites.cpp</c>.</summary>
public static class VmSpriteFrameInstall
{
    public static int ResolveSpriteRotation(
        VmSpriteTempFrame frame,
        int requestedRot,
        double angleBams,
        bool flipAgain)
    {
        if (requestedRot >= 0) return requestedRot;
        if (frame.TextureIds[0] == frame.TextureIds[1] && flipAgain)
            return (int)((-angleBams + (1 << 28) * (360.0 + 45.0 / 2 * 9) / 360.0) / (1 << 28)) & 15;
        if (frame.TextureIds[0] == frame.TextureIds[1])
            return (int)((angleBams + (1 << 28) * (45.0 / 2 * 9) / 360.0) / (1 << 28)) & 15;
        if (flipAgain)
            return (int)((-angleBams + (1 << 28) * (360.0 + (45.0 / 2 * 9 - 180.0 / 16)) / 360.0) / (1 << 28)) & 15;
        return (int)((angleBams + (1 << 28) * (45.0 / 2 * 9 - 180.0 / 16) / 360.0) / (1 << 28)) & 15;
    }

    public static bool TryGetSpriteTextureId(
        VmSpriteTempFrame frame,
        int frameIndex,
        int numFrames,
        int rot,
        double angleBams,
        bool flipAgain,
        out int textureId,
        out bool mirror)
    {
        textureId = -1;
        mirror = false;
        if (frameIndex < 0 || frameIndex >= numFrames) return false;
        var resolvedRot = ResolveSpriteRotation(frame, rot, angleBams, flipAgain);
        mirror = (frame.FlipMask & (1 << resolvedRot)) != 0;
        textureId = frame.TextureIds[resolvedRot];
        return textureId >= 0;
    }

    public static bool InstallSpriteLump(VmSpriteTempFrame frame, int textureId, int frameIndex, char rotChar, bool flipped, ref int maxFrame)
    {
        if (frameIndex >= 256) return false;
        var rotation = rotChar switch
        {
            >= '0' and <= '9' => rotChar - '0',
            >= 'A' and <= 'Z' => rotChar - 'A' + 10,
            >= 'a' and <= 'z' => rotChar - 'a' + 10,
            _ => 17
        };
        if (rotation > 16) return false;
        if (frameIndex > maxFrame) maxFrame = frameIndex;
        if (rotation == 0)
        {
            for (var r = 14; r >= 0; r -= 2)
            {
                if (frame.TextureIds[r] < 0)
                {
                    frame.TextureIds[r] = textureId;
                    if (flipped) frame.FlipMask |= (ushort)(1 << r);
                    frame.RotateMode = 0;
                }
            }
        }
        else
        {
            rotation = rotation <= 8 ? (rotation - 1) * 2 : (rotation - 9) * 2 + 1;
            if (frame.TextureIds[rotation] < 0)
            {
                frame.TextureIds[rotation] = textureId;
                if (flipped) frame.FlipMask |= (ushort)(1 << rotation);
                frame.RotateMode = 1;
            }
        }

        return true;
    }

    public static void FinalizeSpriteFrames(IList<VmSpriteTempFrame> frames, ref int maxFrame)
    {
        if (maxFrame < 0) return;
        maxFrame++;
        for (var f = 0; f < maxFrame; f++)
        {
            var frame = frames[f];
            switch (frame.RotateMode)
            {
                case -1:
                    break;
                case 0:
                    for (var rot = 1; rot < 16; rot++) frame.TextureIds[rot] = frame.TextureIds[0];
                    if ((frame.FlipMask & 1) != 0) frame.FlipMask = 0xFFFF;
                    break;
                case 1:
                    for (var rot = 0; rot < 8; rot++)
                    {
                        if (frame.TextureIds[rot * 2 + 1] < 0) frame.TextureIds[rot * 2 + 1] = frame.TextureIds[rot * 2];
                        if (frame.TextureIds[rot * 2] < 0) frame.TextureIds[rot * 2] = frame.TextureIds[rot * 2 + 1];
                    }

                    break;
            }
        }
    }
}
