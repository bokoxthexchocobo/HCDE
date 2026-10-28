namespace HCDE.MapLoader;

/// <summary>Native <c>FWadFile::Open</c> namespace and skin logic from <c>file_wad.cpp</c>.</summary>
public static class WadArchiveNamespaceAssigner
{
    private static int _nextSkinNamespace = (int)WadLumpNamespace.FirstSkin;

    public static void ResetSkinNamespaceCounter() => _nextSkinNamespace = (int)WadLumpNamespace.FirstSkin;

    public static void AssignNamespaces(
        WadLumpEntry[] entries,
        WadLumpMetadata[] metadata,
        IWadArchiveMessageSink? sink = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(metadata);
        if (metadata.Length != entries.Length)
            throw new ArgumentException("Metadata length must match lump count.");

        SetNamespace(entries, metadata, "s_start", "s_end", WadLumpNamespace.Sprites, sink);
        SetNamespace(entries, metadata, "f_start", "f_end", WadLumpNamespace.Flats, sink, flatHack: true);
        SetNamespace(entries, metadata, "c_start", "c_end", WadLumpNamespace.Colormaps, sink);
        SetNamespace(entries, metadata, "a_start", "a_end", WadLumpNamespace.AcsLibrary, sink);
        SetNamespace(entries, metadata, "tx_start", "tx_end", WadLumpNamespace.NewTextures, sink);
        SetNamespace(entries, metadata, "v_start", "v_end", WadLumpNamespace.StrifeVoices, sink);
        SetNamespace(entries, metadata, "hi_start", "hi_end", WadLumpNamespace.Hires, sink);
        SetNamespace(entries, metadata, "vx_start", "vx_end", WadLumpNamespace.Voxels, sink);
        ApplySkinHack(entries, metadata, sink);
    }

    public static bool IsMarker(string lumpName, string marker) =>
        lumpName.Length > 0 && marker.Length > 0 &&
        char.ToUpperInvariant(lumpName[0]) == char.ToUpperInvariant(marker[0]) &&
        (string.Equals(lumpName, marker, StringComparison.OrdinalIgnoreCase) ||
         (marker.Length > 1 && marker[1] == '_' &&
          lumpName.AsSpan(1).Equals(marker.AsSpan(), StringComparison.OrdinalIgnoreCase)));

    private static void SetNamespace(
        WadLumpEntry[] entries,
        WadLumpMetadata[] metadata,
        string startMarker,
        string endMarker,
        WadLumpNamespace space,
        IWadArchiveMessageSink? sink,
        bool flatHack = false)
    {
        var markers = new List<(bool IsEnd, int Index)>();
        for (var index = 0; index < entries.Length; index++)
        {
            if (IsMarker(entries[index].Name, startMarker))
                markers.Add((false, index));
            else if (IsMarker(entries[index].Name, endMarker))
                markers.Add((true, index));
        }

        if (markers.Count == 0 || markers.All(marker => marker.IsEnd))
        {
            if (flatHack && markers.Any(marker => marker.IsEnd))
            {
                var end = markers[^1].Index;
                for (var lump = 0; lump < end; lump++)
                {
                    if (entries[lump].Size == 4096)
                        metadata[lump].MaybeFlat = true;
                }
            }

            return;
        }

        var markerIndex = 0;
        var warnedOverlap = false;
        while (markerIndex < markers.Count)
        {
            if (markers[markerIndex].IsEnd)
            {
                sink?.Warn($"{endMarker} marker without corresponding {startMarker} found.");
                markerIndex++;
                continue;
            }

            var start = markerIndex++;
            while (markerIndex < markers.Count && !markers[markerIndex].IsEnd)
            {
                sink?.Warn($"duplicate {startMarker} marker found.");
                markerIndex++;
            }

            while (markerIndex < markers.Count - 1 && markers[markerIndex].IsEnd && markers[markerIndex + 1].IsEnd)
            {
                sink?.Warn($"duplicate {endMarker} marker found.");
                markerIndex++;
            }

            var end = markerIndex >= markers.Count ? entries.Length : markers[markerIndex++].Index;
            for (var lump = markers[start].Index + 1; lump < end; lump++)
            {
                if (metadata[lump].Namespace != WadLumpNamespace.Global)
                {
                    if (!warnedOverlap)
                        sink?.Warn($"Overlapping namespaces found (lump {lump})");
                    warnedOverlap = true;
                    continue;
                }

                if (space == WadLumpNamespace.Sprites && entries[lump].Size < 8)
                    continue;
                metadata[lump].Namespace = space;
            }
        }
    }

    private static void ApplySkinHack(
        WadLumpEntry[] entries,
        WadLumpMetadata[] metadata,
        IWadArchiveMessageSink? sink)
    {
        var skinned = false;
        var hasMap = false;
        for (var index = 0; index < entries.Length; index++)
        {
            var name = entries[index].Name;
            if (name.StartsWith("s_skin", StringComparison.Ordinal))
            {
                var entry = entries[index];
                entries[index] = new WadLumpEntry(entry.FilePosition, entry.Size, "s_skin");
                if (!skinned)
                {
                    skinned = true;
                    var skinNamespace = (WadLumpNamespace)_nextSkinNamespace++;
                    for (var lump = 0; lump < metadata.Length; lump++)
                        metadata[lump].Namespace = skinNamespace;
                }
            }

            if (IsDoomMapName(name))
                hasMap = true;
        }

        if (skinned && hasMap)
            sink?.Attention("The maps will not be loaded because it has a skin.");
    }

    private static bool IsDoomMapName(string name)
    {
        if (name.Length == 6 &&
            name[0] == 'M' && name[1] == 'A' && name[2] == 'P' &&
            name[3] is >= '0' and <= '9' &&
            name[4] is >= '0' and <= '9')
            return true;
        return name.Length == 5 &&
               name[0] == 'E' && name[1] is >= '0' and <= '9' &&
               name[2] == 'M' && name[3] is >= '0' and <= '9';
    }
}

public interface IWadArchiveMessageSink
{
    void Warn(string message);
    void Attention(string message);
}
