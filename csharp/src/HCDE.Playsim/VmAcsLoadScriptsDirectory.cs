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

    public static bool FindChunkPresent(byte[]? chunk) => chunk is not null;

    public static int EnhancedScriptEntrySize(bool intermediateLayout) => intermediateLayout ? 12 : 8;

    public static int ScriptDirectoryHeaderSkipWords(bool intermediateLayout) => 2;

    public static int PtrOffsetAfterCount(bool intermediateLayout) =>
        ScriptDirectoryHeaderSkipWords(intermediateLayout);

    public static bool HasScriptDirectoryChunk(byte[]? sptrChunk) => sptrChunk is not null;

    public static bool OldFormatDuplicateScanEnabled(int format) => format == AcsOld;

    public static int MaxScriptIndex(int numScripts) => numScripts - 1;

    public static bool CanCompareAdjacent(int index, int numScripts) =>
        index >= 0 && index < MaxScriptIndex(numScripts);

    public static int SflgChunkPayloadOffset() => 8;

    public static int SvctChunkPayloadOffset() => 8;

    public static int SaryScriptNumberOffset() => 4;

    public static int SnamStringTableOffset() => 8;

    public static int SnamLookupBaseIndex() => 3;

    public static int ResolveSnamStringOffset(int lookupIndex, int baseIndex) =>
        baseIndex + lookupIndex;

    public static bool ScriptPtrFound(object? ptr) => ptr is not null;

    public static int ApplyScriptFlags(int existing, int flags) => flags;

    public static int ApplyVarCount(int existing, int varCount) => varCount;

    public static bool VarCountChunkPresent(byte[]? svct) => svct is not null;

    public static bool ArrayChunkPresent(byte[]? sary) => sary is not null;

    public static bool NameChunkPresent(byte[]? snam) => snam is not null;

    public static bool ContinueSaryWalk(byte[]? next) => next is not null;

    public static void ResetScriptsDirectory(ref int numScripts) => numScripts = 0;

    public sealed class ScriptPtrSlot
    {
        public int Number { get; set; }
        public int Type { get; set; }
        public int ArgCount { get; set; }
        public int Address { get; set; }
        public int Flags { get; set; }
        public int VarCount { get; set; } = LocalSize;
    }

    public sealed class ScriptsDirectoryScratch
    {
        public List<ScriptPtrSlot> Scripts { get; } = new();
        public int NumScripts { get; set; }
        public int Format { get; set; }
    }

    public static int ResolveEnhancedScriptCount(uint lumpHeader, int sptrPayloadBytes) =>
        IsAcsMagicHeader(lumpHeader)
            ? CountLittleEnhancedScriptsBy8(sptrPayloadBytes)
            : CountEnhancedScriptsBy12(sptrPayloadBytes);

    public static bool ShouldAllocateScriptTable(int numScripts) => numScripts != 0;

    public static void CopyOldScriptRecord(ScriptPtrSlot target, OldScriptRecord source)
    {
        target.Number = source.Number;
        target.Type = source.Type;
        target.ArgCount = source.ArgCount;
        target.Address = source.Address;
    }

    public static void CopyEnhancedScriptRecord(ScriptPtrSlot target, EnhancedScriptRecord source)
    {
        target.Number = source.Number;
        target.Type = source.Type;
        target.ArgCount = source.ArgCount;
        target.Address = source.Address;
    }

    public static void PopulateOldFormatDirectory(
        ScriptsDirectoryScratch scratch,
        IReadOnlyList<OldScriptRecord> rawEntries)
    {
        scratch.Scripts.Clear();
        scratch.NumScripts = rawEntries.Count;
        if (!ShouldAllocateScriptTable(scratch.NumScripts))
            return;
        foreach (var entry in rawEntries)
        {
            var slot = new ScriptPtrSlot();
            CopyOldScriptRecord(slot, entry);
            scratch.Scripts.Add(slot);
        }
    }

    public static void PopulateEnhancedFormatDirectory(
        ScriptsDirectoryScratch scratch,
        uint lumpHeader,
        int sptrPayloadBytes,
        IReadOnlyList<EnhancedScriptRecord> rawEntries)
    {
        scratch.Scripts.Clear();
        scratch.NumScripts = ResolveEnhancedScriptCount(lumpHeader, sptrPayloadBytes);
        if (!ShouldAllocateScriptTable(scratch.NumScripts))
            return;
        var limit = Math.Min(scratch.NumScripts, rawEntries.Count);
        for (var i = 0; i < limit; i++)
        {
            var slot = new ScriptPtrSlot();
            CopyEnhancedScriptRecord(slot, rawEntries[i]);
            scratch.Scripts.Add(slot);
        }
    }

    public static void InitializeScriptDefaults(IList<ScriptPtrSlot> scripts)
    {
        for (var i = 0; i < scripts.Count; i++)
        {
            scripts[i].Flags = 0;
            scripts[i].VarCount = InitScriptVarCount();
        }
    }

    public static void SortScriptsInPlace(List<ScriptPtrSlot> scripts) =>
        scripts.Sort((a, b) => CompareScriptNumber(a.Number, b.Number));

    public static void ResolveOldFormatAdjacentDuplicates(ScriptsDirectoryScratch scratch)
    {
        if (!ShouldSortScripts(scratch.NumScripts) || !OldFormatDuplicateScanEnabled(scratch.Format))
            return;
        var scripts = scratch.Scripts;
        for (var i = 0; i < MaxScriptIndex(scratch.NumScripts); i++)
        {
            if (!CanCompareAdjacent(i, scratch.NumScripts))
                continue;
            var left = scripts[i];
            var right = scripts[i + 1];
            if (!IsDuplicateScriptNumber(left.Number, right.Number))
                continue;
            if (!ShouldSwapClosedFirst(left.Type, right.Type, ScriptClosed))
                continue;
            (scripts[i], scripts[i + 1]) = (scripts[i + 1], scripts[i]);
        }
    }

    public readonly record struct ChunkFlagEntry(short ScriptNumber, short Flags);

    public static void ApplySflgChunkEntries(
        IReadOnlyList<ChunkFlagEntry> entries,
        Dictionary<int, ScriptPtrSlot> byNumber)
    {
        foreach (var entry in entries)
        {
            if (!byNumber.TryGetValue(entry.ScriptNumber, out var ptr) || !ScriptPtrFound(ptr))
                continue;
            ptr.Flags = ApplyScriptFlags(ptr.Flags, entry.Flags);
        }
    }

    public readonly record struct ChunkVarCountEntry(short ScriptNumber, short VarCount);

    public static void ApplySvctChunkEntries(
        IReadOnlyList<ChunkVarCountEntry> entries,
        Dictionary<int, ScriptPtrSlot> byNumber)
    {
        foreach (var entry in entries)
        {
            if (!byNumber.TryGetValue(entry.ScriptNumber, out var ptr) || !ScriptPtrFound(ptr))
                continue;
            ptr.VarCount = ApplyVarCount(ptr.VarCount, entry.VarCount);
        }
    }

    public readonly record struct ChunkArrayEntry(int ScriptNumber, int ParsedVarCount);

    public static void WalkSaryChunks(
        IReadOnlyList<ChunkArrayEntry> chunks,
        Dictionary<int, ScriptPtrSlot> byNumber)
    {
        foreach (var chunk in chunks)
        {
            if (!byNumber.TryGetValue(chunk.ScriptNumber, out var ptr) || !ScriptPtrFound(ptr))
                continue;
            ptr.VarCount = chunk.ParsedVarCount;
        }
    }

    public static int SnamLookupTableIndexFromScriptNumber(int scriptNumber) =>
        -scriptNumber - 1;

    public static bool TryResolveSnamNameIndex(
        int scriptNumber,
        int lookupTableValue,
        out int stringTableOffset)
    {
        stringTableOffset = 0;
        if (!IsNamedScriptNumber(scriptNumber))
            return false;
        var lookupIndex = SnamLookupTableIndexFromScriptNumber(scriptNumber);
        stringTableOffset = ResolveSnamStringOffset(lookupIndex, SnamLookupBaseIndex()) + lookupTableValue;
        return true;
    }

    public static void ApplySnamNameIndices(
        IList<ScriptPtrSlot> scripts,
        IReadOnlyList<int> snamLookupWords,
        Func<int, int> globalNameIndexFromStringOffset)
    {
        for (var i = 0; i < scripts.Count; i++)
        {
            var number = scripts[i].Number;
            if (!IsNamedScriptNumber(number))
                continue;
            var tableIndex = SnamLookupTableIndexFromScriptNumber(number);
            if (tableIndex < 0 || tableIndex >= snamLookupWords.Count)
                continue;
            var stringOffset = ResolveSnamStringOffset(tableIndex, SnamLookupBaseIndex());
            scripts[i].Number = globalNameIndexFromStringOffset(stringOffset + snamLookupWords[tableIndex]);
        }
    }

    public static bool ShouldResortAfterSnam(bool snamPresent, int numScripts) =>
        snamPresent && ShouldSortScripts(numScripts);

    public readonly record struct LoadScriptsDirectoryInput(
        int Format,
        uint LumpHeader,
        IReadOnlyList<OldScriptRecord>? OldEntries,
        byte[]? SptrChunk,
        int SptrPayloadBytes,
        IReadOnlyList<EnhancedScriptRecord>? EnhancedEntries,
        IReadOnlyList<ChunkFlagEntry>? SflgEntries,
        IReadOnlyList<ChunkVarCountEntry>? SvctEntries,
        IReadOnlyList<ChunkArrayEntry>? SaryEntries,
        IReadOnlyList<int>? SnamLookupWords,
        Func<int, int>? GlobalNameIndexFromStringOffset);

    public static void RunLoadScriptsDirectory(
        ScriptsDirectoryScratch scratch,
        LoadScriptsDirectoryInput input)
    {
        scratch.Format = input.Format;
        scratch.Scripts.Clear();
        scratch.NumScripts = 0;

        switch (input.Format)
        {
            case AcsOld:
                if (input.OldEntries is not null)
                    PopulateOldFormatDirectory(scratch, input.OldEntries);
                break;
            case AcsEnhanced:
            case AcsLittleEnhanced:
                if (!HasScriptDirectoryChunk(input.SptrChunk))
                    break;
                if (input.EnhancedEntries is not null)
                    PopulateEnhancedFormatDirectory(
                        scratch,
                        input.LumpHeader,
                        input.SptrPayloadBytes,
                        input.EnhancedEntries);
                break;
        }

        InitializeScriptDefaults(scratch.Scripts);
        if (ShouldSortScripts(scratch.NumScripts))
            SortScriptsInPlace(scratch.Scripts);
        ResolveOldFormatAdjacentDuplicates(scratch);

        if (ShouldSkipEnhancedChunks(scratch.Format))
            return;

        var byNumber = scratch.Scripts.ToDictionary(s => s.Number, s => s);
        if (input.SflgEntries is not null)
            ApplySflgChunkEntries(input.SflgEntries, byNumber);

        if (input.SvctEntries is not null)
            ApplySvctChunkEntries(input.SvctEntries, byNumber);

        if (input.SaryEntries is not null)
            WalkSaryChunks(input.SaryEntries, byNumber);

        if (input.SnamLookupWords is not null && input.GlobalNameIndexFromStringOffset is not null)
        {
            ApplySnamNameIndices(scratch.Scripts, input.SnamLookupWords, input.GlobalNameIndexFromStringOffset);
            if (ShouldResortAfterSnam(true, scratch.NumScripts))
                SortScriptsInPlace(scratch.Scripts);
        }
    }
}
