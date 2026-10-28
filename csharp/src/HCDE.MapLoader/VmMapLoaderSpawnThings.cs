namespace HCDE.MapLoader;

/// <summary>
/// <c>MapLoader::SetMapThingUserData</c> and <c>SpawnThings</c> from <c>maploader.cpp</c> (1238–1273, 1437–1450).
/// </summary>
public static class VmMapLoaderSpawnThings
{
    public readonly struct UserDataEntry
    {
        public UserDataEntry(string key, int intVal, double floatVal, string stringVal)
        {
            Key = key;
            IntVal = intVal;
            FloatVal = floatVal;
            StringVal = stringVal;
        }

        public string Key { get; }
        public int IntVal { get; }
        public double FloatVal { get; }
        public string StringVal { get; }
    }

    public enum UserVarKind
    {
        Unsupported,
        String,
        Float,
        Int,
        Bool,
    }

    public static bool IsWritableUserField(bool isNative, bool isPrivate, bool isProtected, bool isStatic, bool isScalar) =>
        !isNative && !isPrivate && !isProtected && !isStatic && isScalar;

    public static UserVarKind ClassifyUserVarType(bool isString, bool isFloat, bool isInt, bool isBool)
    {
        if (isString)
            return UserVarKind.String;
        if (isFloat)
            return UserVarKind.Float;
        if (isInt || isBool)
            return UserVarKind.Int;
        return UserVarKind.Unsupported;
    }

    public static IEnumerable<UserDataEntry> EnumerateUserDataChain(
        IReadOnlyList<UserDataEntry> table,
        int startIndex)
    {
        for (var i = startIndex; i < table.Count && table[i].Key.Length > 0; i++)
            yield return table[i];
    }

    public static object? ResolveUserDataValue(UserDataEntry entry, UserVarKind kind) =>
        kind switch
        {
            UserVarKind.String => entry.StringVal,
            UserVarKind.Float => entry.FloatVal,
            UserVarKind.Int or UserVarKind.Bool => entry.IntVal,
            _ => null,
        };

    public static IEnumerable<int> EnumerateMapThingIndices(int count)
    {
        for (var i = 0; i < count; i++)
            yield return i;
    }

    public static bool TryGetUserDataIndex(IReadOnlyDictionary<int, int> indexByThing, int thingIndex, out int userDataIndex) =>
        indexByThing.TryGetValue(thingIndex, out userDataIndex);

    public static bool ShouldApplyUserData(bool actorPresent, bool hasUserDataIndex) =>
        actorPresent && hasUserDataIndex;

    public static int CountThingsToSpawn(int mapThingsConvertedSize) => mapThingsConvertedSize;
}
