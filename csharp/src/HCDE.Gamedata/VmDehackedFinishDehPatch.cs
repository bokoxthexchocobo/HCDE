namespace HCDE.Gamedata;

public sealed class VmDehackedPickupWrapperDraft
{
    public string SourceActor { get; init; } = "";
    public string PickupClassName { get; init; } = "";
    public bool IsSpecial { get; init; }
}

public sealed class VmDehackedClassReplacementChain
{
    public string Replacement { get; set; } = "";
    public string Replacee { get; set; } = "";
    public string? PriorReplacement { get; set; }
}

/// <summary><c>FinishDehPatch</c> from <c>d_dehacked.cpp</c> (3799–3892).</summary>
public static class VmDehackedFinishDehPatch
{
    public const uint Mf8RetargetAfterSlam = 0x00000080;
    public const uint MfSpecial = 0x00000080;
    public const uint Mf6IntryMove = 1u << 14;
    public const uint SufWeapon = 1u;

    public static bool ShouldLogMbfAliasMismatch(int aliasCount, int factoryCount) =>
        aliasCount != factoryCount;

    public static uint ApplyRetargetAfterSlamFlags(uint flags8) => flags8 | Mf8RetargetAfterSlam;

    public static bool ShouldCreatePickupWrapper(uint actorFlags) => (actorFlags & MfSpecial) != 0;

    public static string BuildPickupClassName(int nameIndex) => $"DehackedPickup{nameIndex}";

    public static bool TryAllocatePickupClassName(int startIndex, Func<string, bool> classExists, out string className, out int nextIndex)
    {
        var index = startIndex;
        while (true)
        {
            className = BuildPickupClassName(index);
            if (!classExists(className))
            {
                nextIndex = index + 1;
                return true;
            }

            index++;
        }
    }

    public static void LinkPickupReplacement(
        string pickupClass,
        string replacee,
        string? oldReplacement,
        VmDehackedClassReplacementChain chain)
    {
        chain.Replacee = replacee;
        chain.Replacement = pickupClass;
        if (oldReplacement is not null)
            chain.Replacement = oldReplacement;
    }

    public static uint MarkStateUseWeapon(uint useFlags) => useFlags | SufWeapon;

    public static int CountWeaponStateSlots(IReadOnlyList<VmDehackedStateMapEntry> entries)
    {
        var total = 0;
        foreach (var entry in entries)
            total += entry.StateSpan;
        return total;
    }
}
