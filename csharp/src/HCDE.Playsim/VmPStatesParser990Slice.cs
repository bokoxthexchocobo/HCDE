namespace HCDE.Playsim;

/// <summary>
/// `ResolveGotoLabel` tail, `FixStatePointers`, `ResolveGotoLabels`, and `SetGotoLabel` from <c>p_states.cpp</c> (**751–900**).
/// </summary>
public static class VmPStatesParser990Slice
{
    public const int StateDefineIndexFlag = 1 << 0;
    public const int StateDefineStateFlag = 1 << 1;
    public const int StateDefineStopFlag = 1 << 2;
    public const int StateDefineLabelFlag = 1 << 3;

    public enum GotoScopeResolveKind
    {
        SuperParent,
        SameNamedClass,
        NamedForeignClass,
        UnknownClass,
        NotActorClass,
        NotDerivedFrom,
    }

    public static GotoScopeResolveKind ClassifyGotoScopeResolution(
        bool scopeIsSuper,
        object? parentClass,
        object? namedClass,
        object? currentType,
        bool namedClassIsActor,
        bool namedIsAncestorOfCurrent)
    {
        if (scopeIsSuper)
            return parentClass is null ? GotoScopeResolveKind.NotDerivedFrom : GotoScopeResolveKind.SuperParent;

        if (namedClass is null)
            return GotoScopeResolveKind.UnknownClass;

        if (!namedClassIsActor)
            return GotoScopeResolveKind.NotActorClass;

        if (!namedIsAncestorOfCurrent)
            return GotoScopeResolveKind.NotDerivedFrom;

        return ReferenceEquals(currentType, namedClass)
            ? GotoScopeResolveKind.SameNamedClass
            : GotoScopeResolveKind.NamedForeignClass;
    }

    public static bool TrySplitGotoLabelOffset(string remainder, out string label, out int offset, out bool hasOffset)
    {
        var plus = remainder.IndexOf('+');
        if (plus >= 0)
        {
            label = remainder[..plus];
            hasOffset = true;
            var offsetText = remainder[(plus + 1)..];
            if (!long.TryParse(
                    offsetText,
                    System.Globalization.NumberStyles.Integer,
                    null,
                    out var parsed))
            {
                offset = 0;
                return false;
            }

            offset = (int)parsed;
            return label.Length > 0;
        }

        label = remainder;
        offset = 0;
        hasOffset = false;
        return label.Length > 0;
    }

    public static object? ResolveGotoLabelInActorType(
        bool sameType,
        string label,
        Func<string, object?> findInSelf,
        Func<string, object?> findInOther) =>
        sameType ? findInSelf(label) : findInOther(label);

    public static bool TryApplyOwnedStateOffset(
        object? baseState,
        int stateIndex,
        int ownedCount,
        int offset,
        out int targetIndex)
    {
        targetIndex = stateIndex + offset;
        if (baseState is null || stateIndex < 0 || targetIndex < 0 || targetIndex >= ownedCount)
        {
            targetIndex = -1;
            return false;
        }

        return true;
    }

    public static int FixIndexedStatePointersInTree<T>(
        IList<T> list,
        Func<T, object?> getState,
        Func<T, int> getDefineFlags,
        Func<T, IList<T>> getChildren,
        Func<int, object?> translateIndexToState,
        Action<T, object?, int> setStateAndFlags)
    {
        var fixedCount = 0;
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            if (getDefineFlags(entry) == StateDefineIndexFlag && getState(entry) is int encodedIndex)
            {
                var translated = translateIndexToState(encodedIndex);
                setStateAndFlags(entry, translated, StateDefineStateFlag);
                fixedCount++;
            }

            var children = getChildren(entry);
            if (children.Count > 0)
            {
                fixedCount += FixIndexedStatePointersInTree(
                    children,
                    getState,
                    getDefineFlags,
                    getChildren,
                    translateIndexToState,
                    setStateAndFlags);
            }
        }

        return fixedCount;
    }

    public static int ResolvePendingGotoLabelsInTree<T>(
        IList<T> list,
        Func<T, object?> getState,
        Func<T, int> getDefineFlags,
        Func<T, IList<T>> getChildren,
        Func<object, object?> resolveLabel,
        Action<T, object?, int> setResolvedState)
    {
        var resolved = 0;
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            if (getState(entry) is { } pendingLabel && getDefineFlags(entry) == StateDefineLabelFlag)
            {
                var target = resolveLabel(pendingLabel);
                setResolvedState(entry, target, StateDefineStateFlag);
                resolved++;
            }

            var children = getChildren(entry);
            if (children.Count > 0)
            {
                resolved += ResolvePendingGotoLabelsInTree(
                    children,
                    getState,
                    getDefineFlags,
                    getChildren,
                    resolveLabel,
                    setResolvedState);
            }
        }

        return resolved;
    }

    public static bool TrySetGotoLabelOnParserSession(
        object? lastState,
        int lastLabel,
        object? lastStateBeforeLabel,
        string gotoText,
        Action<object?, object?> assignPendingGotoLabel,
        Action<int, string?> retargetFromLabel,
        Action<object?> clearLastStateBeforeLabel)
    {
        if (lastState is not null)
        {
            assignPendingGotoLabel(lastState, gotoText);
            clearLastStateBeforeLabel(null);
            return true;
        }

        if (lastLabel >= 0)
        {
            retargetFromLabel(lastLabel + 1, gotoText);
            if (lastStateBeforeLabel is not null)
            {
                assignPendingGotoLabel(lastStateBeforeLabel, gotoText);
                clearLastStateBeforeLabel(null);
            }

            return true;
        }

        return false;
    }

    public interface IMutableGotoDefine
    {
        object? State { get; set; }
        int DefineFlags { get; set; }
    }
}
