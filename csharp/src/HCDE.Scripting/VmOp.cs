namespace HCDE.Scripting;

/// <summary>
/// Opcode numbers from <c>src/common/scripting/vm/vmops.h</c>, in source order.
/// Only the integer core is named here. Every other byte stops the function.
/// </summary>
public enum VmOp
{
    Nop = 0,
    Li = 1,
    Lk = 2,
    Move = 77,
    Test = 90,
    Jmp = 92,
    Ret = 101,
    Reti = 102,
    SllRr = 110,
    SrlRi = 114,
    SraRi = 117,
    AddRr = 119,
    AddRk = 120,
    Addi = 121,
    SubRr = 122,
    MulRr = 125,
    DivRr = 127,
    ModRr = 133,
    AndRr = 139,
    OrRr = 141,
    XorRr = 143,
    Abs = 153,
    Neg = 154,
    Not = 155,
    EqR = 156,
    LtRr = 158,
    LeRr = 161,
}

public enum VmFault
{
    None,
    UnknownOpcode,
    ConstantRange,
    DivisionByZero,
    Overflow,
    ShiftRange,
    BadJump,
    RanOffEnd,
    Budget,
}

public readonly record struct VmResult(bool Returned, int Value, VmFault Fault);

/// <summary>
/// Packs one little-endian VM word: opcode, A, then B and C in the high half.
/// </summary>
public static class VmCode
{
    public const int RetFinal = 0x80;

    public static int Rrr(VmOp op, int a, int b, int c) =>
        (int)op | (a << 8) | (b << 16) | (c << 24);

    public static int Li(int register, short immediate) =>
        (int)VmOp.Li | (register << 8) | ((ushort)immediate << 16);

    public static int Lk(int register, int index) =>
        (int)VmOp.Lk | (register << 8) | ((index & 0xFFFF) << 16);

    public static int Addi(int dest, int source, sbyte immediate) =>
        (int)VmOp.Addi | (dest << 8) | (source << 16) | ((byte)immediate << 24);

    public static int Test(int register, ushort value) =>
        (int)VmOp.Test | (register << 8) | (value << 16);

    public static int Jmp(int offset) =>
        (int)VmOp.Jmp | ((offset & 0xFFFFFF) << 8);

    public static int Cmp(VmOp op, int expect, int left, int right) =>
        (int)op | (expect << 8) | (left << 16) | (right << 24);

    public static int RetInt(int register) =>
        (int)VmOp.Ret | (RetFinal << 8) | (register << 24);

    public static int Reti(short immediate) =>
        (int)VmOp.Reti | (RetFinal << 8) | ((ushort)immediate << 16);
}
