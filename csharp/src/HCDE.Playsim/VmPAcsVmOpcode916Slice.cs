namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> LS/RS array+var shifts, INC*, and DEC vars from <c>p_acs.cpp</c> (**8365–8514**).
/// Exclusive to chunk **916**; chunk **915** owns **8205–8363**.
/// </summary>
public static class VmPAcsVmOpcode916Slice
{
    public static void LsScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var shift = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) << shift);
        sp -= 2;
    }

    public static void LsMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var shift = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) << shift);
        sp -= 2;
    }

    public static void LsWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] <<= stack[sp - 1];
        sp -= 2;
    }

    public static void LsGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] <<= stack[sp - 1];
        sp -= 2;
    }

    public static void RsScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] >>= stack[sp - 1];
        sp--;
    }

    public static void RsMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] >>= stack[sp - 1];
        sp--;
    }

    public static void RsWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] >>= stack[sp - 1];
        sp--;
    }

    public static void RsGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] >>= stack[sp - 1];
        sp--;
    }

    public static void RsScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var shift = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) >> shift);
        sp -= 2;
    }

    public static void RsMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var shift = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) >> shift);
        sp -= 2;
    }

    public static void RsWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] >>= stack[sp - 1];
        sp -= 2;
    }

    public static void RsGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] >>= stack[sp - 1];
        sp -= 2;
    }

    public static void IncScriptVar(Span<int> locals, int varIndex) => locals[varIndex]++;

    public static void IncMapVar(Span<int> mapVars, int varIndex) => mapVars[varIndex]++;

    public static void IncWorldVar(Span<int> worldVars, int varIndex) => worldVars[varIndex]++;

    public static void IncGlobalVar(Span<int> globalVars, int varIndex) => globalVars[varIndex]++;

    public static void IncScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) + 1);
        sp--;
    }

    public static void IncMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) + 1);
        sp--;
    }

    public static void IncWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        worldArrays[arrayIndex][elementIndex]++;
        sp--;
    }

    public static void IncGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        globalArrays[arrayIndex][elementIndex]++;
        sp--;
    }

    public static void DecScriptVar(Span<int> locals, int varIndex) => locals[varIndex]--;

    public static void DecMapVar(Span<int> mapVars, int varIndex) => mapVars[varIndex]--;

    public static void DecWorldVar(Span<int> worldVars, int varIndex) => worldVars[varIndex]--;

    public static void DecGlobalVar(Span<int> globalVars, int varIndex) => globalVars[varIndex]--;

    public sealed class AcsScriptArrayStore
    {
        private readonly Dictionary<(int Array, int Index), int> _cells = new();

        public int Get(int arrayId, int index) =>
            _cells.TryGetValue((arrayId, index), out var value) ? value : 0;

        public void Set(int arrayId, int index, int value) => _cells[(arrayId, index)] = value;
    }

    public sealed class AcsMapArrayStore
    {
        private readonly Dictionary<(int Array, int Index), int> _cells = new();

        public int Get(int arrayId, int index) =>
            _cells.TryGetValue((arrayId, index), out var value) ? value : 0;

        public void Set(int arrayId, int index, int value) => _cells[(arrayId, index)] = value;
    }
}
