namespace HCDE.Playsim;

/// <summary><c>FBehavior::Init</c> import resolution and <c>~FBehavior</c> (<c>p_acs.cpp</c> 2653–2790).</summary>
public static class VmAcsBehaviorInitImports
{
    public static bool ShouldResolveImportFunction(int address, int importNum) => address == 0 && importNum == 0;

    public static bool ImportFunctionMatches(int libAddress, int libImportNum) =>
        libAddress != 0 || libImportNum != 0;

    public static void ApplyResolvedImportFunctionNative(
        VmAcsBehaviorInitLoad.ScriptFunctionInFile target,
        VmAcsBehaviorInitLoad.ScriptFunctionInFile real,
        int libIndex,
        int libFuncIndex)
    {
        target.Address = libFuncIndex;
        target.ImportNum = libIndex + 1;
        target.LocalCount = real.LocalCount;
        target.HasReturnValue = real.HasReturnValue;
    }

    public static bool ImportArgCountMismatch(int libArgCount, int funcArgCount) => libArgCount != funcArgCount;

    public static int ResolveImportMapVarNative(int mapVarValue, int libMapVarValue, int libraryId) =>
        mapVarValue == 0 ? libMapVarValue | libraryId : mapVarValue;

    public static bool ShouldSkipNullImport(object? lib) => lib == null;

    public static bool ImportArraySizeMismatch(uint libSize, uint expectedSize) => libSize != expectedSize;

    public static void ResolveImportArrayNative(
        int varNum,
        int impNum,
        int numArrays,
        int slot,
        uint expectedSize,
        uint libSize,
        Action<int, int> assignArray,
        Action<int, int> assignMapStore)
    {
        if (impNum < 0)
            return;
        assignArray(numArrays + slot, impNum);
        assignMapStore(varNum, numArrays + slot);
        if (libSize != expectedSize)
            throw new InvalidOperationException("Imported array size mismatch.");
    }

    public static void ReleaseArrayStoreNative(int numArrays, Action<int> releaseElements)
    {
        for (var i = 0; i < numArrays; i++)
            releaseElements(i);
    }

    public static void ReleaseBehaviorNative(
        bool hasScripts,
        bool hasArrays,
        bool hasArrayStore,
        Action releaseScripts,
        Action releaseArrays,
        Action releaseArrayStore)
    {
        if (hasScripts)
            releaseScripts();
        if (hasArrays)
            releaseArrays();
        if (hasArrayStore)
            releaseArrayStore();
    }

    public static bool InitSucceededNative(int numScripts, int numFunctions) => numScripts >= 0 && numFunctions >= 0;

    public static int FindMapVarNameOffsetNative(int offset) => offset + 4;

    public static bool HasImportArrays(int numTotal, int numArrays) => numTotal > numArrays;
}
