using System.Runtime.InteropServices;

namespace HCDE.Net.Core;

public static class DemEventStreamConverter
{
    public static bool TryConvertToCanonical(ReadOnlySpan<byte> legacyDem, bool clientInput, Span<byte> output, out int length)
    {
        length = 0;
        if (output.Length < EventRecordsCodec.EmptyBlockSize)
            return false;

        var records = new List<EventRecord>();
        var legacyCursor = 0;
        while (legacyCursor < legacyDem.Length)
        {
            var eventType = legacyDem[legacyCursor++];
            if (!DemoCommandPolicy.IsAllowedTicEvent(eventType))
                return false;

            Span<byte> payloadBuffer = stackalloc byte[512];
            if (!CanonicalEventPayloadCodec.TryBuildFromLegacy(eventType, legacyDem, ref legacyCursor, payloadBuffer, out var payloadLength))
                return false;

            if (clientInput && !DemoCommandPolicy.IsAllowedClientInput(eventType))
                continue;

            records.Add(new EventRecord(eventType, payloadBuffer[..payloadLength].ToArray()));
        }

        length = EventRecordsCodec.Write(output, CollectionsMarshal.AsSpan(records));
        return length > 0;
    }
}
