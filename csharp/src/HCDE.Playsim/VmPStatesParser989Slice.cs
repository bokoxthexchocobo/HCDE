namespace HCDE.Playsim;

/// <summary>
/// `InstallStates` cache tail, dehacked state import, goto retarget, and `ResolveGotoLabel` head from <c>p_states.cpp</c> (**601–750**).
/// </summary>
public static class VmPStatesParser989Slice
{
    public const int StateDefineIndexFlag = 1 << 0;
    public const int StateDefineStateFlag = 1 << 1;
    public const int StateDefineStopFlag = 1 << 2;
    public const int StateDefineLabelFlag = 1 << 3;
    public const int InvalidLastLabel = -1;

    public static void CacheInstalledDefaultStates(
        Action<object?>? setSpawn,
        Action<object?>? setSee,
        Action<object?>? setMelee,
        Action<object?>? setMissile,
        object? spawn,
        object? see,
        object? melee,
        object? missile)
    {
        setSpawn?.Invoke(spawn);
        setSee?.Invoke(see);
        setMelee?.Invoke(melee);
        setMissile?.Invoke(missile);
    }

    public static int ImportStateLabelTree<TSource, TDest>(
        IReadOnlyList<TSource> labels,
        IList<TDest> destination,
        Func<TSource, string> getLabel,
        Func<TSource, object?> getState,
        Func<TSource, IReadOnlyList<TSource>?> getChildren,
        Func<string, object?, TDest> createDefine,
        Action<TDest, IList<TDest>> attachChildren)
    {
        var copied = 0;
        for (var i = 0; i < labels.Count; i++)
        {
            var def = createDefine(getLabel(labels[i]), getState(labels[i]));
            destination.Add(def);
            copied++;
            var children = getChildren(labels[i]);
            if (children is { Count: > 0 })
            {
                var childList = new List<TDest>();
                copied += ImportStateLabelTree(children, childList, getLabel, getState, getChildren, createDefine, attachChildren);
                attachChildren(def, childList);
            }
        }

        return copied;
    }

    public static bool ShouldRebuildStateDefinesFromClass(object? actorClass, object? stateLabels) =>
        actorClass is not null && stateLabels is not null;

    public static void ResetStateDefineSession(
        Action? clearStateArray,
        Action<object?>? setLastState,
        Action<object?>? setLastStateBeforeLabel,
        Action<int>? setLastLabel)
    {
        clearStateArray?.Invoke();
        setLastState?.Invoke(null);
        setLastStateBeforeLabel?.Invoke(null);
        setLastLabel?.Invoke(InvalidLastLabel);
    }

    public static bool ShouldMergeFlatStateDefine(
        object? childLabels,
        bool labelAlreadyPresent) =>
        childLabels is null && !labelAlreadyPresent;

    public static bool ShouldRetargetIndexedDefine(object? encodedState, int defineFlags, int targetCount) =>
        defineFlags == StateDefineIndexFlag && encodedState is int pointer && pointer == targetCount;

    public static void ApplyRetargetToStop(IMutableRetargetDefine define)
    {
        define.State = null;
        define.DefineFlags = StateDefineStopFlag;
    }

    public static void ApplyRetargetToLabel(IMutableRetargetDefine define, object labelTarget)
    {
        define.State = labelTarget;
        define.DefineFlags = StateDefineLabelFlag;
    }

    public static int RetargetIndexedDefinesInTree<T>(
        IList<T> list,
        int targetCount,
        string? targetLabel,
        Func<T, object?> getState,
        Func<T, int> getDefineFlags,
        Func<T, IList<T>> getChildren,
        Action<T, bool, object?> applyRetarget)
    {
        var retargeted = 0;
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            if (ShouldRetargetIndexedDefine(getState(entry), getDefineFlags(entry), targetCount))
            {
                applyRetarget(entry, targetLabel is null, targetLabel!);
                retargeted++;
            }

            var children = getChildren(entry);
            if (children.Count > 0)
                retargeted += RetargetIndexedDefinesInTree(
                    children,
                    targetCount,
                    targetLabel,
                    getState,
                    getDefineFlags,
                    getChildren,
                    applyRetarget);
        }

        return retargeted;
    }

    public static bool GotoLabelHasClassScope(string gotoName, out int separatorIndex)
    {
        separatorIndex = gotoName.IndexOf("::", StringComparison.Ordinal);
        return separatorIndex >= 0;
    }

    public static bool GotoClassScopeIsSuper(string classToken) =>
        string.Equals(classToken, "Super", StringComparison.OrdinalIgnoreCase);

    public static string ExtractGotoLabelAfterScope(string gotoName, int separatorIndex) =>
        gotoName[(separatorIndex + 2)..];

    public interface IMutableRetargetDefine
    {
        object? State { get; set; }
        int DefineFlags { get; set; }
    }
}
