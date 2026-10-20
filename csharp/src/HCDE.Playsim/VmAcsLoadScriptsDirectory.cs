namespace HCDE.Playsim;

/// <summary><c>FBehavior::LoadScriptsDirectory</c> from <c>p_acs.cpp</c> (2794–2991).</summary>
public static class VmAcsLoadScriptsDirectory
{
    public const int AcsOld = 0;
    public const int AcsEnhanced = 1;
    public const int AcsLittleEnhanced = 2;
    public const int LocalSize = 64;
    public const int ScriptClosed = 1;

    public static int DecodeOldScriptNumber(int packed) => packed % 1000;

    public static int DecodeOldScriptType(int packed) => packed / 1000;

    public static int OldScriptDirectoryOffset(int dataWord1) => dataWord1;

    public static int CountOldScripts(int countWord) => countWord;

    public static int CountEnhancedScriptsBy12(int size) => size / 12;

    public static int CountLittleEnhancedScriptsBy8(int size) => size / 8;

    public static bool IsAcsMagicHeader(uint header) => header == 0x00494341; // 'ACS\0'

    public static bool ShouldSkipEnhancedChunks(int format) => format == AcsOld;

    public static int InitScriptVarCount() => LocalSize;

    public static bool ShouldSortScripts(int numScripts) => numScripts > 1;

    public static bool IsDuplicateScriptNumber(int left, int right) => left == right;

    public static bool ShouldSwapClosedFirst(int leftType, int rightType, int scriptClosed) =>
        rightType == scriptClosed;

    public static int SnamIndexToNameTableIndex(int snamIndex) => -(snamIndex + 1);

    public static bool IsNamedScriptNumber(int scriptNumber) => scriptNumber < 0;

    public static int ParseSaryScriptNum(short raw) => raw;

    public static bool SaryChunkLargeEnough(int chunkSize) => chunkSize >= 6;

    public static int FlagChunkEntryCount(int chunkPayloadBytes) => chunkPayloadBytes / 4;

    public static bool LoopHasMoreFlagEntries(int remaining) => remaining > 0;

    public static int CompareScriptNumber(int a, int b) => a.CompareTo(b);

    public static bool EnhancedUsesIntermediateLayout(uint header) => !IsAcsMagicHeader(header);

    public readonly record struct OldScriptRecord(int Number, int Type, int ArgCount, int Address);

    public static OldScriptRecord MapOldScriptEntry(int packedNumber, int argCount, int address) =>
        new(DecodeOldScriptNumber(packedNumber), DecodeOldScriptType(packedNumber), argCount, address);

    public readonly record struct EnhancedScriptRecord(int Number, int Type, int ArgCount, int Address);

    public static EnhancedScriptRecord MapEnhancedScriptEntry(int number, int type, int argCount, int address) =>
        new(number, type, argCount, address);

    public static bool TryResolveDuplicateSwap(EnhancedScriptRecord left, EnhancedScriptRecord right, out bool swap)
    {
        swap = false;
        if (!IsDuplicateScriptNumber(left.Number, right.Number)) return false;
        swap = ShouldSwapClosedFirst(left.Type, right.Type, ScriptClosed);
        return true;
    }

    public static int LittleEndianToInt32(int value) => value;

    public static short LittleEndianToInt16(short value) => value;

    public static bool FindChunkPresent(byte[]? chunk) => chunk is not null;

    public static int ScriptFlagsFromChunk(ushort raw) => raw;

    public static int ScriptVarCountFromChunk(ushort raw) => raw;

    public static bool ShouldResortAfterSnam(bool anyNamed) => anyNamed;
}
