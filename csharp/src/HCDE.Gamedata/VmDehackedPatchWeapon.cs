namespace HCDE.Gamedata;

/// <summary><c>PatchWeapon</c> frame and ammo dispatch from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchWeapon
{
    public static bool IsWeaponIndexInRange(int weaponNum, int weaponNameCount) =>
        weaponNum >= 0 && weaponNum < 9 && weaponNum < weaponNameCount;

    public static int ClampAmmoTypeIndex(int value, int ammoNameCount)
    {
        if (value < 0 || value >= 12 || value >= ammoNameCount) return 5;
        return value;
    }

    public static bool TryMapWeaponFrameLabel(string key, int stateNum, IDictionary<string, int> labels)
    {
        if (!key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase)) return false;
        if (key.StartsWith("Deselect", StringComparison.OrdinalIgnoreCase))
        {
            labels["Select"] = stateNum;
            return true;
        }

        if (key.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
        {
            labels["Deselect"] = stateNum;
            return true;
        }

        if (key.StartsWith("Bobbing", StringComparison.OrdinalIgnoreCase))
        {
            labels["Ready"] = stateNum;
            return true;
        }

        if (key.StartsWith("Shooting", StringComparison.OrdinalIgnoreCase))
        {
            labels["Fire"] = stateNum;
            return true;
        }

        if (key.StartsWith("Firing", StringComparison.OrdinalIgnoreCase))
        {
            labels["Flash"] = stateNum;
            return true;
        }

        return false;
    }
}
