namespace HCDE.Client;

internal sealed class VmMusicMidiEventCursor(IEnumerable<VmMusicSmfEvent> events) : IDisposable
{
    private readonly IEnumerator<VmMusicSmfEvent> iterator = events.GetEnumerator();
    private VmMusicSmfEvent? pending;
    private bool done;
    private System.Runtime.ExceptionServices.ExceptionDispatchInfo? failure;

    internal bool CheckDone() { ReadPending(); return done; }

    internal int Write(ushort division, ref uint tempo, Span<uint> buffer, uint maximumTime, bool skipSysex)
    {
        if (buffer.Length < 3) throw new ArgumentException("MIDI buffer must hold an event.", nameof(buffer));
        var written = 0;
        ulong elapsed = 0;
        while (elapsed <= maximumTime)
        {
            ReadPending();
            if (done) break;
            var item = pending ?? throw new InvalidOperationException("Missing MIDI event.");
            var words = 3 + (skipSysex && item.Event >= 0x80000000 ? 0 : (item.Data.Length + 3) / 4);
            if (words > buffer.Length) throw new InvalidDataException("MIDI event exceeds output buffer capacity.");
            if (words > buffer.Length - written) break;
            var eventTempo = tempo;
            var index = 0;
            written += VmMusicSmfEventBuffer.Write([item], ref index, ref tempo, division,
                buffer[written..], uint.MaxValue, skipSysex);
            elapsed += (ulong)item.Delay * eventTempo / division;
            pending = null;
        }
        return written;
    }

    private void ReadPending()
    {
        failure?.Throw();
        if (pending is not null || done) return;
        try
        {
            if (iterator.MoveNext()) pending = iterator.Current;
            else done = true;
        }
        catch (Exception error)
        {
            failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error);
            throw;
        }
    }

    public void Dispose() => iterator.Dispose();
}
