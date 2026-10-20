namespace HCDE.Gamedata;

/// <summary><c>PatchMisc</c> armor/health defaults from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchMiscArmor
{
    public static double GreenArmorSavePercent(int greenAc) => greenAc == 1 ? 33.335 : 50;

    public static int ArmorSaveAmount(int armorClass) => 100 * armorClass;

    public static int NormalizeInfighting(int raw) =>
        raw == 0xDD ? 1 : raw == -1 ? -1 : 0;
}
