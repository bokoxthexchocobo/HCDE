namespace HCDE.Gamedata;

/// <summary><c>R_InitSpriteDefs</c> sprite/voxel scan from <c>sprites.cpp</c>.</summary>
public static class VmSpriteInitScan
{
    public static void ScanSpriteTextures(
        uint spriteNameId,
        IReadOnlyList<int> textureChainHead,
        IReadOnlyList<int> textureChainNext,
        IReadOnlyList<(uint NameId, string LumpName)> textures,
        IList<VmSpriteTempFrame> sprTemp,
        ref int maxFrame,
        Action<VmSpriteTempFrame, int, char, char, bool> installLump)
    {
        var hash = textureChainHead[(int)(spriteNameId % (uint)textureChainHead.Count)];
        while (hash >= 0)
        {
            var tex = textures[hash];
            if (tex.NameId != spriteNameId) { hash = textureChainNext[hash]; continue; }
            if (tex.LumpName.Length < 5) throw new InvalidOperationException("Sprite name incomplete");
            var frame = sprTemp[0];
            installLump(frame, hash, tex.LumpName[4], tex.LumpName.Length >= 6 ? tex.LumpName[5] : '\0', false);
            if (tex.LumpName.Length >= 7 && tex.LumpName[6] != '\0')
                installLump(frame, hash, tex.LumpName[6], tex.LumpName.Length >= 8 ? tex.LumpName[7] : '\0', true);
            hash = textureChainNext[hash];
        }
    }

    public static void ApplyVoxelToFrames(
        uint spriteNameId,
        IReadOnlyList<(uint NameId, char Frame, object? Voxel)> voxels,
        IList<VmSpriteTempFrame> sprTemp,
        ref int maxFrame)
    {
        foreach (var v in voxels)
        {
            if (v.NameId != spriteNameId || v.Voxel is null) continue;
            if (v.Frame is ' ' or '\0')
            {
                for (var j = 0; j < sprTemp.Count; j++)
                {
                    if (sprTemp[j].RotateMode == -1) continue;
                    maxFrame = Math.Max(maxFrame, j);
                }
            }
            else
            {
                var j = v.Frame - 'A';
                if (j >= 0 && j < sprTemp.Count) maxFrame = Math.Max(maxFrame, j);
            }
        }
    }

    public static uint SkinSpriteNameToInt(string name)
    {
        Span<char> chars = stackalloc char[4];
        for (var i = 0; i < 4 && i < name.Length; i++) chars[i] = char.ToUpperInvariant(name[i]);
        return (uint)((chars[0] << 24) | (chars[1] << 16) | (chars[2] << 8) | chars[3]);
    }
}
