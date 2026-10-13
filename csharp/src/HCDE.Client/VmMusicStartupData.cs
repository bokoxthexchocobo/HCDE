namespace HCDE.Client;

public sealed class VmMusicStartupData
{
    private readonly byte[]? genMidi;
    private readonly byte[]? dmxGus;
    private readonly byte[]? wgOpn;

    private VmMusicStartupData(byte[]? genMidi, byte[]? dmxGus, byte[]? wgOpn)
    {
        this.genMidi = genMidi;
        this.dmxGus = dmxGus;
        this.wgOpn = wgOpn;
    }

    public static VmMusicStartupData Load(Func<string, byte[]?> globalResource,
        Func<string, byte[]?> fullNameResource, Action<string>? warning = null)
    {
        ArgumentNullException.ThrowIfNull(globalResource);
        ArgumentNullException.ThrowIfNull(fullNameResource);
        var gen = globalResource("GENMIDI");
        byte[]? instruments = null;
        if (gen is null)
        {
            const string message = "No GENMIDI lump found. OPL playback not available.";
            if (warning is null) Console.Error.WriteLine(message); else warning(message);
        }
        else if (gen.Length >= 8 + 175 * 36 && gen.AsSpan(0, 8).SequenceEqual("#OPL_II#"u8))
            instruments = gen.AsSpan(8).ToArray();
        var gus = globalResource("DMXGUSC") ?? globalResource("DMXGUS");
        var opn = fullNameResource("xg.wopn");
        return new(instruments, gus?.ToArray(), opn?.ToArray());
    }

    public byte[]? CreateAdlGenMidiBank() => genMidi is null ? null : VmMusicGenMidiBank.ConvertToWopl(genMidi);
    public byte[]? CreateOpnDefaultBank() => wgOpn?.ToArray();

    public void Apply(Action<ReadOnlyMemory<byte>> setGenMidi, Action<ReadOnlyMemory<byte>> setDmxGus,
        Action<ReadOnlyMemory<byte>> setWgOpn)
    {
        ArgumentNullException.ThrowIfNull(setGenMidi);
        ArgumentNullException.ThrowIfNull(setDmxGus);
        ArgumentNullException.ThrowIfNull(setWgOpn);
        if (genMidi is not null) setGenMidi(genMidi.ToArray());
        if (dmxGus is not null) setDmxGus(dmxGus.ToArray());
        if (wgOpn is not null) setWgOpn(wgOpn.ToArray());
    }
}
