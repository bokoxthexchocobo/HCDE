using System.Buffers.Binary;

namespace HCDE.Playsim;

/// <summary>
/// <c>FBehavior::SortScripts</c>, string-table codecs, <c>IsGood</c>, and <c>FindScript</c> from
/// <c>p_acs.cpp</c> (**2993–3142**). Exclusive to chunk **963**; orchestration stays in
/// <c>VmPAcsBehaviorInit</c> (injected host callbacks only).
/// </summary>
public static class VmPAcsBehaviorStringValidate963Slice
{
    public const uint StreFourCc = 0x45545253; // STRE
    public const uint StrlFourCc = 0x4C525453; // STRL
    public const int AcsUnknown = 0;

    public readonly record struct ScriptPtrSlot(int Number, int Address);

    public readonly record struct FunctionDef(int Address, int ImportNum);

    public readonly record struct BehaviorGoodCheck(
        int Format,
        IReadOnlyList<FunctionDef> Functions,
        IReadOnlyList<object?> Imports,
        string ModuleName,
        IReadOnlyList<string>? ImportFunctionNames);

    public static int CompareScriptNumbers(int a, int b) => a.CompareTo(b);

    public static int SortScriptsComparison(ScriptPtrSlot a, ScriptPtrSlot b) =>
        CompareScriptNumbers(a.Number, b.Number);

    /// <summary>Decrypt STRE payloads in place and retag chunk headers to STRL (native loop).</summary>
    public static int UnencryptStringChunks(Span<byte> data, Func<uint, int, byte[]?> findChunk, Func<byte[], byte[]?>? nextChunk)
    {
        var converted = 0;
        byte[]? chunk = findChunk(StreFourCc, data.Length);
        while (chunk != null)
        {
            if (chunk.Length >= 16)
            {
                var count = BinaryPrimitives.ReadInt32LittleEndian(chunk.AsSpan(12));
                for (var strnum = 0; strnum < count; strnum++)
                {
                    var ofsIndex = 20 + strnum * 4;
                    if (ofsIndex + 4 > chunk.Length)
                        break;
                    var ofs = BinaryPrimitives.ReadInt32LittleEndian(chunk.AsSpan(ofsIndex));
                    var dataIndex = 8 + ofs;
                    var p = (byte)(ofs * 157135);
                    var i = 0;
                    while (dataIndex + i < chunk.Length)
                    {
                        chunk[dataIndex + i] ^= (byte)(p + (i >> 1));
                        if (chunk[dataIndex + i] == 0)
                            break;
                        i++;
                    }
                }

                BinaryPrimitives.WriteUInt32LittleEndian(chunk.AsSpan(0), StrlFourCc);
                converted++;
            }

            var next = nextChunk?.Invoke(chunk);
            chunk = next;
        }

        return converted;
    }

    public static void UnescapeStringTable(Span<byte> chunkStart, Span<byte> dataBase, bool hasPadding)
    {
        if (!hasPadding)
        {
            if (chunkStart.Length < 4)
                return;
            var count = BinaryPrimitives.ReadInt32LittleEndian(chunkStart);
            for (var strnum = 0; strnum < count; strnum++)
            {
                var ofsIndex = 4 + strnum * 4;
                if (ofsIndex + 4 > chunkStart.Length)
                    break;
                var ofs = BinaryPrimitives.ReadInt32LittleEndian(chunkStart.Slice(ofsIndex, 4));
                BinaryPrimitives.WriteInt32LittleEndian(chunkStart.Slice(ofsIndex, 4), ofs);
                UnescapeStrBinInPlace(dataBase, ofs);
            }
        }
        else
        {
            if (chunkStart.Length < 8)
                return;
            var count = BinaryPrimitives.ReadInt32LittleEndian(chunkStart.Slice(4));
            for (var strnum = 0; strnum < count; strnum++)
            {
                var ofsIndex = 12 + strnum * 4;
                if (ofsIndex + 4 > chunkStart.Length)
                    break;
                var ofs = BinaryPrimitives.ReadInt32LittleEndian(chunkStart.Slice(ofsIndex, 4));
                BinaryPrimitives.WriteInt32LittleEndian(chunkStart.Slice(ofsIndex, 4), ofs);
                UnescapeStrBinInPlace(chunkStart.Slice(8), ofs);
            }
        }
    }

    public static void UnescapeStrBinInPlace(Span<byte> data, int offset)
    {
        if (offset < 0 || offset >= data.Length)
            return;
        var read = offset;
        var write = offset;
        while (read < data.Length)
        {
            var c = data[read++];
            if (c != (byte)'\\')
            {
                data[write++] = c;
                continue;
            }

            if (read >= data.Length)
                break;
            var esc = data[read++];
            byte outCh = esc switch
            {
                (byte)'a' => (byte)'\a',
                (byte)'b' => (byte)'\b',
                (byte)'c' => 0x1c,
                (byte)'f' => (byte)'\f',
                (byte)'n' => (byte)'\n',
                (byte)'t' => (byte)'\t',
                (byte)'r' => (byte)'\r',
                (byte)'v' => (byte)'\v',
                (byte)'?' => (byte)'?',
                (byte)'\n' => 0,
                _ => esc,
            };
            if (outCh != 0)
                data[write++] = outCh;
        }

        data[write] = 0;
    }

    public static bool IsGood(in BehaviorGoodCheck check)
    {
        if (check.Format == AcsUnknown)
            return false;

        for (var i = 0; i < check.Functions.Count; i++)
        {
            var func = check.Functions[i];
            if (func.Address == 0 && func.ImportNum == 0)
                return false;
        }

        for (var i = check.Imports.Count - 1; i >= 0; i--)
        {
            if (check.Imports[i] == null)
                return false;
        }

        return true;
    }

    public static int? FindScriptIndex(IReadOnlyList<ScriptPtrSlot> scripts, int scriptNumber)
    {
        if (scripts.Count == 0)
            return null;
        var idx = BinarySearchScriptNumber(scripts, scriptNumber);
        if (idx < 0)
            return null;
        if (idx > 0 && scripts[idx - 1].Number == scriptNumber)
            idx--;
        return scripts[idx].Number == scriptNumber ? idx : null;
    }

    public static int? FindScriptModuleIndex(IReadOnlyList<IReadOnlyList<ScriptPtrSlot>> modules, int scriptNumber)
    {
        for (var i = 0; i < modules.Count; i++)
        {
            if (FindScriptIndex(modules[i], scriptNumber) != null)
                return i;
        }

        return null;
    }

    static int BinarySearchScriptNumber(IReadOnlyList<ScriptPtrSlot> scripts, int scriptNumber)
    {
        var lo = 0;
        var hi = scripts.Count - 1;
        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            var cmp = CompareScriptNumbers(scripts[mid].Number, scriptNumber);
            if (cmp == 0)
                return mid;
            if (cmp < 0)
                lo = mid + 1;
            else
                hi = mid - 1;
        }

        return -1;
    }
}
