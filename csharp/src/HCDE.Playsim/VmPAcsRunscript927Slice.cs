namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> push/stack opcodes and <c>PCD_LSPEC1</c>…<c>5</c> from <c>p_acs.cpp</c> (**7216–7365**); chunk **927**.
/// </summary>
public static class VmPAcsRunscript927Slice
{
    public static void MarkScriptFinished(ref VmPAcsIsClientSideScript.ScriptState state) =>
        state = VmPAcsIsClientSideScript.ScriptState.PleaseRemove;

    public static bool TryInternTagStringOnStack(
        List<int> stack,
        Func<int, string?> lookupString,
        Func<string, int> addGlobalString)
    {
        if (stack.Count == 0) return false;
        var index = stack[^1];
        var resolved = lookupString(index) ?? "";
        stack[^1] = addGlobalString(resolved);
        return true;
    }

    public static bool TryPushNumber(ReadOnlySpan<byte> script, ref int pc, List<int> stack)
    {
        if (pc + 4 > script.Length) return false;
        var value = script[pc] | (script[pc + 1] << 8) | (script[pc + 2] << 16) | (script[pc + 3] << 24);
        pc += 4;
        stack.Add(value);
        return true;
    }

    public static bool TryPushInlineBytes(ReadOnlySpan<byte> script, ref int pc, List<int> stack, int byteCount)
    {
        if (byteCount < 1 || pc + byteCount > script.Length) return false;
        for (var i = 0; i < byteCount; i++)
            stack.Add(script[pc + i]);
        pc += byteCount;
        return true;
    }

    public static bool TryPushCountedBytes(ReadOnlySpan<byte> script, ref int pc, List<int> stack)
    {
        if (pc >= script.Length) return false;
        var count = script[pc];
        pc += count + 1;
        if (pc > script.Length) return false;
        for (var temp = -count; temp != 0; temp++)
            stack.Add(script[pc + temp]);
        return true;
    }

    public static bool TryDup(List<int> stack, ref int sp)
    {
        if (sp < 1) return false;
        stack.Add(stack[sp - 1]);
        sp++;
        return true;
    }

    public static bool TrySwap(List<int> stack, ref int sp)
    {
        if (sp < 2) return false;
        (stack[sp - 2], stack[sp - 1]) = (stack[sp - 1], stack[sp - 2]);
        return true;
    }

    public static int MaskLineSpecialArg(int value, int specialArgMask) => value & specialArgMask;

    public static bool TryPopLspecOperands(
        List<int> stack,
        ref int sp,
        int operandCount,
        int specialArgMask,
        Span<int> args)
    {
        if (operandCount < 1 || operandCount > 5 || sp < operandCount)
            return false;
        for (var i = 0; i < operandCount; i++)
            args[operandCount - 1 - i] = MaskLineSpecialArg(stack[sp - 1 - i], specialArgMask);
        sp -= operandCount;
        return true;
    }

    public static bool ReadEmbeddedLspecNumber(ReadOnlySpan<byte> script, ref int pc, out int special)
    {
        special = 0;
        if (pc >= script.Length) return false;
        special = script[pc++];
        return true;
    }

    public static void ResetStackOnMalformedLspec(ref int sp) => sp = 0;
}
