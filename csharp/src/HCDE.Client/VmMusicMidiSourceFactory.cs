namespace HCDE.Client;

public sealed class VmMusicMidiSourceFactory
{
    public string LastError { get; private set; } = "";

    public IVmMusicSmfSource? Create(ReadOnlyMemory<byte> data, VmMusicMidiType type)
    {
        LastError = "";
        if (type is not VmMusicMidiType.Mus and not VmMusicMidiType.Mids and not VmMusicMidiType.Midi
            and not VmMusicMidiType.Xmi and not VmMusicMidiType.Hmi)
        {
            LastError = type == VmMusicMidiType.None ? "Unable to identify MIDI data"
                : $"Managed {type} source decoding is not implemented.";
            return null;
        }
        try
        {
            return type switch
            {
                VmMusicMidiType.Mus => new VmMusicMusSource(data.Span),
                VmMusicMidiType.Mids => new VmMusicMidsSource(data.Span),
                VmMusicMidiType.Xmi => new VmMusicXmiSource(data.Span),
                VmMusicMidiType.Hmi => new VmMusicHmiSource(data.Span),
                _ => new VmMusicSmfPlaybackSource(data.Span),
            };
        }
        catch (InvalidDataException error)
        {
            LastError = error.Message;
            return null;
        }
    }
}
