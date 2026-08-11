using System.Buffers.Binary;
using System.Text;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public enum VerificationErrorKind : byte
{
    None = 0,
    Engine = 1,
    FileUnknown = 2,
    FileMissing = 3,
    FileOrder = 4,
}

public sealed class VerificationErrorPacket
{
    public VerificationErrorKind Kind { get; init; }
    public byte HostMajor { get; init; }
    public byte HostMinor { get; init; }
    public byte HostRevision { get; init; }
    public byte GuestMajor { get; init; }
    public byte GuestMinor { get; init; }
    public byte GuestRevision { get; init; }
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
}

/// <summary>
/// PRE_VERIFICATION_ERROR encode/decode matching <c>SendVerificationError</c>.
/// </summary>
public static class VerificationErrorCodec
{
    public const int EnginePayloadSize = 9;
    public const int FileListHeaderSize = 7;

    public static int Write(Span<byte> netBuffer, VerificationErrorPacket error)
    {
        if (netBuffer.Length < 3)
            return 0;

        netBuffer[0] = (byte)NetCommandFlags.Setup;
        netBuffer[1] = (byte)PregameSetupType.VerificationError;
        netBuffer[2] = (byte)error.Kind;

        if (error.Kind == VerificationErrorKind.Engine)
        {
            if (netBuffer.Length < EnginePayloadSize)
                return 0;
            netBuffer[3] = error.HostMajor;
            netBuffer[4] = error.HostMinor;
            netBuffer[5] = error.HostRevision;
            netBuffer[6] = error.GuestMajor;
            netBuffer[7] = error.GuestMinor;
            netBuffer[8] = error.GuestRevision;
            return EnginePayloadSize;
        }

        var offset = FileListHeaderSize;
        foreach (var file in error.Files)
        {
            var bytes = Encoding.ASCII.GetBytes(file);
            if (offset + bytes.Length + 1 > netBuffer.Length)
                break;
            bytes.CopyTo(netBuffer[offset..]);
            netBuffer[offset + bytes.Length] = 0;
            offset += bytes.Length + 1;
        }

        var count = (uint)error.Files.Count;
        BinaryPrimitives.WriteUInt32BigEndian(netBuffer[3..], count);
        return offset;
    }

    public static bool TryRead(ReadOnlySpan<byte> netBuffer, out VerificationErrorPacket packet)
    {
        packet = new VerificationErrorPacket();
        if (netBuffer.Length < 3)
            return false;
        if (netBuffer[0] != (byte)NetCommandFlags.Setup)
            return false;
        if (netBuffer[1] != (byte)PregameSetupType.VerificationError)
            return false;

        var kind = (VerificationErrorKind)netBuffer[2];
        if (kind == VerificationErrorKind.Engine)
        {
            if (netBuffer.Length < EnginePayloadSize)
                return false;
            packet = new VerificationErrorPacket
            {
                Kind = kind,
                HostMajor = netBuffer[3],
                HostMinor = netBuffer[4],
                HostRevision = netBuffer[5],
                GuestMajor = netBuffer[6],
                GuestMinor = netBuffer[7],
                GuestRevision = netBuffer[8],
            };
            return true;
        }

        if (netBuffer.Length < FileListHeaderSize)
            return false;

        var count = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[3..]);
        var files = new List<string>((int)count);
        var offset = FileListHeaderSize;
        for (var i = 0u; i < count; i++)
        {
            if (!ProtocolStreamCodec.TryReadNullTerminatedString(netBuffer, ref offset, out var file))
                return false;
            files.Add(file);
        }

        packet = new VerificationErrorPacket { Kind = kind, Files = files };
        return true;
    }

    public static VerificationErrorPacket FromEngineVerification(
        EngineVerificationResult result,
        EngineInfoSnapshot hostEngine,
        EngineInfoSnapshot guestEngine)
    {
        return result.Error switch
        {
            EngineVerificationError.FileMissing => new VerificationErrorPacket
            {
                Kind = VerificationErrorKind.FileMissing,
                Files = result.MissingFiles.ToArray(),
            },
            EngineVerificationError.FileUnknown => new VerificationErrorPacket
            {
                Kind = VerificationErrorKind.FileUnknown,
                Files = result.UnknownFiles.ToArray(),
            },
            EngineVerificationError.FileOrder => new VerificationErrorPacket
            {
                Kind = VerificationErrorKind.FileOrder,
                Files = hostEngine.WadCrcs.ToArray(),
            },
            _ => new VerificationErrorPacket
            {
                Kind = VerificationErrorKind.Engine,
                HostMajor = hostEngine.Major,
                HostMinor = hostEngine.Minor,
                HostRevision = hostEngine.Revision,
                GuestMajor = guestEngine.Major,
                GuestMinor = guestEngine.Minor,
                GuestRevision = guestEngine.Revision,
            },
        };
    }
}
