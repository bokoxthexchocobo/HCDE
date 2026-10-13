namespace HCDE.Client;

public static class VmMusicSmfMerge
{
    public static VmMusicSmfTrack Sequence(IReadOnlyList<VmMusicSmfTrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        var (initialTempo, hasTempo) = InitialTempo(tracks);
        var sequenced = new List<VmMusicSmfEvent>();
        foreach (var track in tracks) sequenced.AddRange(track.Events);
        return new(initialTempo, sequenced.AsReadOnly(), hasTempo);
    }

    internal static (uint Tempo, bool Present) InitialTempo(IReadOnlyList<VmMusicSmfTrack> tracks)
    {
        uint tempo = 500000;
        var present = false;
        foreach (var track in tracks)
        {
            ArgumentNullException.ThrowIfNull(track);
            if (track.HasInitialTempo) { tempo = track.InitialTempo; present = true; }
        }
        return (tempo, present);
    }

    public static VmMusicSmfTrack Merge(IReadOnlyList<VmMusicSmfTrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        var (initialTempo, hasTempo) = InitialTempo(tracks);
        return new(initialTempo, ReadMergedEvents(tracks.Select(track => (IEnumerable<VmMusicSmfEvent>)track.Events).ToArray())
            .ToList().AsReadOnly(), hasTempo);
    }

    internal static IEnumerable<VmMusicSmfEvent> ReadMergedEvents(IReadOnlyList<IEnumerable<VmMusicSmfEvent>> tracks)
    {
        var cursors = new IEnumerator<VmMusicSmfEvent>?[tracks.Count];
        var active = new bool[tracks.Count];
        var due = new ulong[tracks.Count];
        try
        {
            for (var i = 0; i < tracks.Count; i++)
            {
                var cursor = tracks[i].GetEnumerator();
                cursors[i] = cursor;
                active[i] = cursor.MoveNext();
                if (active[i]) due[i] = cursor.Current.Delay;
            }
            ulong time = 0;
            var previous = tracks.Count == 0 ? -1 : 0;
            while (true)
            {
                var selected = -1;
                // Native gives the current track precedence for further events at this tick.
                if (previous >= 0 && active[previous] && due[previous] == time)
                    selected = previous;
                else
                {
                    for (var i = 0; i < tracks.Count; i++)
                        if (active[i] && (selected < 0 || due[i] < due[selected])) selected = i;
                }
                if (selected < 0) break;
                var cursor = cursors[selected]!;
                var item = cursor.Current;
                yield return item with { Delay = checked((uint)(due[selected] - time)) };
                time = due[selected];
                active[selected] = cursor.MoveNext();
                if (active[selected]) due[selected] = checked(time + cursor.Current.Delay);
                previous = selected;
            }
        }
        finally
        {
            foreach (var cursor in cursors) cursor?.Dispose();
        }
    }
}
