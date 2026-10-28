namespace HCDE.Playsim;

/// <summary>
/// Jump verification, script state storage, and decorate state-definition lookup from <c>p_states.cpp</c> (**301–450**).
/// </summary>
public static class VmPStatesStorage987Slice
{
    public const int NamedNameLabelBase = 0x10000000;
    public const int JumpIndexShift = 16;
    public const int JumpIndexMask = 32767;
    public const int StateDefineNextFlag = 1 << 1;

    public static bool UsesNamedNameLabelEncoding(int storagePos) => storagePos >= NamedNameLabelBase;

    public static int DecodeNamedNameLabel(int storagePos) => storagePos - NamedNameLabelBase;

    public static bool UsesRuntimeJumpEncoding(int storagePos) => storagePos < 0;

    public static int DecodeJumpTargetIndex(int storagePos) => (storagePos >> JumpIndexShift) & JumpIndexMask;

    public static int DecodeJumpStorageByteOffset(int storagePos) => ((storagePos & 65535) - 1) * 4;

    public static bool VerifyJumpTargetInClassHierarchy(
        object? startClass,
        object? actorBaseClass,
        object? callingState,
        int targetOffset,
        Func<object?, object?>? getParentClass,
        Func<object?, object?, bool>? ownsState,
        Func<object?, int, object?>? stateAtOffset)
    {
        if (getParentClass is null || ownsState is null || stateAtOffset is null)
            return false;

        var cls = startClass;
        while (cls is not null && !ReferenceEquals(cls, actorBaseClass))
        {
            if (ownsState(cls, callingState))
            {
                var target = stateAtOffset(callingState, targetOffset);
                return ownsState(cls, target);
            }

            cls = getParentClass(cls);
        }

        return false;
    }

    public static object? ResolveScriptStateFromStorage(
        int storagePos,
        object? actorClass,
        bool exact,
        Func<int, object?, object?>? findStateByNamedName,
        Func<int, object?, int, object?>? resolveJumpEncodedState,
        Func<int, int, object?, bool, object?>? findStateFromLabelCount)
    {
        if (UsesNamedNameLabelEncoding(storagePos))
        {
            var named = DecodeNamedNameLabel(storagePos);
            return findStateByNamedName?.Invoke(named, actorClass);
        }

        if (UsesRuntimeJumpEncoding(storagePos))
        {
            var index = DecodeJumpTargetIndex(storagePos);
            var offset = DecodeJumpStorageByteOffset(storagePos);
            return resolveJumpEncodedState?.Invoke(offset, actorClass, index);
        }

        if (storagePos <= 0)
            return null;

        return ResolveBundledStoragePos(storagePos, actorClass, exact, findStateFromLabelCount);
    }

    public static object? ResolveBundledStoragePos(
        int storagePos,
        object? actorClass,
        bool exact,
        Func<int, int, object?, bool, object?>? findStateFromLabelCount,
        Func<int, int>? readLabelCount = null,
        Func<int, object?>? readEmbeddedState = null)
    {
        var bundleOffset = (storagePos - 1) * 4;
        var labelCount = readLabelCount?.Invoke(bundleOffset) ?? 0;
        if (labelCount == 0)
            return readEmbeddedState?.Invoke(bundleOffset + sizeof(int));

        if (actorClass is null || findStateFromLabelCount is null)
            return null;

        return findStateFromLabelCount(labelCount, bundleOffset + sizeof(int), actorClass, exact);
    }

    public static int FindStateDefineIndex<T>(
        IList<T> list,
        Func<T, string> getLabel,
        string name)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (getLabel(list[i]) == name)
                return i;
        }

        return -1;
    }

    public static int FindOrCreateStateDefineIndex<T>(
        IList<T> list,
        Func<T, string> getLabel,
        string name,
        bool create,
        Func<string, T> createDefine)
    {
        var existing = FindStateDefineIndex(list, getLabel, name);
        if (existing >= 0)
            return existing;

        if (!create)
            return -1;

        list.Add(createDefine(name));
        return list.Count - 1;
    }

    public static T? WalkStateDefineAddress<T>(
        IReadOnlyList<string> nameParts,
        IList<T> rootList,
        Func<T, string> getLabel,
        Func<T, IList<T>> getChildren,
        Func<string, T> createDefine)
        where T : class
    {
        if (nameParts.Count == 0)
            return null;

        IList<T> currentList = rootList;
        T? current = null;
        for (var i = 0; i < nameParts.Count; i++)
        {
            var index = FindOrCreateStateDefineIndex(
                currentList,
                getLabel,
                nameParts[i],
                create: true,
                createDefine);
            if (index < 0)
                return null;
            current = currentList[index];
            currentList = getChildren(current);
        }

        return current;
    }
}
