namespace HCDE.Playsim;

/// <summary><c>FBehaviorContainer</c> module helpers from <c>p_acs.cpp</c> (1977–2124).</summary>
public static class VmAcsBehaviorContainer
{
    public static void LoadDefaultModulesNative(
        IEnumerable<string> loadAcsTokens,
        Func<string, bool> moduleExists,
        Action<string> loadModule,
        Action<string> printMissing)
    {
        foreach (var token in loadAcsTokens)
        {
            if (moduleExists(token))
                loadModule(token);
            else
                printMissing(token);
        }
    }

    public static FBehavior? LoadModuleNative(
        int lumpNum,
        IReadOnlyList<(int LumpNum, FBehavior Behavior)> staticModules,
        Func<FBehavior> createBehavior,
        Func<FBehavior, bool> initBehavior,
        Action<string> printInvalid)
    {
        if (lumpNum == -1)
            return null;

        for (var i = 0; i < staticModules.Count; i++)
        {
            if (staticModules[i].LumpNum == lumpNum)
                return staticModules[i].Behavior;
        }

        var behavior = createBehavior();
        if (initBehavior(behavior))
            return behavior;

        printInvalid(lumpNum.ToString());
        return null;
    }

    public static bool CheckAllGoodNative(IEnumerable<bool> moduleGoodFlags)
    {
        foreach (var good in moduleGoodFlags)
        {
            if (!good)
                return false;
        }

        return true;
    }

    public static int? GetModuleIndexNative(int lib, int moduleCount) =>
        (uint)lib >= (uint)moduleCount ? null : lib;

    public static void MarkLevelVarStringsNative(
        int moduleCount,
        Action<int> markMapVars,
        Action markServerScripts,
        Action markClientScripts)
    {
        for (var mod = 0; mod < moduleCount; mod++)
            markMapVars(mod);
        markServerScripts();
        markClientScripts();
    }

    public static void LockLevelVarStringsNative(
        int moduleCount,
        int levelNum,
        Action<int, int> lockMapVars,
        Action<int> lockServerScripts,
        Action<int> lockClientScripts)
    {
        for (var mod = 0; mod < moduleCount; mod++)
            lockMapVars(mod, levelNum);
        lockServerScripts(levelNum);
        lockClientScripts(levelNum);
    }

    public static void UnlockLevelVarStringsNative(int levelNum, Action<int> unlockForLevel) =>
        unlockForLevel(levelNum);

    public static void MarkMapVarStringsNative(int numArrays, Action markMapStore, Action<int> markArrayStore)
    {
        markMapStore();
        for (var i = 0; i < numArrays; i++)
            markArrayStore(i);
    }

    public static void LockMapVarStringsNative(int levelNum, int numArrays, Action<int> lockMapStore, Action<int, int> lockArrayStore)
    {
        lockMapStore(levelNum);
        for (var i = 0; i < numArrays; i++)
            lockArrayStore(i, levelNum);
    }

    public static void SerializeModuleStatesNative(
        int staticModuleCount,
        int savedModuleCount,
        Action<int> serializeEachModule)
    {
        if (savedModuleCount != staticModuleCount)
            throw new InvalidOperationException("Level was saved with a different number of ACS modules.");

        for (var mod = 0; mod < staticModuleCount; mod++)
            serializeEachModule(mod);
    }

    public sealed class FBehavior;
}
