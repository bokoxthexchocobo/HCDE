using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// <c>FBehavior::GetFunction</c>, name/array lookup, chunk walk, and string lookup head from
/// <c>p_acs.cpp</c> (**3155–3304**). Exclusive to chunk **964**.
/// </summary>
public static class VmPAcsBehaviorLookup964Slice
{
    public const int LibraryIdShift = 20;
    public const int StrpoolLibraryId = int.MaxValue >> LibraryIdShift;
    public const int AcsOld = 0;
    public const uint FnamFourCc = 0x4D414E46;
    public const uint MexpFourCc = 0x5058454D;

    public readonly record struct ScriptFunctionDef(int Address, int ImportNum);

    public readonly record struct ArrayInfo(int ArraySize, int[] Elements);

    public readonly record struct GetFunctionResult(bool Found, int? ImportModuleIndex, int ResolvedAddress, bool SelfModule);

    public static bool FunctionIndexInRange(int funcNum, int numFunctions) =>
        (uint)funcNum < (uint)numFunctions;

    public static GetFunctionResult ResolveGetFunction(
        int funcNum,
        int numFunctions,
        ScriptFunctionDef func,
        IReadOnlyList<object?> imports,
        Func<int, object?, GetFunctionResult>? importGetFunction)
    {
        if (!FunctionIndexInRange(funcNum, numFunctions))
            return new GetFunctionResult(false, null, 0, false);

        if (func.ImportNum != 0)
        {
            var importIndex = func.ImportNum - 1;
            if ((uint)importIndex >= (uint)imports.Count || imports[importIndex] == null)
                return new GetFunctionResult(false, null, 0, false);
            if (importGetFunction != null)
                return importGetFunction(func.Address, imports[importIndex]);
            return new GetFunctionResult(true, importIndex, func.Address, false);
        }

        return new GetFunctionResult(true, null, func.Address, true);
    }

    public static int FindStringInChunk(ReadOnlySpan<byte> namesChunk, ReadOnlySpan<char> varName)
    {
        if (namesChunk.Length < 12)
            return -1;
        var count = BinaryPrimitives.ReadInt32LittleEndian(namesChunk.Slice(8));
        const int stringBase = 8;
        for (var i = 0; i < count; i++)
        {
            var ofsIndex = 12 + i * 4;
            if (ofsIndex + 4 > namesChunk.Length)
                break;
            var strOfs = BinaryPrimitives.ReadInt32LittleEndian(namesChunk.Slice(ofsIndex));
            var strStart = stringBase + strOfs;
            if (TryReadChunkCString(namesChunk, strStart, out var existing)
                && existing.Equals(varName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static int ResolveMapArrayIndex(int mapVarIndex, IReadOnlyList<int> mapVarStore)
    {
        if (mapVarIndex < 0 || mapVarIndex >= mapVarStore.Count)
            return -1;
        return mapVarStore[mapVarIndex];
    }

    public static int GetArrayVal(int arrayNum, int index, int numTotalArrays, IReadOnlyList<ArrayInfo?> arrays)
    {
        if ((uint)arrayNum >= (uint)numTotalArrays)
            return 0;
        if (arrays[arrayNum] is not { } array || (uint)index >= (uint)array.ArraySize)
            return 0;
        return array.Elements[index];
    }

    public static bool SetArrayVal(int arrayNum, int index, int value, int numTotalArrays, ArrayInfo?[] arrays)
    {
        if ((uint)arrayNum >= (uint)numTotalArrays)
            return false;
        if (arrays[arrayNum] is not { } array || (uint)index >= (uint)array.ArraySize)
            return false;
        array.Elements[index] = value;
        return true;
    }

    public static bool CopyStringToArray(
        int arrayNum,
        int index,
        int maxLength,
        ReadOnlySpan<char> chars,
        int numTotalArrays,
        ArrayInfo?[] arrays)
    {
        if ((uint)arrayNum >= (uint)numTotalArrays || index < 0)
            return false;
        if (arrays[arrayNum] is not { } array)
            return false;

        var room = array.ArraySize - index;
        if (room < maxLength)
            maxLength = room;

        var charIndex = 0;
        while (maxLength-- > 0)
        {
            var ch = charIndex < chars.Length ? chars[charIndex++] : '\0';
            array.Elements[index++] = ch;
            if (ch == 0)
                return true;
        }

        return charIndex >= chars.Length || chars[charIndex] == '\0';
    }

    public static int FindChunkOffset(ReadOnlySpan<byte> data, int dataSize, uint chunkId, int startOffset = 0)
    {
        var cursor = startOffset;
        while (cursor >= 0 && cursor + 8 <= data.Length && cursor < dataSize)
        {
            var id = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(cursor));
            if (id == chunkId)
                return cursor;
            var size = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(cursor + 4));
            cursor += size + 8;
        }

        return -1;
    }

    public static int NextChunkOffset(ReadOnlySpan<byte> data, int dataSize, int chunkOffset)
    {
        if (chunkOffset < 0 || chunkOffset + 8 > data.Length)
            return -1;
        var id = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(chunkOffset));
        var size = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(chunkOffset + 4));
        var cursor = chunkOffset + size + 8;
        while (cursor >= 0 && cursor + 8 <= data.Length && cursor < dataSize)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(cursor)) == id)
                return cursor;
            size = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(cursor + 4));
            cursor += size + 8;
        }

        return -1;
    }

    public static bool LookupUsesStringPool(uint index) => (index >> LibraryIdShift) == StrpoolLibraryId;

    public static bool LookupModuleInRange(uint index, int moduleCount) =>
        (index >> LibraryIdShift) < (uint)moduleCount;

    public static int LookupModuleIndex(uint index) => (int)(index >> LibraryIdShift);

    public static int LookupStringIndex(uint index) => (int)(index & 0xffff);

    public static bool OldFormatStringInRange(int format, int stringTable, uint index, ReadOnlySpan<byte> data)
    {
        if (format != AcsOld || stringTable == 0)
            return false;
        if (stringTable + 4 > data.Length)
            return false;
        var count = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(stringTable));
        return index < (uint)count;
    }

    public static bool EnhancedFormatStringInRange(int format, int stringTable, uint index, ReadOnlySpan<byte> data)
    {
        if (format == AcsOld || stringTable == 0)
            return false;
        if (stringTable + 8 > data.Length)
            return false;
        var count = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(stringTable + 4));
        return index < (uint)count;
    }

    static bool TryReadChunkCString(ReadOnlySpan<byte> chunk, int start, out ReadOnlySpan<char> text)
    {
        text = ReadOnlySpan<char>.Empty;
        if (start < 0 || start >= chunk.Length)
            return false;
        var end = start;
        while (end < chunk.Length && chunk[end] != 0)
            end++;
        if (end >= chunk.Length)
            return false;
        text = Encoding.ASCII.GetString(chunk.Slice(start, end - start));
        return true;
    }
}
