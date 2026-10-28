using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// <c>FBehavior::LookupString</c> Hexen localization tail, typed-script dispatch, stop-my-scripts,
/// and <c>DACSThinker</c> serialize head from <c>p_acs.cpp</c> (**3305–3454**). Exclusive to chunk **965**.
/// </summary>
public static class VmPAcsBehaviorLookup965Slice
{
    public const int ScriptLightning = 12;
    public const int AcsAlways = 2;

    static readonly string[] TypedScriptTypeNames =
    {
        "Closed",
        "Open",
        "Respawn",
        "Death",
        "Enter",
        "Pickup",
        "BlueReturn",
        "RedReturn",
        "WhiteReturn",
        "Unknown",
        "Unknown",
        "Unknown",
        "Lightning",
        "Unloading",
        "Disconnect",
        "Return",
        "Event",
        "Kill",
        "Reopen",
    };

    public readonly record struct TypedScriptEntry(ushort Type, int Number);

    public readonly record struct SavingRunningscript(int ScriptNum, object? LevelScript);

    public readonly record struct TypedScriptStartCandidate(int ScriptNumber, int StartFlags, bool RunImmediately);

    public static int OldFormatStringDataOffset(ReadOnlySpan<byte> data, int stringTable, uint index)
    {
        if (!VmPAcsBehaviorLookup964Slice.OldFormatStringInRange(
                VmPAcsBehaviorLookup964Slice.AcsOld, stringTable, index, data))
            return -1;
        return BinaryPrimitives.ReadInt32LittleEndian(data.Slice(stringTable + 4 + (int)index * 4));
    }

    public static int EnhancedFormatStringDataOffset(ReadOnlySpan<byte> data, int stringTable, uint index)
    {
        if (!VmPAcsBehaviorLookup964Slice.EnhancedFormatStringInRange(
                VmPAcsBehaviorLookup964Slice.AcsOld + 1, stringTable, index, data))
            return -1;
        var rel = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(stringTable + 4 * (3 + (int)index)));
        return stringTable + rel;
    }

    public static string NormalizeHexenPrintToken(ReadOnlySpan<char> source)
    {
        var sb = new StringBuilder(source.Length);
        foreach (var raw in source)
        {
            var c = char.ToUpperInvariant(raw);
            if (IsLocalizationPunctuation(c) || c == ' ')
                continue;
            sb.Append(c);
        }

        var token = sb.ToString();
        return token.Length <= 5 ? token : token[..5];
    }

    public static string BuildHexenLocalizationLabel(string mapName, uint stringIndex, ReadOnlySpan<char> rawString) =>
        $"TXT_ACS_{mapName}_{stringIndex}_{NormalizeHexenPrintToken(rawString)}";

    public static string? ResolveOldLookupString(
        ReadOnlySpan<byte> data,
        int stringTable,
        uint index,
        bool shouldLocalize,
        bool forPrint,
        string mapName,
        Func<string, string?>? checkGlobalString)
    {
        var strOffset = OldFormatStringDataOffset(data, stringTable, index);
        if (strOffset < 0 || strOffset >= data.Length)
            return null;
        if (!TryReadCString(data, strOffset, out var raw))
            return null;

        if (shouldLocalize && forPrint && checkGlobalString != null)
        {
            var label = BuildHexenLocalizationLabel(mapName, index, raw);
            var translated = checkGlobalString(label);
            if (translated != null)
                return translated;
        }

        return raw.ToString();
    }

    public static string? ResolveEnhancedLookupString(ReadOnlySpan<byte> data, int stringTable, uint index)
    {
        var strOffset = EnhancedFormatStringDataOffset(data, stringTable, index);
        if (strOffset < 0 || !TryReadCString(data, strOffset, out var raw))
            return null;
        return raw.ToString();
    }

    public static string ResolveTypedScriptTypeName(int type) =>
        (uint)type < (uint)TypedScriptTypeNames.Length
            ? TypedScriptTypeNames[type]
            : TypedScriptTypeNames[ScriptLightning - 1];

    public static IReadOnlyList<TypedScriptStartCandidate> EnumerateModuleTypedScriptStarts(
        ushort type,
        bool always,
        bool runNow,
        IReadOnlyList<TypedScriptEntry> scripts)
    {
        var flag = always ? AcsAlways : 0;
        var starts = new List<TypedScriptStartCandidate>();
        for (var i = 0; i < scripts.Count; i++)
        {
            if (scripts[i].Type != type)
                continue;
            starts.Add(new TypedScriptStartCandidate(scripts[i].Number, flag, runNow));
        }

        return starts;
    }

    public static IReadOnlyList<object?> CollectStopMyScriptThinkers(object? serverThinker, object? clientSideThinker)
    {
        var thinkers = new List<object?>(2);
        if (serverThinker != null)
            thinkers.Add(serverThinker);
        if (clientSideThinker != null)
            thinkers.Add(clientSideThinker);
        return thinkers;
    }

    public static IReadOnlyList<string> DacsthinkerSerializeFieldKeys => new[] { "scripts", "lastscript" };

    public static bool ShouldWriteRunningScriptsArray(bool isWriting, int runningScriptsUsedCount) =>
        isWriting && runningScriptsUsedCount > 0;

    public static SavingRunningscript BuildSavingRunningscript(int scriptNum, object? levelScript) =>
        new(scriptNum, levelScript);

    public static IReadOnlyList<string> SavingRunningscriptObjectKeys => new[] { "num", "script" };

    public static IReadOnlyList<SavingRunningscript> CollectRunningScriptsForWrite(
        IEnumerable<KeyValuePair<int, object?>> pairs)
    {
        var list = new List<SavingRunningscript>();
        foreach (var pair in pairs)
        {
            if (pair.Value is null)
                continue;
            list.Add(BuildSavingRunningscript(pair.Key, pair.Value));
        }

        return list;
    }

    static bool IsLocalizationPunctuation(char c) => c is '.' or ',' or '-' or '+' or '!' or '?';

    static bool TryReadCString(ReadOnlySpan<byte> data, int start, out ReadOnlySpan<char> text)
    {
        text = ReadOnlySpan<char>.Empty;
        if (start < 0 || start >= data.Length)
            return false;
        var end = start;
        while (end < data.Length && data[end] != 0)
            end++;
        if (end >= data.Length)
            return false;
        text = Encoding.ASCII.GetString(data.Slice(start, end - start));
        return true;
    }
}
