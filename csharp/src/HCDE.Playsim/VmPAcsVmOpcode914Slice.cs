namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> DIV array, MOD, and AND var opcodes from <c>p_acs.cpp</c> (**8043–8203**).
/// Exclusive to chunk **914**; chunk **913** owns **7876–8041**.
/// </summary>
public static class VmPAcsVmOpcode914Slice
{
    public enum ScriptState
    {
        Running,
        DivideBy0,
        ModulusBy0,
    }

    public static ScriptState DivMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) / divisor);
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState DivWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] /= divisor;
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState DivGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.DivideBy0;
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] /= divisor;
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState ModScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        locals[varIndex] %= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState ModMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        mapVars[varIndex] %= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState ModWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        worldVars[varIndex] %= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState ModGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        globalVars[varIndex] %= divisor;
        sp--;
        return ScriptState.Running;
    }

    public static ScriptState ModScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        var elementIndex = stack[sp - 2];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) % divisor);
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState ModMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) % divisor);
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState ModWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] %= divisor;
        sp -= 2;
        return ScriptState.Running;
    }

    public static ScriptState ModGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var divisor = stack[sp - 1];
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] %= divisor;
        sp -= 2;
        return ScriptState.Running;
    }

    public static void AndScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] &= stack[sp - 1];
        sp--;
    }

    public static void AndMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] &= stack[sp - 1];
        sp--;
    }

    public static void AndWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] &= stack[sp - 1];
        sp--;
    }

    public static void AndGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] &= stack[sp - 1];
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
