namespace HCDE.MapLoader;

public readonly struct MapLumpDescriptor
{
    public MapLumpDescriptor(MapLumpKind kind, WadLumpEntry entry)
    {
        Kind = kind;
        Entry = entry;
    }

    public MapLumpKind Kind { get; }
    public WadLumpEntry Entry { get; }
}

public readonly struct MapLumpCatalog
{
    public MapLumpCatalog(string mapName, MapDataFormat format, MapLumpDescriptor[] lumps)
    {
        MapName = mapName;
        Format = format;
        Lumps = lumps;
    }

    public string MapName { get; }
    public MapDataFormat Format { get; }
    public IReadOnlyList<MapLumpDescriptor> Lumps { get; }

    public bool TryGetLump(MapLumpKind kind, out MapLumpDescriptor descriptor)
    {
        foreach (var lump in Lumps)
        {
            if (lump.Kind == kind)
            {
                descriptor = lump;
                return true;
            }
        }

        descriptor = default;
        return false;
    }
}

public static class MapLumpCatalogReader
{
    public static bool TryReadMap(
        ReadOnlySpan<byte> wad,
        string mapName,
        out MapLumpCatalog catalog,
        out string? rejectReason)
    {
        catalog = default;
        rejectReason = null;
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out rejectReason))
            return false;

        var labelIndex = -1;
        for (var i = 0; i < entries.Length; i++)
        {
            if (string.Equals(entries[i].Name, mapName, StringComparison.OrdinalIgnoreCase))
            {
                labelIndex = i;
                break;
            }
        }

        if (labelIndex < 0)
        {
            rejectReason = "map-label-not-found";
            return false;
        }

        if (labelIndex + 1 >= entries.Length)
        {
            rejectReason = "map-has-no-lumps";
            return false;
        }

        var lumps = new List<MapLumpDescriptor>();
        var format = MapDataFormat.Unknown;
        var cursor = labelIndex + 1;
        while (cursor < entries.Length)
        {
            var entry = entries[cursor];
            if (IsMapLabel(entry.Name))
                break;

            if (string.Equals(entry.Name, MapLumpNames.Textmap, StringComparison.OrdinalIgnoreCase))
            {
                format = MapDataFormat.UdmfText;
                lumps.Add(new MapLumpDescriptor(MapLumpKind.Things, entry));
                cursor++;
                continue;
            }

            if (TryResolveBinaryLump(entry.Name, out var kind))
            {
                if (format == MapDataFormat.Unknown)
                    format = MapDataFormat.DoomBinary;
                lumps.Add(new MapLumpDescriptor(kind, entry));
                cursor++;
                continue;
            }

            break;
        }

        if (lumps.Count == 0)
        {
            rejectReason = "map-lumps-not-found";
            return false;
        }

        if (format == MapDataFormat.Unknown)
            format = MapDataFormat.DoomBinary;

        catalog = new MapLumpCatalog(mapName, format, lumps.ToArray());
        return true;
    }

    private static bool IsMapLabel(string name)
    {
        if (name.Length is < 4 or > 8)
            return false;

        if (name.StartsWith("MAP", StringComparison.OrdinalIgnoreCase)
            && name.Length == 5
            && char.IsDigit(name[3])
            && char.IsDigit(name[4]))
        {
            return true;
        }

        if (name.Length == 4
            && char.ToUpperInvariant(name[0]) is 'E' or 'M'
            && char.IsDigit(name[1])
            && char.ToUpperInvariant(name[2]) is 'M' or 'L'
            && char.IsDigit(name[3]))
        {
            return true;
        }

        return false;
    }

    private static bool TryResolveBinaryLump(string name, out MapLumpKind kind)
    {
        for (var i = 0; i < MapLumpNames.BinaryMapLumpOrder.Length; i++)
        {
            if (string.Equals(name, MapLumpNames.BinaryMapLumpOrder[i], StringComparison.OrdinalIgnoreCase))
            {
                kind = (MapLumpKind)(i + 1);
                return true;
            }
        }

        kind = default;
        return false;
    }
}
