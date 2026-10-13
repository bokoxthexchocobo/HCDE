namespace HCDE.Client;

public sealed class VmMusicWildMidiInstrumentCatalog
{
    private readonly Dictionary<ushort, (VmMusicWildMidiInstrumentPatch Patch, string FileName)> entries = [];
    private readonly Func<string, byte[]?> readResource;
    public bool AutomaticAmplification { get; private set; }
    public bool CombineAmplification { get; private set; }
    public bool FixRelease { get; private set; }
    public VmMusicWildMidiReverbSettings Reverb { get; } = new();

    internal void EnableReleaseFix() => FixRelease = true;

    internal void EnableAutomaticAmplification(bool combine)
    {
        AutomaticAmplification = true;
        CombineAmplification |= combine;
    }

    public VmMusicWildMidiInstrumentCatalog(Func<string, byte[]?> readResource)
    {
        ArgumentNullException.ThrowIfNull(readResource);
        this.readResource = readResource;
    }

    public VmMusicWildMidiInstrumentPatch Register(ushort id, string fileName, byte note = 0)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        var patch = new VmMusicWildMidiInstrumentPatch(id, note);
        entries.Add(id, (patch, fileName));
        return patch;
    }

    public VmMusicWildMidiInstrumentPatch RegisterConfigured(ushort id, string name, VmMusicWildMidiConfigPaths paths, byte note = 0,
        byte keep = 0, byte remove = 0, int amplification = 1024, IReadOnlyList<int?>? envelopeTargets = null,
        IReadOnlyList<float?>? envelopeTimes = null)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if ((keep & ~68) != 0) throw new ArgumentOutOfRangeException(nameof(keep));
        if ((remove & ~160) != 0) throw new ArgumentOutOfRangeException(nameof(remove));
        if (amplification < 0) throw new ArgumentOutOfRangeException(nameof(amplification));
        var targets = envelopeTargets?.ToArray() ?? new int?[6];
        if (targets.Length != 6 || targets.Any(value => value is < 0 or > 4194240))
            throw new ArgumentException("Envelope target overrides require six valid stages.", nameof(envelopeTargets));
        var fileName = paths.ResolvePatch(name);
        var times = envelopeTimes?.ToArray() ?? new float?[6];
        if (times.Length != 6 || times.Any(value => value.HasValue && (!float.IsFinite(value.Value) || value.Value < 1.47f || value.Value > 45000f)))
            throw new ArgumentException("Envelope time overrides require six valid stages.", nameof(envelopeTimes));
        if (!entries.TryGetValue(id, out var entry))
        {
            var patch = Register(id, fileName, note);
            patch.ConfigureNote(note, keep, remove, amplification, targets, times);
            return patch;
        }
        entry.Patch.ConfigureNote(note, keep, remove, amplification, targets, times);
        entries[id] = (entry.Patch, fileName);
        return entry.Patch;
    }

    public VmMusicWildMidiInstrumentPatch? FindExact(ushort id) => entries.TryGetValue(id, out var entry) ? entry.Patch : null;

    public int LoadSamples(VmMusicWildMidiInstrumentPatch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);
        if (!entries.TryGetValue(patch.Id, out var entry) || !ReferenceEquals(entry.Patch, patch))
            throw new ArgumentException("The patch is not owned by this catalog.", nameof(patch));
        if (patch.Loaded) return 0;
        var bytes = readResource(entry.FileName) ?? throw new FileNotFoundException("The GUS patch resource was not found.", entry.FileName);
        if (bytes.Length > 0x1fffffff) throw new InvalidDataException("The GUS patch resource exceeds the native file size limit.");
        patch.Load(bytes, AutomaticAmplification, CombineAmplification);
        return 0;
    }
}
