using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct LiveControlCapabilities
{
    public LiveControlCapabilities(ulong capabilityMask, uint? sessionId = null)
    {
        CapabilityMask = capabilityMask;
        SessionId = sessionId;
    }

    public ulong CapabilityMask { get; }
    public uint? SessionId { get; }

    public int WireSize =>
        LiveConstants.ControlCapabilitiesMinSize
        + (SessionId.HasValue ? 4 : 0);

    public static bool TryRead(ReadOnlySpan<byte> payloadAtCapabilitiesOffset, out LiveControlCapabilities capabilities)
    {
        capabilities = default;
        if (payloadAtCapabilitiesOffset.Length < LiveConstants.ControlCapabilitiesMinSize)
            return false;
        if (!payloadAtCapabilitiesOffset[..4].SequenceEqual(LiveConstants.ControlCapabilitiesMagic))
            return false;

        var version = payloadAtCapabilitiesOffset[4];
        var flags = (LiveControlCapabilityFlags)payloadAtCapabilitiesOffset[5];
        if (version == 0 || version > LiveConstants.ControlCapabilitiesVersion)
            return false;
        if ((flags & ~LiveControlCapabilityFlags.SessionId) != 0)
            return false;

        var mask = BinaryPrimitives.ReadUInt64BigEndian(payloadAtCapabilitiesOffset[6..]);
        uint? sessionId = null;
        if ((flags & LiveControlCapabilityFlags.SessionId) != 0)
        {
            if (payloadAtCapabilitiesOffset.Length < LiveConstants.ControlCapabilitiesFullSize)
                return false;
            sessionId = BinaryPrimitives.ReadUInt32BigEndian(payloadAtCapabilitiesOffset[14..]);
        }

        capabilities = new LiveControlCapabilities(mask, sessionId);
        return true;
    }

    public static int Write(Span<byte> payloadAtCapabilitiesOffset, LiveControlCapabilities capabilities)
    {
        var wireSize = LiveConstants.ControlCapabilitiesMinSize + (capabilities.SessionId.HasValue ? 4 : 0);
        if (payloadAtCapabilitiesOffset.Length < wireSize)
            return 0;

        LiveConstants.ControlCapabilitiesMagic.CopyTo(payloadAtCapabilitiesOffset);
        payloadAtCapabilitiesOffset[4] = LiveConstants.ControlCapabilitiesVersion;
        payloadAtCapabilitiesOffset[5] = capabilities.SessionId.HasValue
            ? (byte)LiveControlCapabilityFlags.SessionId
            : (byte)LiveControlCapabilityFlags.None;
        BinaryPrimitives.WriteUInt64BigEndian(payloadAtCapabilitiesOffset[6..], capabilities.CapabilityMask);
        if (capabilities.SessionId is uint sessionId)
            BinaryPrimitives.WriteUInt32BigEndian(payloadAtCapabilitiesOffset[14..], sessionId);
        return wireSize;
    }
}

public readonly struct LiveControlBasePayload
{
    public LiveControlBasePayload(uint gameTic, byte consolePlayer, byte maxClients)
    {
        GameTic = gameTic;
        ConsolePlayer = consolePlayer;
        MaxClients = maxClients;
    }

    public uint GameTic { get; }
    public byte ConsolePlayer { get; }
    public byte MaxClients { get; }

    public static bool TryRead(ReadOnlySpan<byte> payload, out LiveControlBasePayload basePayload)
    {
        basePayload = default;
        if (payload.Length < LiveConstants.ControlBasePayloadSize)
            return false;

        var gameTic = BinaryPrimitives.ReadUInt32BigEndian(payload);
        basePayload = new LiveControlBasePayload(gameTic, payload[4], payload[5]);
        return true;
    }

    public static int Write(Span<byte> payload, LiveControlBasePayload basePayload)
    {
        if (payload.Length < LiveConstants.ControlBasePayloadSize)
            return 0;

        BinaryPrimitives.WriteUInt32BigEndian(payload, basePayload.GameTic);
        payload[4] = basePayload.ConsolePlayer;
        payload[5] = basePayload.MaxClients;
        return LiveConstants.ControlBasePayloadSize;
    }
}

public static class LiveControlPayloadCodec
{
    public static bool TryRead(ReadOnlySpan<byte> payload, out LiveControlBasePayload basePayload, out LiveControlCapabilities? capabilities)
    {
        basePayload = default;
        capabilities = null;
        if (!LiveControlBasePayload.TryRead(payload, out basePayload))
            return false;

        if (payload.Length < LiveConstants.ControlMinPayloadSize)
            return true;

        if (!LiveControlCapabilities.TryRead(payload[LiveConstants.ControlBasePayloadSize..], out var parsed))
            return true;

        capabilities = parsed;
        return true;
    }

    public static int Write(
        Span<byte> payload,
        LiveControlBasePayload basePayload,
        LiveControlCapabilities? capabilities = null)
    {
        var written = LiveControlBasePayload.Write(payload, basePayload);
        if (written == 0)
            return 0;

        if (capabilities is null)
            return written;

        var capWritten = LiveControlCapabilities.Write(
            payload[LiveConstants.ControlBasePayloadSize..],
            capabilities.Value);
        return capWritten == 0 ? 0 : written + capWritten;
    }
}
