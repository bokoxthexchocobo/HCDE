namespace HCDE.MapLoader;

public readonly struct BinaryMapBehavior
{
    public static BinaryMapBehavior Absent => new(false, MapBehaviorFormat.Unknown, 0, Array.Empty<byte>());

    public BinaryMapBehavior(bool isPresent, MapBehaviorFormat format, uint directoryOffset, byte[] data)
    {
        IsPresent = isPresent;
        Format = format;
        DirectoryOffset = directoryOffset;
        Data = data;
    }

    public bool IsPresent { get; }
    public MapBehaviorFormat Format { get; }
    public uint DirectoryOffset { get; }
    public byte[] Data { get; }
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

        behavior = new BinaryMapBehavior(true, record.Format, record.DirectoryOffset, record.Data);
        return true;
    }
}
