using HCDE.MapLoader;

namespace HCDE.Client;

internal static class VmWadScriptVisibility
{
    public static bool[] GetGlobalEntries(IReadOnlyList<WadLumpEntry> entries)
    {
        var global = Enumerable.Repeat(true, entries.Count).ToArray();
        if (entries.Any(entry => entry.Name.StartsWith("S_SKIN", StringComparison.OrdinalIgnoreCase)))
        {
            Array.Fill(global, false);
            return global;
        }
        foreach (var prefix in new[] { "S", "F", "C", "A", "TX", "V", "HI", "VX" })
        {
            var markers = new List<(int Index, bool Start)>();
            for (var index = 0; index < entries.Count; index++)
            {
                if (IsMarker(entries[index].Name, prefix + "_START")) markers.Add((index, true));
                else if (IsMarker(entries[index].Name, prefix + "_END")) markers.Add((index, false));
            }
            var position = 0;
            while (position < markers.Count)
            {
                if (!markers[position].Start) { position++; continue; }
                var start = markers[position++].Index;
                while (position < markers.Count && markers[position].Start) position++;
                while (position + 1 < markers.Count && !markers[position].Start && !markers[position + 1].Start) position++;
                // Native unmatched starts extend to EOF; marker endpoints themselves stay global.
                var end = position < markers.Count ? markers[position++].Index : entries.Count;
                for (var index = start + 1; index < end; index++)
                    if (global[index] && (prefix != "S" || entries[index].Size >= 8)) global[index] = false;
            }
        }
        return global;
    }

    private static bool IsMarker(string name, string marker) =>
        name.Equals(marker, StringComparison.OrdinalIgnoreCase)
        || (marker[1] == '_' && name.Equals(marker[0] + marker, StringComparison.OrdinalIgnoreCase));
}
