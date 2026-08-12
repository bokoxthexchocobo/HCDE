using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public static class ServerSnapshotQuitterCodec
{
    public static int Write(Span<byte> chunk, ReadOnlySpan<byte> quitterPlayerSlots)
    {
        if (quitterPlayerSlots.Length == 0 || quitterPlayerSlots.Length > byte.MaxValue)
            return 0;

        var required = quitterPlayerSlots.Length + 1;
        if (chunk.Length < required)
            return 0;

        chunk[0] = (byte)quitterPlayerSlots.Length;
        quitterPlayerSlots.CopyTo(chunk[1..]);
        return required;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        ushort quitterBytes,
        out byte[] quitterPlayerSlots,
        out string? rejectReason)
    {
        quitterPlayerSlots = Array.Empty<byte>();
        rejectReason = null;

        if (quitterBytes == 0)
            return true;

        if (chunk.Length < quitterBytes)
        {
            rejectReason = "server-snapshot-quitter-truncated";
            return false;
        }

        var count = chunk[0];
        if (count + 1 != quitterBytes)
        {
            rejectReason = "server-snapshot-quitter-length-mismatch";
            return false;
        }

        if (count > NetConstants.MaxPlayers)
        {
            rejectReason = "server-snapshot-invalid-quitter-slot";
            return false;
        }

        for (var i = 0; i < count; i++)
        {
            if (chunk[i + 1] >= NetConstants.MaxPlayers)
            {
                rejectReason = "server-snapshot-invalid-quitter-slot";
                return false;
            }
        }

        quitterPlayerSlots = chunk.Slice(1, count).ToArray();
        return true;
    }
}
