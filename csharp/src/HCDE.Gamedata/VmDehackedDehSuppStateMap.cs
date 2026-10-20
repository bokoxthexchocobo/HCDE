namespace HCDE.Gamedata;

public sealed class VmDehackedStateMapEntry
{
    public string ActorType { get; init; } = "";
    public string StateLabel { get; init; } = "";
    public int StateSpan { get; set; }
    public bool OwnerIsPickup { get; set; }
}

/// <summary><c>LoadDehSupp</c> <c>StateMap</c> from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppStateMap
{
    public static bool TryParseEntry(
        string actorType,
        string stateLabel,
        int stateSpan,
        bool actorIsPickup,
        out VmDehackedStateMapEntry entry)
    {
        entry = new VmDehackedStateMapEntry
        {
            ActorType = actorType,
            StateLabel = stateLabel,
            StateSpan = stateSpan,
            OwnerIsPickup = actorIsPickup
        };
        return stateSpan >= 1 && !string.IsNullOrWhiteSpace(actorType) && !string.IsNullOrWhiteSpace(stateLabel);
    }

    public static int AssignDehIndices(IReadOnlyList<VmDehackedStateMapEntry> entries)
    {
        var count = 0;
        foreach (var e in entries)
            count += e.StateSpan;
        return count;
    }
}
