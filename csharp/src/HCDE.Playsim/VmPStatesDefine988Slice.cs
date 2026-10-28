namespace HCDE.Playsim;

/// <summary>
/// State definition install, label indexing, and label-list materialization from <c>p_states.cpp</c> (**451–600**).
/// </summary>
public static class VmPStatesDefine988Slice
{
    public const int StateDefineIndexFlag = 1 << 0;
    public const int InvalidStateLabelIndex = -1;
    public const string SpawnStateName = "Spawn";

    public static bool CanInstallStates(object? defaults, object? actorInfo) =>
        defaults is not null && actorInfo is not null;

    public static object? SelectSpawnStateForInstall(object? foundSpawn, object? defaultSpawn) =>
        foundSpawn ?? defaultSpawn;

    public static void ApplyStateLabelAssignment(IMutableStateDefine stateDefine, object? state, int defineFlags)
    {
        stateDefine.State = state;
        stateDefine.DefineFlags = defineFlags;
    }

    public static int EncodePendingStateArrayIndex(int arraySize) => arraySize + 1;

    public static bool TryDecodeStateLabelIndex(int encodedIndex, int arraySize, out int index)
    {
        index = InvalidStateLabelIndex;
        if (encodedIndex <= 0 || encodedIndex > arraySize + 1)
            return false;
        index = encodedIndex - 1;
        return true;
    }

    public static void AssignPendingStateIndexLabel(
        IMutableStateDefine stateDefine,
        int arraySize,
        out int lastLabelIndex)
    {
        lastLabelIndex = arraySize;
        stateDefine.State = EncodePendingStateArrayIndex(arraySize);
        stateDefine.DefineFlags = StateDefineIndexFlag;
    }

    public static object? ResolveStateDefineByNameParts(
        IReadOnlyList<string> nameParts,
        Func<IReadOnlyList<object>, string, object?> findInList,
        Func<object, IReadOnlyList<object>> getChildren,
        IReadOnlyList<object> rootList)
    {
        IReadOnlyList<object> current = rootList;
        object? define = null;
        for (var i = 0; i < nameParts.Count; i++)
        {
            define = findInList(current, nameParts[i]);
            if (define is null)
                return null;
            current = getChildren(define);
        }

        return define is IMutableStateDefine mutable ? mutable.State : null;
    }

    public static bool ShouldPruneEmptyStateDefine(string label, object? state, int childCount) =>
        string.IsNullOrEmpty(label) || (state is null && childCount == 0);

    public static int PruneEmptyStateDefines<T>(
        IList<T> list,
        Func<T, string> getLabel,
        Func<T, object?> getState,
        Func<T, int> getChildCount)
    {
        var removed = 0;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (!ShouldPruneEmptyStateDefine(getLabel(entry), getState(entry), getChildCount(entry)))
                continue;
            list.RemoveAt(i);
            removed++;
        }

        return removed;
    }

    public static bool MaterializeStateLabelListEnabled(int countAfterPrune) => countAfterPrune > 0;

    public static int CompareStateLabelIndices(int leftIndex, int rightIndex) => leftIndex - rightIndex;

    public static bool ShouldDestroyExistingStateList(object? stateList, bool reinstalling) =>
        reinstalling && stateList is not null;

    public static int CountCachedInstallStates(int foundSpawn, int foundSee, int foundMelee, int foundMissile) =>
        foundSpawn + foundSee + foundMelee + foundMissile;

    public interface IMutableStateDefine
    {
        object? State { get; set; }
        int DefineFlags { get; set; }
    }
}
