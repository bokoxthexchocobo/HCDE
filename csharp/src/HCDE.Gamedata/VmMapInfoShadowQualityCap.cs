namespace HCDE.Gamedata;

/// <summary><c>NormalizeHcdeShadowQualityCap</c> from <c>g_mapinfo.cpp</c> (1066–1075).</summary>
public static class VmMapInfoShadowQualityCap
{
    private static readonly int[] Steps = [8192, 4096, 2048, 1024, 512, 256, 128];

    public static int Normalize(int value)
    {
        foreach (var step in Steps)
        {
            if (value >= step)
                return step;
        }

        return 128;
    }
}
