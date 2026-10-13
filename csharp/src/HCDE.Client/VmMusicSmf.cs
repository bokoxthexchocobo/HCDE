using System.Buffers.Binary;

namespace HCDE.Client;

// The packed event word uses ZMusic's high-byte type and low 24-bit payload.
public readonly record struct VmMusicSmfEvent(uint Delay, uint Event, ReadOnlyMemory<byte> Data = default);

public static class VmMusicSmf
{
    public static byte[] Serialize(ushort division, uint initialTempo, IEnumerable<VmMusicSmfEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        var file = new List<byte>
        {
            77, 84, 104, 100, 0, 0, 0, 6, 0, 0, 0, 1,
            (byte)(division >> 8), (byte)division,
            77, 84, 114, 107, 0, 0, 0, 0,
            0, 255, 81, 3, (byte)(initialTempo >> 16), (byte)(initialTempo >> 8), (byte)initialTempo,
        };
        uint delay = 0;
        byte runningStatus = 255;
        foreach (var item in events)
        {
            delay = checked(delay + item.Delay);
            var type = item.Event >> 24;
            var payload = item.Event & 0xffffff;
            switch (type)
            {
                case 1:
                    WriteVariable(file, delay);
                    delay = 0;
                    file.AddRange([255, 81, 3, (byte)(payload >> 16), (byte)(payload >> 8), (byte)payload]);
                    runningStatus = 255;
                    break;
                case 128:
                    if (payload == 0 || payload != item.Data.Length)
                        throw new ArgumentException("Long MIDI event length must match a nonempty payload.", nameof(events));
                    WriteVariable(file, delay);
                    delay = 0;
                    var bytes = item.Data.Span;
                    var sysex = bytes[0] == 240;
                    file.Add(sysex ? (byte)240 : (byte)247);
                    WriteVariable(file, payload - (sysex ? 1u : 0u));
                    foreach (var value in bytes[(sysex ? 1 : 0)..]) file.Add(value);
                    runningStatus = 255;
                    break;
                case 0:
                    var status = (byte)payload;
                    if (status is < 128 or >= 240)
                        throw new ArgumentException("Short MIDI event requires a channel status byte.", nameof(events));
                    WriteVariable(file, delay);
                    delay = 0;
                    if (status != runningStatus)
                    {
                        file.Add(status);
                        runningStatus = status;
                    }
                    file.Add((byte)((payload >> 8) & 127));
                    if ((status >> 4) is not 12 and not 13) file.Add((byte)((payload >> 16) & 127));
                    break;
            }
        }
        WriteVariable(file, delay);
        file.AddRange([255, 47, 0]);
        var result = file.ToArray();
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(18, 4), (uint)(result.Length - 22));
        return result;
    }

    private static void WriteVariable(List<byte> file, uint value)
    {
        if (value > 0x0fffffff) throw new ArgumentOutOfRangeException(nameof(value), "MIDI variable values are limited to 28 bits.");
        Span<byte> bytes = stackalloc byte[4];
        var offset = 3;
        bytes[offset] = (byte)(value & 127);
        while ((value >>= 7) != 0) bytes[--offset] = (byte)((value & 127) | 128);
        foreach (var item in bytes[offset..]) file.Add(item);
    }
}
