namespace HCDE.Gamedata;

/// <summary>
/// LOCKDEF init (`ClearLocks`, sorted key list, `P_InitKeyMessages`) and `P_CheckKeys` head from <c>a_keys.cpp</c> (**344–493**).
/// Chunk **1026**; <c>ParseLock</c> body stays on chunk **173** / <c>VmLockDefParser</c>.
/// </summary>
public static class VmAKeysInitCheck1026Slice
{
    public const int GiShareware = 1;

    public sealed class KeyTypeEntry
    {
        public required string ClassName { get; init; }
        public int KeyNumber { get; set; }
    }

    public sealed class KeyInitState
    {
        public bool KeysDone { get; set; }
        public VmLockDefDatabase Database { get; } = new();
        public List<KeyTypeEntry> SortedKeyTypes { get; } = [];
    }

    public static void ClearLocks(IList<KeyTypeEntry> allKeyTypes, KeyInitState state)
    {
        foreach (var key in allKeyTypes)
            key.KeyNumber = 0;
        state.Database.Locks.Clear();
        state.Database.KeyNumbersByClass.Clear();
        state.KeysDone = false;
    }

    public static void ApplyKeyNumbersFromDatabase(IList<KeyTypeEntry> allKeyTypes, VmLockDefDatabase database)
    {
        foreach (var entry in allKeyTypes)
            entry.KeyNumber = database.KeyNumbersByClass.TryGetValue(entry.ClassName, out var n) ? n : 0;
    }

    public static int CompareKeyNumbers(KeyTypeEntry a, KeyTypeEntry b) =>
        a.KeyNumber.CompareTo(b.KeyNumber);

    public static void CreateSortedKeyList(IList<KeyTypeEntry> allKeyTypes, KeyInitState state)
    {
        var assigned = new List<KeyTypeEntry>();
        var unassigned = new List<KeyTypeEntry>();
        foreach (var entry in allKeyTypes)
        {
            if (entry.KeyNumber > 0)
                assigned.Add(entry);
            else
                unassigned.Add(entry);
        }

        assigned.Sort(CompareKeyNumbers);
        state.SortedKeyTypes.Clear();
        state.SortedKeyTypes.AddRange(assigned);
        state.SortedKeyTypes.AddRange(unassigned);
    }

    public static bool TryInitializeFromLockDefTexts(
        IList<KeyTypeEntry> allKeyTypes,
        IEnumerable<string> lockDefTexts,
        KeyInitState state,
        ILockDefActorCatalog actors,
        ILockDefGameFilter games,
        List<string> errors)
    {
        ClearLocks(allKeyTypes, state);
        foreach (var text in lockDefTexts)
        {
            if (!VmLockDefParser.TryParseDocument(text, state.Database, actors, games, errors))
                return false;
            ApplyKeyNumbersFromDatabase(allKeyTypes, state.Database);
        }

        CreateSortedKeyList(allKeyTypes, state);
        state.KeysDone = true;
        return errors.Count == 0;
    }

    public static bool LockCheck(VmInventoryItem owner, VmLockDefLock lockDef)
    {
        if (lockDef.KeyGroups.Count == 0)
            return VmActorKeyCheck.LockMatches(owner, [], anyKeyIfEmpty: true);

        var groups = lockDef.KeyGroups
            .Select(g => (IReadOnlyList<string>)g.AnyKeyList.Select(k => k.ActorClass).ToList())
            .ToList();
        return VmActorKeyCheck.LockMatches(owner, groups, anyKeyIfEmpty: false);
    }

    public static bool CheckKeysEarlyAllow(int keyNum) => keyNum <= 0;

    public sealed record CheckKeysSuccess(bool Allowed);

    public sealed record CheckKeysFailure(string FailText, bool UseRemoteMessage);

    public static bool TryEvaluateCheckKeysHead(
        VmInventoryItem? owner,
        int keyNum,
        bool keysDone,
        int gameInfoFlags,
        VmLockDefDatabase database,
        bool remote,
        out CheckKeysSuccess? success,
        out CheckKeysFailure? failure)
    {
        success = null;
        failure = null;
        if (owner is null)
            return false;
        if (CheckKeysEarlyAllow(keyNum))
        {
            success = new CheckKeysSuccess(true);
            return true;
        }

        if (!keysDone)
            return false;

        if (!database.Locks.TryGetValue(keyNum, out var lockDef))
        {
            var text = keyNum == 103 && (gameInfoFlags & GiShareware) != 0
                ? "$TXT_RETAIL_ONLY"
                : "$TXT_DOES_NOT_WORK";
            failure = new CheckKeysFailure(text, UseRemoteMessage: false);
            return true;
        }

        if (LockCheck(owner!, lockDef))
        {
            success = new CheckKeysSuccess(true);
            return true;
        }

        var msg = remote && !string.IsNullOrEmpty(lockDef.RemoteMessage)
            ? lockDef.RemoteMessage
            : lockDef.Message;
        failure = new CheckKeysFailure(msg, UseRemoteMessage: remote);
        return true;
    }
}
