namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> AND/EOR/OR array, EOR/OR vars, and left-shift vars from <c>p_acs.cpp</c> (**8205–8363**).
/// Exclusive to chunk **915**; chunk **914** owns **8043–8203**.
/// </summary>
public static class VmPAcsVmOpcode915Slice
{
    public static void AndScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) & mask);
        sp -= 2;
    }

    public static void AndMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) & mask);
        sp -= 2;
    }

    public static void AndWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] &= stack[sp - 1];
        sp -= 2;
    }

    public static void AndGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] &= stack[sp - 1];
        sp -= 2;
    }

    public static void EorScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] ^= stack[sp - 1];
        sp--;
    }

    public static void EorMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] ^= stack[sp - 1];
        sp--;
    }

    public static void EorWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] ^= stack[sp - 1];
        sp--;
    }

    public static void EorGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] ^= stack[sp - 1];
        sp--;
    }

    public static void EorScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) ^ mask);
        sp -= 2;
    }

    public static void EorMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) ^ mask);
        sp -= 2;
    }

    public static void EorWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] ^= stack[sp - 1];
        sp -= 2;
    }

    public static void EorGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] ^= stack[sp - 1];
        sp -= 2;
    }

    public static void OrScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] |= stack[sp - 1];
        sp--;
    }

    public static void OrMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] |= stack[sp - 1];
        sp--;
    }

    public static void OrWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] |= stack[sp - 1];
        sp--;
    }

    public static void OrGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] |= stack[sp - 1];
        sp--;
    }

    public static void OrScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) | mask);
        sp -= 2;
    }

    public static void OrMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var mask = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) | mask);
        sp -= 2;
    }

    public static void OrWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] |= stack[sp - 1];
        sp -= 2;
    }

    public static void OrGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] |= stack[sp - 1];
        sp -= 2;
    }

    public static void LsScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] <<= stack[sp - 1];
        sp--;
    }

    public static void LsMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] <<= stack[sp - 1];
        sp--;
    }

    public static void LsWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] <<= stack[sp - 1];
        sp--;
    }

    public static void LsGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] <<= stack[sp - 1];
        sp--;
    }

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
