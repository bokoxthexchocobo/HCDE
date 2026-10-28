namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> call/return and stack arithmetic opcodes from <c>p_acs.cpp</c> (**7534–7709**).
/// Exclusive to chunk **911** (chunk **17** owns <c>PCD_CALLSTACK</c> **7526–7533** only).
/// </summary>
public static class VmPAcsVmOpcode911Slice
{
    public const int StackSize = 4096;
    public const int CallStackMargin = 64;
    public const int LibraryIdShift = 20;
    public const int FuncnumMask = 0xFFFF;

    public enum ScriptState
    {
        Running,
        PleaseRemove,
        DivideBy0,
        ModulusBy0,
    }

    public readonly record struct ScriptFunction(int ArgCount, int LocalCount, int Address);

    public readonly record struct CallReturnFrame(
        int ReturnAddress,
        int ReturnFunctionKey,
        int ReturnModuleKey,
        int SavedLocalsBase,
        int SavedLocalsCount,
        bool DiscardResult,
        uint EntryInstrCount);

    public static int MaskCallStackFuncnum(int taggedFuncnum) => taggedFuncnum & FuncnumMask;

    public static int LibraryIdFromTaggedFuncnum(int taggedFuncnum) => taggedFuncnum >> LibraryIdShift;

    public static (int moduleKey, int funcnum) ResolveCallStackTarget(int stackTopTagged, Func<int, int?> resolveModuleKey)
    {
        var moduleKey = resolveModuleKey(LibraryIdFromTaggedFuncnum(stackTopTagged)) ?? 0;
        return (moduleKey, MaskCallStackFuncnum(stackTopTagged));
    }

    public static (int moduleKey, int funcnum) ResolveEmbeddedCallTarget(int activeModuleKey, int nextFuncByte) =>
        (activeModuleKey, nextFuncByte);

    public static bool IsFunctionMissing(ScriptFunction? func) => func is null;

    public static bool WouldOverflowStackForCall(int sp, int localCount) =>
        sp + localCount + CallStackMargin > StackSize;

    public static int ZeroInitializeLocalSlots(Span<int> stack, ref int sp, int localCount)
    {
        for (var i = 0; i < localCount; i++)
            stack[sp + i] = 0;
        sp += localCount;
        return localCount;
    }

    public static int CallReturnStackWords() =>
        (System.Runtime.InteropServices.Marshal.SizeOf<CallReturnFrame>() + sizeof(int) - 1) / sizeof(int);

    public static int LocalsBaseForCall(int sp, int argCount) => sp - argCount;

    public static int PopReturnValueOperand(bool returnVal, ref int sp, ReadOnlySpan<int> stack)
    {
        if (returnVal)
            return stack[--sp];
        return 0;
    }

    public static bool ShouldPushReturnValue(bool discardResult) => !discardResult;

    public static ScriptState RestoreReturnFrame(
        ref int sp,
        Span<int> stack,
        CallReturnFrame frame,
        int returnValue,
        out int restoredLocalsBase,
        out int restoredLocalsCount,
        out int returnPc,
        out int returnFunctionKey,
        out int returnModuleKey)
    {
        sp -= CallReturnStackWords();
        restoredLocalsBase = frame.SavedLocalsBase;
        restoredLocalsCount = frame.SavedLocalsCount;
        returnPc = frame.ReturnAddress;
        returnFunctionKey = frame.ReturnFunctionKey;
        returnModuleKey = frame.ReturnModuleKey;
        sp = restoredLocalsBase + restoredLocalsCount;
        if (ShouldPushReturnValue(frame.DiscardResult))
            stack[sp++] = returnValue;
        return ScriptState.Running;
    }

    public static bool StackHasBinaryOperands(int sp) => sp >= 2;

    public static int StackSecond(ReadOnlySpan<int> stack, int sp) => stack[sp - 2];

    public static int StackTop(ReadOnlySpan<int> stack, int sp) => stack[sp - 1];

    public static void StoreBinaryResult(Span<int> stack, ref int sp, int value)
    {
        stack[sp - 2] = value;
        sp--;
    }

    public static ScriptState TryApplyAdd(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) + StackTop(stack, sp));
        return ScriptState.Running;
    }

    public static ScriptState TryApplySubtract(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) - StackTop(stack, sp));
        return ScriptState.Running;
    }

    public static ScriptState TryApplyMultiply(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) * StackTop(stack, sp));
        return ScriptState.Running;
    }

    public static ScriptState TryApplyDivide(Span<int> stack, ref int sp)
    {
        var divisor = StackTop(stack, sp);
        if (divisor == 0)
            return ScriptState.DivideBy0;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) / divisor);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyModulus(Span<int> stack, ref int sp)
    {
        var divisor = StackTop(stack, sp);
        if (divisor == 0)
            return ScriptState.ModulusBy0;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) % divisor);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyEq(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) == StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyNe(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) != StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static ScriptState TryApplyLt(Span<int> stack, ref int sp)
    {
        if (!StackHasBinaryOperands(sp))
            return ScriptState.PleaseRemove;
        StoreBinaryResult(stack, ref sp, StackSecond(stack, sp) < StackTop(stack, sp) ? 1 : 0);
        return ScriptState.Running;
    }

    public static CallReturnFrame BuildCallReturnFrame(
        int returnPc,
        int returnFunctionKey,
        int returnModuleKey,
        int localsBase,
        int localsCount,
        bool discardResult,
        uint entryInstrCount) =>
        new(returnPc, returnFunctionKey, returnModuleKey, localsBase, localsCount, discardResult, entryInstrCount);

    public static bool TryEnterScriptFunction(
        ScriptFunction func,
        int returnPc,
        int returnFunctionKey,
        int returnModuleKey,
        int localsBase,
        int localsCount,
        bool discardResult,
        uint runaway,
        Span<int> stack,
        ref int sp,
        out CallReturnFrame frame,
        out int newPc,
        out int newFunctionKey,
        out int newModuleKey)
    {
        frame = default;
        newPc = 0;
        newFunctionKey = 0;
        newModuleKey = 0;
        if (WouldOverflowStackForCall(sp, func.LocalCount))
            return false;
        ZeroInitializeLocalSlots(stack, ref sp, func.LocalCount);
        frame = BuildCallReturnFrame(
            returnPc, returnFunctionKey, returnModuleKey, localsBase, localsCount, discardResult, runaway);
        sp += CallReturnStackWords();
        newPc = func.Address;
        newFunctionKey = returnFunctionKey;
        newModuleKey = returnModuleKey;
        return true;
    }
}
