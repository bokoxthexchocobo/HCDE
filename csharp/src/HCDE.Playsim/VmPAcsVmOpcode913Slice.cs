namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> SUB/MUL/DIV var/array opcodes from <c>p_acs.cpp</c> (**7876–8041**).
/// Exclusive to chunk **913**; chunk **912** owns **7710–7874**.
/// </summary>
public static class VmPAcsVmOpcode913Slice
{
    public enum ScriptState
    {
        Running,
        DivideBy0,
    }

    public static void SubScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] -= stack[sp - 1];
        sp--;
    }

    public static void SubMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] -= stack[sp - 1];
        sp--;
    }

    public static void SubWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] -= stack[sp - 1];
        sp--;
    }

    public static void SubGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] -= stack[sp - 1];
        sp--;
    }

    public static void SubScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var delta = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) - delta);
        sp -= 2;
    }

    public static void SubMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var delta = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) - delta);
        sp -= 2;
    }

    public static void SubWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] -= stack[sp - 1];
        sp -= 2;
    }

    public static void SubGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] -= stack[sp - 1];
        sp -= 2;
    }

    public static void MulScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] *= stack[sp - 1];
        sp--;
    }

    public static void MulMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] *= stack[sp - 1];
        sp--;
    }

    public static void MulWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] *= stack[sp - 1];
        sp--;
    }

    public static void MulGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] *= stack[sp - 1];
        sp--;
    }

    public static void MulScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var factor = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) * factor);
        sp -= 2;
    }

    public static void MulMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var factor = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) * factor);
        sp -= 2;
    }

    public static void MulWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] *= stack[sp - 1];
        sp -= 2;
    }

    public static void MulGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] *= stack[sp - 1];
        sp -= 2;
    }

    public static ScriptState DivScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        locals[varIndex] /= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState DivMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        mapVars[varIndex] /= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState DivWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        worldVars[varIndex] /= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState DivGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        globalVars[varIndex] /= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState DivScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        var elementIndex = stack[sp - 2];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) / divisor);
        sp -= 2;
        return ScriptState.Running;
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
