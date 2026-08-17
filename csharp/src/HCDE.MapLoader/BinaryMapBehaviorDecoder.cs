namespace HCDE.MapLoader;

public readonly struct BinaryMapBehavior
{
    public static BinaryMapBehavior Absent => new(
        false,
        MapBehaviorFormat.Unknown,
        0,
        Array.Empty<byte>(),
        Array.Empty<MapBehaviorScriptEntry>(),
        Array.Empty<MapBehaviorScriptBytecode>());

    public BinaryMapBehavior(
        bool isPresent,
        MapBehaviorFormat format,
        uint directoryOffset,
        byte[] data,
        IReadOnlyList<MapBehaviorScriptEntry> scripts,
        IReadOnlyList<MapBehaviorScriptBytecode> scriptBodies)
    {
        IsPresent = isPresent;
        Format = format;
        DirectoryOffset = directoryOffset;
        Data = data;
        Scripts = scripts;
        ScriptBodies = scriptBodies;
    }

    public bool IsPresent { get; }
    public MapBehaviorFormat Format { get; }
    public uint DirectoryOffset { get; }
    public byte[] Data { get; }
    public IReadOnlyList<MapBehaviorScriptEntry> Scripts { get; }
    public IReadOnlyList<MapBehaviorScriptBytecode> ScriptBodies { get; }
}

public static class BinaryMapBehaviorDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out BinaryMapBehavior behavior,
        out string? rejectReason)
    {
        behavior = BinaryMapBehavior.Absent;
        rejectReason = null;
        if (!catalog.TryGetLump(MapLumpKind.Behavior, out var behaviorLump))
            return true;

        if (!WadArchiveReader.TryReadLumpData(wad, behaviorLump.Entry, out var behaviorData, out rejectReason)
            || !MapBehaviorCodec.TryProbe(behaviorData, out var record, out rejectReason))
        {
            return false;
        }

        if (!MapBehaviorDirectoryCodec.TryReadScripts(
                record.Data,
                record.Format,
                record.DirectoryOffset,
                out var scripts,
                out rejectReason))
        {
            return false;
        }

        if (!MapBehaviorBytecodeWalker.TryWalkScripts(
                record.Data,
                record.Format,
                scripts,
                out var scriptBodies,
                out rejectReason))
        {
            return false;
        }

        behavior = new BinaryMapBehavior(
            true,
            record.Format,
            record.DirectoryOffset,
            record.Data,
            scripts,
            scriptBodies);
        return true;
    }
}
