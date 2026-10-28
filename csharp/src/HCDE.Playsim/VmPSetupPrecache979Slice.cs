namespace HCDE.Playsim;

/// <summary>
/// `AddToList` texture precache helpers and `PrecacheLevel` header from `p_setup.cpp` (**1–150**).
/// </summary>
public static class VmPSetupPrecache979Slice
{
    public const int HitFlat = 1;
    public const int HitWall = 2;
    public const int HitSky = 4;

    public static bool HitlistBitAlreadySet(byte current, int bitmask) =>
        (current & bitmask) != 0;

    public static byte MergeHitlistBit(byte current, int bitmask) =>
        (byte)(current | bitmask);

    public static bool ShouldSkipAddToList(byte hitlistEntry, int bitmask) =>
        HitlistBitAlreadySet(hitlistEntry, bitmask);

    public static bool TextureMatchesAnimationBase(int texIndex, int baseIndex) =>
        texIndex == baseIndex;

    public static bool TextureInsideContinuousAnimation(int texIndex, int baseIndex, int numFrames, bool discrete) =>
        !discrete && baseIndex < texIndex && texIndex < baseIndex + numFrames;

    public static bool ShouldExpandAnimationFrames(int texIndex, int baseIndex, int numFrames, bool discrete) =>
        TextureMatchesAnimationBase(texIndex, baseIndex)
        || TextureInsideContinuousAnimation(texIndex, baseIndex, numFrames, discrete);

    public static bool ShouldMarkSwitchFrames(bool switchFound, int numFrames) =>
        switchFound && numFrames > 0;

    public static bool ShouldUseBoomAnimatedSwitchFallback(int numFrames, int pairNumFrames) =>
        numFrames == 1 && pairNumFrames == 1;

    public static bool ShouldMarkAnimatedDoorFrames(bool animatedDoorFound, int numTextureFrames) =>
        animatedDoorFound && numTextureFrames > 0;

    public static bool PrecacheLevelHeaderInBand(int nativeLine) => nativeLine >= 149 && nativeLine <= 150;

    public static int MarkHitlistRange(byte[] hitlist, int startIndex, int frameCount, int bitmask)
    {
        var marked = 0;
        for (var i = 0; i < frameCount; i++)
        {
            var index = startIndex + i;
            if (index < 0 || index >= hitlist.Length)
                continue;
            if (ShouldSkipAddToList(hitlist[index], bitmask))
                continue;
            hitlist[index] = MergeHitlistBit(hitlist[index], bitmask);
            marked++;
        }

        return marked;
    }

    public static bool TryAddToHitlist(byte[] hitlist, int textureIndex, int bitmask, out byte mergedEntry)
    {
        mergedEntry = 0;
        if (textureIndex < 0 || textureIndex >= hitlist.Length)
            return false;
        if (ShouldSkipAddToList(hitlist[textureIndex], bitmask))
            return false;

        mergedEntry = MergeHitlistBit(hitlist[textureIndex], bitmask);
        hitlist[textureIndex] = mergedEntry;
        return true;
    }

    public static int ApplyAddToListPolicy(byte[] hitlist, int textureIndex, int bitmask)
    {
        if (!TryAddToHitlist(hitlist, textureIndex, bitmask, out _))
            return 0;
        return 1;
    }
}
