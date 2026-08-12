namespace HCDE.Net.Core;

public static class AuthorityEventsCodec
{
    public static bool TryPeek(ReadOnlySpan<byte> chunk) =>
        chunk.Length >= LiveConstants.AuthorityEventsHeaderSize
        && chunk[..4].SequenceEqual(LiveConstants.AuthorityEventsMagic);

    public static bool TryReadAndSkip(ReadOnlySpan<byte> chunk, ref int cursor, out string? rejectReason)
    {
        rejectReason = null;
        if (!AuthorityEventsHeader.TryRead(chunk[cursor..], out var header))
        {
            rejectReason = "missing-authority-events-header";
            return false;
        }

        if (header.ProtocolVersion != LiveConstants.AuthorityEventsProtocolVersion)
        {
            rejectReason = "authority-events-version-mismatch";
            return false;
        }

        cursor += LiveConstants.AuthorityEventsHeaderSize;
        for (var i = 0; i < header.EventCount; i++)
        {
            if (chunk.Length - cursor < 15)
            {
                rejectReason = "authority-event-truncated";
                return false;
            }

            var eventType = chunk[cursor++];
            cursor += 3;
            cursor += 4;
            cursor += 4;
            cursor += 2;
            cursor += 2;
            cursor += 2;
            var classNameLen = chunk[cursor++];
            if ((eventType == 1 || eventType == 4) && classNameLen == 0)
            {
                rejectReason = "authority-event-missing-class-name";
                return false;
            }

            if (chunk.Length - cursor < classNameLen + 56)
            {
                rejectReason = "authority-event-payload-truncated";
                return false;
            }

            cursor += classNameLen + 56;
        }

        return true;
    }
}
