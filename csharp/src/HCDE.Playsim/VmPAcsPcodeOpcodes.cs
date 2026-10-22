namespace HCDE.Playsim;

/// <summary>ACS p-code opcode constants from <c>p_acs.cpp</c> (78–469).</summary>
public static partial class VmPAcsPcodeOpcodes
{
    public const int PcodeCommandCount = 385;

    public static readonly int[] OpcodeTable = BuildOpcodeTable();

    static int[] BuildOpcodeTable()
    {
        var table = new int[PcodeCommandCount + 1];
        for (var i = 0; i < table.Length; i++)
            table[i] = i;
        table[PCD_TAGSTRING] = PCD_TAGSTRING;
        table[PCD_SETACTORPROPERTY] = PCD_SETACTORPROPERTY;
        table[PCD_GETACTORPROPERTY] = PCD_GETACTORPROPERTY;
        table[PCD_SAVESTRING] = PCD_SAVESTRING;
        table[PCD_CALLSTACK] = PCD_CALLSTACK;
        table[PCD_ASSIGNSCRIPTARRAY] = PCD_ASSIGNSCRIPTARRAY;
        table[PCD_LSPEC5EX] = PCD_LSPEC5EX;
        table[PCD_LSPEC5EXRESULT] = PCD_LSPEC5EXRESULT;
        table[PCD_TRANSLATIONRANGE4] = PCD_TRANSLATIONRANGE4;
        table[PCD_TRANSLATIONRANGE5] = PCD_TRANSLATIONRANGE5;
        return table;
    }

    public static bool IsValidOpcode(int opcode) =>
        opcode >= PCD_NOP && opcode <= PcodeCommandCount;

    public static bool IsControlFlowOpcode(int opcode) =>
        opcode is 52 or 53 or 69 or 84 or 356 or 364 or 365;

    public static bool IsInventoryOpcode(int opcode) =>
        opcode is >= 142 and <= 148 or 338 or 339 or 284 or 285 or 286 or 287;

    public static bool IsHudOpcode(int opcode) =>
        opcode is >= 159 and <= 167 or 254 or 255;

    public static bool UsesStringPool(int opcode) =>
        opcode is PCD_TAGSTRING or PCD_SAVESTRING or PCD_SETRESULTVALUE or PCD_SAVESTRING;

    public static int StackDeltaForOpcode(int opcode) =>
        opcode switch
        {
            PCD_PUSHNUMBER => 1,
            PCD_DROP => -1,
            PCD_DUP => 1,
            PCD_SWAP => 0,
            _ => 0,
        };

    public static int DecodeOpcodeName(string name) =>
        name switch
        {
            "PCD_NOP" => PCD_NOP,
            "PCD_TERMINATE" => PCD_TERMINATE,
            "PCD_TAGSTRING" => PCD_TAGSTRING,
            "PCD_SETACTORPROPERTY" => PCD_SETACTORPROPERTY,
            "PCD_SAVESTRING" => PCD_SAVESTRING,
            "PCD_CALLSTACK" => PCD_CALLSTACK,
            "PCD_LSPEC5EX" => PCD_LSPEC5EX,
            _ => -1,
        };

    public static bool IsArithmeticOpcode(int opcode) =>
        opcode is >= 14 and <= 25 or >= 34 and <= 49;

    public static bool IsArrayOpcode(int opcode) =>
        opcode is >= 207 and <= 234 or >= 366 and <= 379;

    public static bool IsDirectLspec(int opcode) =>
        opcode is 9 or 10 or 11 or 12 or 13 or 168 or 169 or 170 or 171 or 172;

    public static int LineSideConstant(int side) => side is 0 or 1 ? side : 0;

    public static int GameTypeFromFlags(int flags) =>
        flags switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 0,
        };
}
