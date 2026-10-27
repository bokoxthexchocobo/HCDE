namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> compare, var/array assign/push/add opcodes from <c>p_acs.cpp</c> (**7710–7874**).
/// Exclusive to chunk **912**; chunk **911** owns **7534–7709**.
/// </summary>
public static class VmPAcsVmOpcode912Slice
{
    public enum ScriptState
    {
        Running,
        PleaseRemove,
    }

    public static bool StackHasBinaryOperands(int sp) => sp >= 2;

    public static int StackSecond(ReadOnlySpan<int> stack, int sp) => stack[sp - 2];

    public static int StackTop(ReadOnlySpan<int> stack, int sp) => stack[sp - 1];

    public static void StoreBinaryResult(Span<int> stack, ref int sp, int value)
    {
        stack[sp - 2] = value;
        sp--;
    }

    public static ScriptState TryApplyGt(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) > StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyLe(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) <= StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyGe(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) >= StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static void AssignScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] = stack[sp - 1];
        sp--;
    }

    public static void AssignMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] = stack[sp - 1];
        sp--;
    }

    public static void AssignWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] = stack[sp - 1];
        sp--;
    }

    public static void AssignGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] = stack[sp - 1];
        sp--;
    }

    public static void AssignScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var value = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, value);
        sp -= 2;
    }

    public static void AssignMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var value = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, value);
        sp -= 2;
    }

    public static void AssignWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] = stack[sp - 1];
        sp -= 2;
    }

    public static void AssignGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] = stack[sp - 1];
        sp -= 2;
    }

    public static void PushScriptVar(ReadOnlySpan<int> locals, int varIndex, Span<int> stack, ref int sp) =>
        stack[sp++] = locals[varIndex];

    public static void PushMapVar(ReadOnlySpan<int> mapVars, int varIndex, Span<int> stack, ref int sp) =>
        stack[sp++] = mapVars[varIndex];

    public static void PushWorldVar(ReadOnlySpan<int> worldVars, int varIndex, Span<int> stack, ref int sp) =>
        stack[sp++] = worldVars[varIndex];

    public static void PushGlobalVar(ReadOnlySpan<int> globalVars, int varIndex, Span<int> stack, ref int sp) =>
        stack[sp++] = globalVars[varIndex];

    public static void PushScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        stack[sp - 1] = arrays.Get(arrayIndex, elementIndex);
    }

    public static void PushMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 1];
        stack[sp - 1] = mapArrays.Get(arrayId, elementIndex);
    }

    public static void PushWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        stack[sp - 1] = worldArrays[arrayIndex][elementIndex];
    }

    public static void PushGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 1];
        stack[sp - 1] = globalArrays[arrayIndex][elementIndex];
    }

    public static void AddScriptVar(Span<int> locals, int varIndex, Span<int> stack, ref int sp)
    {
        locals[varIndex] += stack[sp - 1];
        sp--;
    }

    public static void AddMapVar(Span<int> mapVars, int varIndex, Span<int> stack, ref int sp)
    {
        mapVars[varIndex] += stack[sp - 1];
        sp--;
    }

    public static void AddWorldVar(Span<int> worldVars, int varIndex, Span<int> stack, ref int sp)
    {
        worldVars[varIndex] += stack[sp - 1];
        sp--;
    }

    public static void AddGlobalVar(Span<int> globalVars, int varIndex, Span<int> stack, ref int sp)
    {
        globalVars[varIndex] += stack[sp - 1];
        sp--;
    }

    public static void AddScriptArray(AcsScriptArrayStore arrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        var delta = stack[sp - 1];
        arrays.Set(arrayIndex, elementIndex, arrays.Get(arrayIndex, elementIndex) + delta);
        sp -= 2;
    }

    public static void AddMapArray(
        AcsMapArrayStore mapArrays,
        int mapVarIndex,
        ReadOnlySpan<int> mapVarCells,
        Span<int> stack,
        ref int sp)
    {
        var arrayId = mapVarCells[mapVarIndex];
        var elementIndex = stack[sp - 2];
        var delta = stack[sp - 1];
        mapArrays.Set(arrayId, elementIndex, mapArrays.Get(arrayId, elementIndex) + delta);
        sp -= 2;
    }

    public static void AddWorldArray(int[][] worldArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        worldArrays[arrayIndex][elementIndex] += stack[sp - 1];
        sp -= 2;
    }

    public static void AddGlobalArray(int[][] globalArrays, int arrayIndex, Span<int> stack, ref int sp)
    {
        var elementIndex = stack[sp - 2];
        globalArrays[arrayIndex][elementIndex] += stack[sp - 1];
        sp -= 2;
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
