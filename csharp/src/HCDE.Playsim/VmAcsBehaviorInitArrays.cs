namespace HCDE.Playsim;

/// <summary><c>FBehavior::Init</c> ARRAY/AINI/AIMP chunk handling (<c>p_acs.cpp</c> 2499–2750).</summary>
public static class VmAcsBehaviorInitArrays
{
    public static void BindMapVarDefaultsNative(int numMapVars, Action<int, int> bindPointer)
    {
        for (var i = 0; i < numMapVars; i++)
            bindPointer(i, i);
    }

    public static (int numArrays, int[] mapIndices, int[] sizes) LoadArrayChunkNative(int chunkPairs, int[] mapIndices, int[] sizes)
    {
        var count = Math.Min(chunkPairs, mapIndices.Length);
        for (var i = 0; i < count; i++)
            mapIndices[i] = sizes[i];
        return (count, mapIndices, sizes);
    }

    public static int InitArrayFromChunkNative(int arraySize, int chunkValueCount, int[] elems, int[] chunkValues)
    {
        var initSize = Math.Min(arraySize, chunkValueCount);
        for (var j = 0; j < initSize; j++)
            elems[j] = chunkValues[j];
        return initSize;
    }

    public static int TotalArrayCountNative(int numArrays, int importChunkTotal) => numArrays + importChunkTotal;

    public static void BuildArrayPointerTableNative(int numArrays, int numTotal, Action<int, int> assignPointer)
    {
        for (var i = 0; i < numArrays; i++)
            assignPointer(i, i);
    }

    public static bool ShouldTagLibraryStrings(int libraryId) => libraryId != 0;

    public static int TagStringMapVarNative(int mapVarValue, string? resolved, Func<string, int> addString) =>
        resolved != null ? addString(resolved) : mapVarValue;

    public static void TagStringArrayElementsNative(int arraySize, int[] elems, Func<int, string?> lookup, Func<string, int> addString)
    {
        for (var j = arraySize; j > 0; j--)
        {
            var str = lookup(elems[j - 1]);
            if (str != null)
                elems[j - 1] = addString(str);
        }
    }

    public static bool ShouldApplyFunctionPtrTag(byte tag) => tag == 2;

    public static bool ShouldResolveStringTag(byte tag) => tag == 1;

    public static void ApplyAtagElementNative(ref int elem, byte tag, int libraryId, Func<int, string?> lookup, Func<string, int> addString)
    {
        if (tag == 2)
            elem |= libraryId;
        else if (tag == 1)
        {
            var str = lookup(elem);
            if (str != null)
                elem = addString(str);
        }
    }

    public static int LoadLibraryTokenNative(string parse, int index, out int nextIndex)
    {
        if (index >= parse.Length || parse[index] == '\0')
        {
            nextIndex = index;
            return -1;
        }

        var start = index;
        while (index < parse.Length && parse[index] != '\0')
            index++;
        nextIndex = index + 1;
        return start;
    }

    public static int TryResolveLibraryLumpNative(int lumpNum) => lumpNum < 0 ? -1 : lumpNum;

    public static int CountLoadedScriptsNative(int numScripts, int numFunctions) => numScripts + numFunctions;
}
