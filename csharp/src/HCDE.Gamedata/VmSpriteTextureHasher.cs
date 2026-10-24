namespace HCDE.Gamedata;

/// <summary>Sprite texture hash chain from <c>R_InitSpriteDefs</c> in <c>sprites.cpp</c>.</summary>
public static class VmSpriteTextureHasher
{
    public static uint SpriteNameId(string name) =>
        name.Length >= 4
            ? (uint)((name[0] << 24) | (name[1] << 16) | (name[2] << 8) | name[3])
            : 0;

    public static void BuildSpriteTextureChains(
        IReadOnlyList<(int Index, string Name, bool IsSprite)> textures,
        IList<int> chainNext,
        IList<int> bucketHead)
    {
        for (var i = 0; i < textures.Count; i++)
        {
            chainNext[i] = -1;
            var tex = textures[i];
            if (!tex.IsSprite || tex.Name.Length < 6) continue;
            var bucket = (int)(SpriteNameId(tex.Name) % (uint)textures.Count);
            chainNext[i] = bucketHead[bucket];
            bucketHead[bucket] = i;
        }
    }
}
