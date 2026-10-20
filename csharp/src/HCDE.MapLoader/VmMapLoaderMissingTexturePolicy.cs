namespace HCDE.MapLoader;

/// <summary><c>SetTexture</c> warning policy from <c>maploader.cpp</c> (143–209).</summary>
public static class VmMapLoaderMissingTexturePolicy
{
    public const int WarnLimit = 20;

    public static bool ShouldPrintWarning(int count) => count <= WarnLimit;

    public static string TruncateFlatName(string name, bool truncate)
    {
        if (!truncate || name.Length <= 8)
            return name;
        return name[..8];
    }
}
