using System.Buffers.Binary;

namespace HCDE.Client;

public interface IVmMusicMidiSysexPolicy
{
    bool SkipSysex { get; set; }
}

public interface IVmMusicSmfSource : IDisposable
{
    bool SetSubsong(int subsong) => subsong == 0;
    ushort Division { get; }
    uint InitialTempo { get; }
    int LoopLimit { get; set; }
    void CheckCapabilities(int technology);
    void Restart();
    void StartPlayback(bool looped, int loopLimit);
    bool CheckDone();
    int MakeEvents(Span<uint> buffer, uint maximumTime);
}

public static class VmMusicSmfSource
{
    public const int BufferWords = 128 * 3;
    public const uint MaximumTime = 1000000 * 600;

    public static byte[] Create(IVmMusicSmfSource source, int loopLimit)
    {
        ArgumentNullException.ThrowIfNull(source);
        source.CheckCapabilities(1);
        source.LoopLimit = loopLimit <= 0 ? 30 : loopLimit;
        try
        {
            source.Restart();
            source.StartPlayback(false, source.LoopLimit);
            return VmMusicSmf.Serialize(source.Division, source.InitialTempo, ReadEvents(source));
        }
        finally
        {
            source.LoopLimit = 0;
        }
    }

    internal static IEnumerable<VmMusicSmfEvent> ReadEvents(IVmMusicSmfSource source)
    {
        var buffer = new uint[BufferWords];
        while (!source.CheckDone())
        {
            var count = source.MakeEvents(buffer, MaximumTime);
            if (count < 0 || count > buffer.Length)
                throw new InvalidDataException("MIDI source returned an invalid event buffer length.");
            foreach (var item in Decode(buffer, count)) yield return item;
        }
    }

    private static List<VmMusicSmfEvent> Decode(uint[] buffer, int count)
    {
        var events = new List<VmMusicSmfEvent>();
        for (var position = 0; position < count;)
        {
            if (count - position < 3) throw new InvalidDataException("Truncated MIDI event header.");
            var delay = buffer[position];
            var word = buffer[position + 2];
            position += 3;
            byte[] data = [];
            if (word >= 0x80000000)
            {
                var length = (int)(word & 0xffffff);
                var words = (length + 3) / 4;
                if (words > count - position) throw new InvalidDataException("Truncated long MIDI event.");
                data = new byte[words * 4];
                for (var i = 0; i < words; i++)
                    BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(i * 4, 4), buffer[position + i]);
                position += words;
                if (data.Length != length) Array.Resize(ref data, length);
            }
            events.Add(new(delay, word, data));
        }
        return events;
    }
}
