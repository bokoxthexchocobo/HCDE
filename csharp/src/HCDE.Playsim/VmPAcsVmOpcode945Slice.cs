namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> logical/bitwise ops, unary ops, <c>PCD_IFNOTGOTO</c>,
/// <c>PCD_LINESIDE</c>, script-wait opcodes, and start of <c>PCD_CLEARLINESPECIAL</c> from
/// <c>p_acs.cpp</c> (**8700–8849**). Exclusive to chunk **945**; full <c>AcsVm</c> dispatch is not matrix credit.
/// </summary>
public static class VmPAcsVmOpcode945Slice
{
    public const int MalformedStackDepth = 2;

    public static bool BinaryStackReady(int sp) => sp >= MalformedStackDepth;

    public static int AndLogical(int left, int right) => (left != 0 && right != 0) ? 1 : 0;

    public static int OrLogical(int left, int right) => (left != 0 || right != 0) ? 1 : 0;

    public static int AndBitwise(int left, int right) => left & right;

    public static int OrBitwise(int left, int right) => left | right;

    public static int EorBitwise(int left, int right) => left ^ right;

    public static int NegateLogical(int value) => value == 0 ? 1 : 0;

    public static int NegateBinary(int value) => ~value;

    public static int LeftShift(int left, int right) => left << right;

    public static int RightShift(int left, int right) => left >> right;

    public static int UnaryMinus(int value) => -value;

    public static void NegateLogicalInPlace(Span<int> stack, ref int sp) =>
        stack[sp - 1] = NegateLogical(stack[sp - 1]);

    public static void PopBinary(Span<int> stack, ref int sp, Func<int, int, int> combine)
    {
        var right = stack[sp - 1];
        var left = stack[sp - 2];
        stack[sp - 2] = combine(left, right);
        sp--;
    }

    public static bool IfNotGotoBranches(int condition) => condition == 0;

    public static bool IfNotGotoAdvancesPcInline(int condition) => condition != 0;

    public static int LineSideStackValue(bool backSide) => backSide ? 1 : 0;

    public static VmPAcsIsClientSideScript.ScriptState ResolveScriptWaitState(bool targetScriptRunning) =>
        targetScriptRunning
            ? VmPAcsIsClientSideScript.ScriptState.ScriptWait
            : VmPAcsIsClientSideScript.ScriptState.ScriptWaitPre;

    public static (int Statedata, VmPAcsIsClientSideScript.ScriptState State) ResolveScriptWaitDirect(
        bool compatScriptWaitFlagSet,
        int pcWord,
        bool targetScriptRunning)
    {
        if (!compatScriptWaitFlagSet)
            return (pcWord, ResolveScriptWaitState(targetScriptRunning));
        return (pcWord, VmPAcsIsClientSideScript.ScriptState.ScriptWait);
    }

    public static int EncodeScriptWaitNamedKey(int nameIndex) => -nameIndex;

    public static bool ClearLineSpecialHasActivationLine(object? activationLine) => activationLine is not null;

    public static int ClearedLineSpecialValue => 0;

    public static VmPAcsIsClientSideScript.ScriptState MalformedOpcodeState() =>
        VmPAcsIsClientSideScript.ScriptState.PleaseRemove;

    public static VmPAcsIsClientSideScript.ScriptState AdvanceAfterMalformed(
        VmPAcsIsClientSideScript.ScriptState current,
        bool malformed) =>
        malformed ? MalformedOpcodeState() : current;
}
