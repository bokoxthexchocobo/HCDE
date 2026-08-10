using System.Buffers.Binary;

namespace HCDE.Net.Pregame;

public sealed class EngineInfoSnapshot
{
    public byte Major { get; init; } = 1;
    public byte Minor { get; init; }
    public byte Revision { get; init; }
    public IReadOnlyList<string> WadCrcs { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Minimal port of <c>Net_SetEngineInfo</c> / <c>Net_VerifyEngine</c> wire layout.
/// </summary>
public static class EngineInfoCodec
{
    public const int HeaderSize = 7;

    public static int Write(Span<byte> buffer, EngineInfoSnapshot snapshot)
    {
        if (buffer.Length < HeaderSize)
            return 0;

        buffer[0] = snapshot.Major;
        buffer[1] = snapshot.Minor;
        buffer[2] = snapshot.Revision;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[3..], (uint)snapshot.WadCrcs.Count);

        var offset = HeaderSize;
        foreach (var crc in snapshot.WadCrcs)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(crc);
            if (offset + bytes.Length + 1 > buffer.Length)
                return 0;
            bytes.CopyTo(buffer[offset..]);
            buffer[offset + bytes.Length] = 0;
            offset += bytes.Length + 1;
        }

        return offset;
    }

    public static bool TryRead(ReadOnlySpan<byte> buffer, out EngineInfoSnapshot snapshot, out int bytesRead)
    {
        snapshot = new EngineInfoSnapshot();
        bytesRead = 0;
        if (buffer.Length < HeaderSize)
            return false;

        var numWads = BinaryPrimitives.ReadUInt32BigEndian(buffer[3..]);
        var offset = HeaderSize;
        var crcs = new List<string>((int)numWads);
        for (var i = 0u; i < numWads; i++)
        {
            var start = offset;
            while (offset < buffer.Length && buffer[offset] != 0)
                offset++;
            if (offset >= buffer.Length)
                return false;
            crcs.Add(System.Text.Encoding.ASCII.GetString(buffer[start..offset]));
            offset++;
        }

        snapshot = new EngineInfoSnapshot
        {
            Major = buffer[0],
            Minor = buffer[1],
            Revision = buffer[2],
            WadCrcs = crcs,
        };
        bytesRead = offset;
        return true;
    }

    public static bool Matches(EngineInfoSnapshot expected, EngineInfoSnapshot actual) =>
        expected.Major == actual.Major
        && expected.Minor == actual.Minor
        && expected.Revision == actual.Revision
        && expected.WadCrcs.SequenceEqual(actual.WadCrcs);
}
