namespace HCDE.Scripting;

/// <summary>
/// Opcode numbers from <c>src/common/scripting/vm/vmops.h</c>, in source order.
/// Only implemented instructions are named here. Every other byte stops the function.
/// </summary>
public enum VmOp
{
    Nop = 0,
    Li = 1,
    Lk = 2,
    Lkf = 3,
    Lks = 4,
    Lkp = 5,
    LkR = 6,
    LkfR = 7,
    LksR = 8,
    LkpR = 9,
    LoadFramePointer = 10,
    Meta = 11,
    Class = 12,
    LoadByte = 13,
    LoadByteR = 14,
    LoadHalf = 15,
    LoadHalfR = 16,
    LoadWord = 17,
    LoadWordR = 18,
    LoadUnsignedByte = 19,
    LoadUnsignedByteR = 20,
    LoadUnsignedHalf = 21,
    LoadUnsignedHalfR = 22,
    LoadFloat = 23,
    LoadFloatR = 24,
    LoadDouble = 25,
    LoadDoubleR = 26,
    LoadString = 27,
    LoadStringR = 28,
    LoadObject = 29,
    LoadObjectR = 30,
    LoadPointer = 31,
    LoadPointerR = 32,
    LoadVector2 = 33,
    LoadVector2R = 34,
    LoadVector3 = 35,
    LoadVector3R = 36,
    LoadVector4 = 37,
    LoadVector4R = 38,
    LoadCString = 39,
    LoadCStringR = 40,
    LoadFloatVector2 = 41,
    LoadFloatVector2R = 42,
    LoadFloatVector3 = 43,
    LoadFloatVector3R = 44,
    LoadFloatVector4 = 45,
    LoadFloatVector4R = 46,
    LoadBit = 47,
    StoreByte = 48,
    StoreByteR = 49,
    StoreHalf = 50,
    StoreHalfR = 51,
    StoreWord = 52,
    StoreWordR = 53,
    StoreFloat = 54,
    StoreFloatR = 55,
    StoreDouble = 56,
    StoreDoubleR = 57,
    StoreString = 58,
    StoreStringR = 59,
    StorePointer = 60,
    StorePointerR = 61,
    StoreObject = 62,
    StoreObjectR = 63,
    StoreVector2 = 64,
    StoreVector2R = 65,
    StoreVector3 = 66,
    StoreVector3R = 67,
    StoreVector4 = 68,
    StoreVector4R = 69,
    StoreFloatVector2 = 70,
    StoreFloatVector2R = 71,
    StoreFloatVector3 = 72,
    StoreFloatVector3R = 73,
    StoreFloatVector4 = 74,
    StoreFloatVector4R = 75,
    StoreBit = 76,
    Move = 77,
    MoveF = 78,
    MoveS = 79,
    MoveA = 80,
    MoveV2 = 81,
    MoveV3 = 82,
    MoveV4 = 83,
    Cast = 84,
    CastB = 85,
    DynCastR = 86,
    DynCastK = 87,
    DynCastClassR = 88,
    DynCastClassK = 89,
    Test = 90,
    TestN = 91,
    Jmp = 92,
    IJmp = 93,
    Param = 94,
    ParamImmediate = 95,
    Call = 96,
    CallK = 97,
    Virtual = 98,
    Scope = 99,
    Result = 100,
    Ret = 101,
    Reti = 102,
    Throw = 103,
    Bound = 104,
    BoundK = 105,
    BoundR = 106,
    Concat = 107,
    LenS = 108,
    CmpS = 109,
    SllRr = 110,
    SllRi = 111,
    SllKr = 112,
    SrlRr = 113,
    SrlRi = 114,
    SrlKr = 115,
    SraRr = 116,
    SraRi = 117,
    SraKr = 118,
    AddRr = 119,
    AddRk = 120,
    Addi = 121,
    SubRr = 122,
    SubRk = 123,
    SubKr = 124,
    MulRr = 125,
    MulRk = 126,
    DivRr = 127,
    DivRk = 128,
    DivKr = 129,
    DivuRr = 130,
    DivuRk = 131,
    DivuKr = 132,
    ModRr = 133,
    ModRk = 134,
    ModKr = 135,
    ModuRr = 136,
    ModuRk = 137,
    ModuKr = 138,
    AndRr = 139,
    AndRk = 140,
    OrRr = 141,
    OrRk = 142,
    XorRr = 143,
    XorRk = 144,
    MinRr = 145,
    MinRk = 146,
    MaxRr = 147,
    MaxRk = 148,
    MinuRr = 149,
    MinuRk = 150,
    MaxuRr = 151,
    MaxuRk = 152,
    Abs = 153,
    Neg = 154,
    Not = 155,
    EqR = 156,
    EqK = 157,
    LtRr = 158,
    LtRk = 159,
    LtKr = 160,
    LeRr = 161,
    LeRk = 162,
    LeKr = 163,
    LtuRr = 164,
    LtuRk = 165,
    LtuKr = 166,
    LeuRr = 167,
    LeuRk = 168,
    LeuKr = 169,
    AddfRr = 170,
    AddfRk = 171,
    SubfRr = 172,
    SubfRk = 173,
    SubfKr = 174,
    MulfRr = 175,
    MulfRk = 176,
    DivfRr = 177,
    DivfRk = 178,
    DivfKr = 179,
    ModfRr = 180,
    ModfRk = 181,
    ModfKr = 182,
    PowfRr = 183,
    PowfRk = 184,
    PowfKr = 185,
    MinfRr = 186,
    MinfRk = 187,
    MaxfRr = 188,
    MaxfRk = 189,
    Atan2 = 190,
    Flop = 191,
    EqfR = 192,
    EqfK = 193,
    LtfRr = 194,
    LtfRk = 195,
    LtfKr = 196,
    LefRr = 197,
    LefRk = 198,
    LefKr = 199,
    NegV2 = 200,
    AddV2 = 201,
    SubV2 = 202,
    DotV2 = 203,
    MulVf2Rr = 204,
    MulVf2Rk = 205,
    DivVf2Rr = 206,
    DivVf2Rk = 207,
    LenV2 = 208,
    EqV2R = 209,
    EqV2K = 210,
    NegV3 = 211,
    AddV3 = 212,
    SubV3 = 213,
    DotV3 = 214,
    CrossV = 215,
    MulVf3Rr = 216,
    MulVf3Rk = 217,
    DivVf3Rr = 218,
    DivVf3Rk = 219,
    LenV3 = 220,
    EqV3R = 221,
    EqV3K = 222,
    NegV4 = 223,
    AddV4 = 224,
    SubV4 = 225,
    DotV4 = 226,
    MulVf4Rr = 227,
    MulVf4Rk = 228,
    DivVf4Rr = 229,
    DivVf4Rk = 230,
    LenV4 = 231,
    EqV4R = 232,
    EqV4K = 233,
    MulQv3 = 234,
    MulQq = 235,
    ConjQ = 236,
    AddPointerR = 237,
    AddPointerK = 238,
    SubPointer = 239,
    EqaR = 240,
    EqaK = 241,
    NullCheck = 242,
}

/// <summary>Native FLOP selectors from vmintern.h.</summary>
public enum VmFloatFunction
{
    Abs, Neg, Exp, Log, Log10, Sqrt, Ceil, Floor,
    Acos, Asin, Atan, Cos, Sin, Tan,
    AcosDegrees, AsinDegrees, AtanDegrees, CosDegrees, SinDegrees, TanDegrees,
    Cosh, Sinh, Tanh, Round,
}

/// <summary>Implemented native CAST and CASTB selectors from vmintern.h.</summary>
public enum VmCast
{
    IntToFloat = 0,
    IntToString = 1,
    UnsignedToFloat = 2,
    UnsignedToString = 3,
    FloatToInt = 4,
    FloatToUnsigned = 5,
    FloatToString = 6,
    StringToInt = 8,
    StringToFloat = 9,
    StringToColor = 12,
    ColorToString = 14,
    Vector2ToString = 16,
    Vector3ToString = 17,
    IntToBool = 21,
    FloatToBool = 22,
    ObjectToBool = 23,
    StringToBool = 24,
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
    ArrayBounds,
    InvalidFloatFunction,
    InvalidCast,
    RegisterRange,
    InvalidComparison,
    ScriptAbort,
    InvalidColor,
    InvalidClass,
    MemoryRange,
    InvalidMemory,
    InvalidFrame,
    InvalidCall,
    ParameterRange,
}

public readonly record struct VmVectorResult(int Components, double X, double Y, double Z, double W);

public readonly record struct VmAbortFrame(string FunctionName, string SourceFileName, int InstructionIndex, int SourceLine)
{
    public bool IsNative { get; init; }
}

public readonly record struct VmMultiResult(bool Returned, VmFault Fault, IReadOnlyList<VmResult> Values)
{
    public string? AbortMessage => Fault == VmFault.ScriptAbort && AbortCode is { } reason ? VmAbortDiagnostics.FormatMessage(reason, AbortContext) : null;
    public string? AbortContext { get; init; }
    public string? AbortStackTrace => AbortFrames is { } frames ? VmAbortDiagnostics.FormatStackTrace(frames) : null;
    public ushort? AbortCode { get; init; }
    public IReadOnlyList<VmAbortFrame>? AbortFrames { get; init; }
}

public readonly record struct VmResult(bool Returned, int Value, VmFault Fault)
{
    public string? AbortMessage => Fault == VmFault.ScriptAbort && AbortCode is { } reason ? VmAbortDiagnostics.FormatMessage(reason, AbortContext) : null;
    public string? AbortContext { get; init; }
    public string? AbortStackTrace => AbortFrames is { } frames ? VmAbortDiagnostics.FormatStackTrace(frames) : null;
    public double? FloatValue { get; init; }
    public VmVectorResult? VectorValue { get; init; }
    public string? StringValue { get; init; }
    public bool HasObjectValue { get; init; }
    public object? ObjectValue { get; init; }
    public ushort? AbortCode { get; init; }
    public IReadOnlyList<VmAbortFrame>? AbortFrames { get; init; }
}

/// <summary>
/// Packs one little-endian VM word: opcode, A, then B and C in the high half.
/// </summary>
public static class VmCode
{
    public const int RetFinal = 0x80;
    public const int CompareApprox = 32;

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

    public static int RetFloat(int register) => Rrr(VmOp.Ret, RetFinal, 1, register);

    public static int RetObject(int source, bool constant = false) => Rrr(VmOp.Ret, RetFinal, constant ? 7 : 3, source);

    public static int RetVector(int register, int components, bool constant = false) =>
        Rrr(VmOp.Ret, RetFinal, 1 | (constant ? 4 : 0) | (components switch
        {
            2 => 8, 3 => 16, 4 => 64,
            _ => throw new ArgumentOutOfRangeException(nameof(components)),
        }), register);

    public static int Reti(short immediate) =>
        (int)VmOp.Reti | (RetFinal << 8) | ((ushort)immediate << 16);
}
