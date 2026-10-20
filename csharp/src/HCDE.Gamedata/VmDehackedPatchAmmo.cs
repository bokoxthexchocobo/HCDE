namespace HCDE.Gamedata;

public sealed class VmDehackedAmmoPatchTarget
{
    public int MaxAmount { get; set; }
    public int PerClip { get; set; }
    public int BackpackMaxAmount { get; set; }
    public int BackpackAmount { get; set; }
}

/// <summary><c>PatchAmmo</c> backpack scaling from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchAmmo
{
    public static bool IsAmmoIndexInRange(int ammoNum, int ammoNameCount) =>
        ammoNum >= 0 && ammoNum < 4 && ammoNum < ammoNameCount;

    public static void ApplyBackpackAmounts(VmDehackedAmmoPatchTarget target)
    {
        target.BackpackMaxAmount = target.MaxAmount * 2;
        target.BackpackAmount = target.PerClip;
    }

    public static int ScaleGiveAmount(int current, int newPer, int oldPer)
    {
        if (oldPer == 0) return current;
        return (int)((long)current * newPer / oldPer);
    }
}
