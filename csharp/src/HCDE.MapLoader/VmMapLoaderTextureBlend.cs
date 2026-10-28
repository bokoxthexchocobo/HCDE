namespace HCDE.MapLoader;

/// <summary><c>MapLoader::SetTexture</c> blend path from <c>maploader.cpp</c> (243–269).</summary>
public static class VmMapLoaderTextureBlend
{
    public static bool TryParseHexBlend(string name, out uint blend)
    {
        blend = 0;
        if (name.Length == 0)
            return false;
        var slice = name.Length > 8 ? name[..8] : name;
        if (!uint.TryParse(slice, System.Globalization.NumberStyles.HexNumber, null, out blend))
            return false;
        return true;
    }

    public static (bool UseColormap, uint Blend, bool UseWallTexture) ResolveBlendName(
        string name,
        Func<string, uint> colormapLookup,
        Func<string, bool> wallTextureExists)
    {
        var cmap = colormapLookup(name);
        if (cmap != 0)
            return (true, cmap, false);

        if (wallTextureExists(name))
            return (false, 0, true);

        if (TryParseHexBlend(name, out var hex))
            return (false, hex, false);

        return (false, 0, false);
    }

    public static string TruncateBlendName(string name)
    {
        if (name.Length <= 8)
            return name;
        return name[..8];
    }
}
