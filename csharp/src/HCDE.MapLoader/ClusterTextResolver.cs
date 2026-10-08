using System.Text;
using HCDE.Gamedata;

namespace HCDE.MapLoader;

/// <summary>Single-WAD text resolution from native F_StartFinale; rendering and localization are separate.</summary>
public static class ClusterTextResolver
{
    public static bool TryResolveTransition(ReadOnlySpan<byte> wad, MapInfoSet metadata,
        string currentMap, string nextMap, bool deathmatch, out ClusterTransitionText? result, out string? error,
        bool secretExit = false)
    {
        result = null; error = null;
        var current = metadata.FindMap(currentMap); var next = metadata.FindMap(nextMap);
        if (current is null || next is null) { error = "cluster-transition-map-missing"; return false; }
        if (deathmatch) return true;
        if (current.ExitTexts.TryGetValue(secretExit ? "Secret" : "Normal", out var exitText)
            || current.ExitTexts.TryGetValue(nextMap, out exitText))
        {
            if (exitText.HasText && exitText.Text.Length != 0)
                result = new ClusterTransitionText(current.Cluster, false,
                    exitText.Lookup ? "$" + exitText.Text : exitText.Text,
                    (exitText.Background ?? metadata.FinaleFlat) is { Length: > 0 } background ? background : "-",
                    exitText.BackgroundIsPicture, exitText.Music ?? metadata.FinaleMusic,
                    exitText.Music is null ? metadata.FinaleMusicOrder : exitText.MusicOrder, -1);
            return true;
        }
        if (current.NoClusterText || current.Cluster == next.Cluster) return true;
        var entering = metadata.FindCluster(next.Cluster);
        var leaving = metadata.FindCluster(current.Cluster);
        var useEntry = entering is { EnterText.Length: > 0 };
        var selected = useEntry ? entering : leaving;
        if (selected is null || !useEntry && selected.ExitText.Length == 0) return true;
        if (!TryResolve(wad, selected, useEntry, out var text, out error)) return false;
        result = new ClusterTransitionText(selected.Cluster, useEntry, text,
            selected.Flat.Length == 0 ? "-" : selected.Flat, selected.FinaleIsPic, selected.Music, selected.MusicOrder,
            selected.CdTrack, entering?.CdId ?? 0);
        return true;
    }

    public static bool TryResolve(ReadOnlySpan<byte> wad, MapInfoCluster cluster, bool entering,
        out string text, out string? error)
    {
        var value = entering ? cluster.EnterText : cluster.ExitText;
        var inLump = entering ? cluster.EnterTextIsLump : cluster.ExitTextIsLump;
        var lookup = entering ? cluster.LookupEnterText : cluster.LookupExitText;
        text = ""; error = null;
        if (value.Length == 0) return true;
        if (!inLump) { text = lookup ? "$" + value : value; return true; }
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out error)) return false;
        for (var i = entries.Length - 1; i > 0; i--)
        {
            if (!entries[i].Name.Equals(value, StringComparison.OrdinalIgnoreCase)) continue;
            if (!WadArchiveReader.TryReadLumpData(wad, entries[i], out var bytes, out error)) return false;
            var zero = bytes.IndexOf((byte)0);
            text = Encoding.UTF8.GetString(zero < 0 ? bytes : bytes[..zero]);
            return true;
        }
        text = $"Unknown text lump '{value}'";
        return true;
    }
}

public sealed record ClusterTransitionText(int Cluster, bool Entering, string Text,
    string Background, bool BackgroundIsPicture, string Music, int MusicOrder = 0, int CdTrack = 0, uint CdId = 0);
