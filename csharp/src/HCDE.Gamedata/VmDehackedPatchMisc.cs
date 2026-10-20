namespace HCDE.Gamedata;

public sealed class VmDehackedMiscInfo
{
    public int StartHealth { get; set; } = 100;
    public int StartBullets { get; set; } = 50;
    public int MaxHealth { get; set; } = 100;
    public int MaxArmor { get; set; } = 200;
    public int BfgCells { get; set; }
    public int ExplosionStyle { get; set; }
    public double ExplosionAlpha { get; set; } = 1;
    public int Infighting { get; set; }
}

/// <summary><c>PatchMisc</c> key table helpers from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchMisc
{
    public static bool TryApplyKnownKey(string key, int value, VmDehackedMiscInfo info)
    {
        switch (key.ToLowerInvariant())
        {
            case "initial health": info.StartHealth = value; return true;
            case "initial bullets": info.StartBullets = value; return true;
            case "max health": info.MaxHealth = value; return true;
            case "max armor": info.MaxArmor = value; return true;
            case "bfg cells/shot": info.BfgCells = value; return true;
            case "monsters infight": info.Infighting = value; return true;
            case "monsters ignore each other": info.Infighting = value != 0 ? -1 : 0; return true;
            default: return false;
        }
    }

    public static void ApplyExplosionAlpha(VmDehackedMiscInfo info, double alpha) => info.ExplosionAlpha = alpha;

    public static int MapInfightingFromDeh(int raw) => raw;
}
