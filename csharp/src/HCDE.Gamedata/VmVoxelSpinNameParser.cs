namespace HCDE.Gamedata;

public sealed class VmVoxelSpinNameParseResult
{
    public bool Valid { get; set; }
    public int NameId { get; set; }
    public char Frame { get; set; }
    public int SpinPerSecond { get; set; }
}

/// <summary>Voxel lump short-name parse from <c>R_InitSpriteDefs</c> in <c>sprites.cpp</c>.</summary>
public static class VmVoxelSpinNameParser
{
    public static bool TryParse(string name, int maxSpriteFrames, out VmVoxelSpinNameParseResult result)
    {
        result = new VmVoxelSpinNameParseResult();
        if (name.Length < 4) return false;
        if (name.Length > 4 && name[4] != ' ')
        {
            if (name[4] < 'A' || name[4] >= 'A' + maxSpriteFrames) return false;
        }
        var spin = 0;
        var sign = 2;
        var j = 5;
        if (j < name.Length && name[j] == '-')
        {
            j++;
            sign = -2;
        }

        for (; j < name.Length; j++)
        {
            if (name[j] is >= '0' and <= '9') spin = spin * 10 + (name[j] - '0');
            else return false;
        }

        result.Valid = true;
        result.NameId = (int)VmSpriteTextureHasher.SpriteNameId(name);
        result.Frame = name.Length > 4 ? name[4] : '\0';
        result.SpinPerSecond = spin * sign;
        return true;
    }
}
