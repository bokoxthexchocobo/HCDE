namespace HCDE.Client;

public sealed class VmReplayGainCache
{
    private readonly Dictionary<string, float> gains = new(StringComparer.Ordinal);
    private bool read;

    public void ReadOnce(Func<IEnumerable<KeyValuePair<string, float>>> load)
    {
        ArgumentNullException.ThrowIfNull(load);
        if (read) return;
        read = true;
        foreach (var entry in load()) gains[entry.Key] = entry.Value;
    }

    public bool TryGet(string hash, out float decibels) => gains.TryGetValue(hash, out decibels);

    public void Store(string hash, float decibels)
    {
        ArgumentNullException.ThrowIfNull(hash);
        gains[hash] = decibels;
    }

    public void Save(Action<IReadOnlyList<KeyValuePair<string, string>>> writeGainsSection)
    {
        ArgumentNullException.ThrowIfNull(writeGainsSection);
        var entries = gains.Select(entry => new KeyValuePair<string, string>(entry.Key,
            VmMusicNumberFormat.Persistence(entry.Value))).ToArray();
        writeGainsSection(Array.AsReadOnly(entries));
    }

    public float SetManualGain(string hash, float decibels,
        Action<IReadOnlyList<KeyValuePair<string, string>>> writeGainsSection, Action<float> setMusicVolume)
    {
        ArgumentNullException.ThrowIfNull(writeGainsSection);
        return VmReplayGain.SetManualGain(hash, decibels, Store, () => Save(writeGainsSection), setMusicVolume);
    }
}
