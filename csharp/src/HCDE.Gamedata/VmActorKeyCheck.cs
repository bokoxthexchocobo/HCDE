namespace HCDE.Gamedata;

public sealed class VmInventoryItem
{
    public string ClassName { get; set; } = "";
    public string? Species { get; set; }
    public List<VmInventoryItem> Inventory { get; } = [];
}

public static class VmActorKeyCheck
{
    public static bool OneKeyMatches(VmInventoryItem owner, string keyClass, string? keySpecies = null)
    {
        if (owner.ClassName.Equals(keyClass, StringComparison.OrdinalIgnoreCase) ||
            (keySpecies is not null && owner.Species?.Equals(keySpecies, StringComparison.OrdinalIgnoreCase) == true))
            return true;
        foreach (var item in owner.Inventory)
        {
            if (item.ClassName.Equals(keyClass, StringComparison.OrdinalIgnoreCase) ||
                (keySpecies is not null && item.Species?.Equals(keySpecies, StringComparison.OrdinalIgnoreCase) == true))
                return true;
        }

        return false;
    }

    public static bool KeygroupMatches(VmInventoryItem owner, IReadOnlyList<string> anyKeys)
    {
        foreach (var key in anyKeys)
        {
            if (OneKeyMatches(owner, key)) return true;
        }

        return false;
    }

    public static bool LockMatches(VmInventoryItem owner, IReadOnlyList<IReadOnlyList<string>> keygroups, bool anyKeyIfEmpty)
    {
        if (keygroups.Count == 0)
        {
            if (!anyKeyIfEmpty) return false;
            return owner.Inventory.Any(i => i.ClassName.Contains("Key", StringComparison.OrdinalIgnoreCase));
        }

        foreach (var group in keygroups)
        {
            if (!KeygroupMatches(owner, group)) return false;
        }

        return true;
    }
}

public static class VmLockDefKeywords
{
    public static IReadOnlyList<string> LockKeywords => ["ANY", "MESSAGE", "REMOTEMESSAGE", "MAPCOLOR", "LOCKEDSOUND"];
}
