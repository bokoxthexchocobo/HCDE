using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

/// <summary><c>I_QueryServerInfo</c> client flow (<c>i_net.cpp</c> 1468–1548).</summary>
public static class VmINetQueryClient
{
    public static bool QueryServerInfoNative(
        string addrName,
        ServerQuerySnapshotDecodeTarget snapshot,
        Action<string>? setError,
        Func<string, string?, bool>? tryBuildAddress,
        Func<bool>? ensureWsa,
        Func<byte[], bool>? sendRequest,
        Func<int>? waitForReply,
        Func<byte[], int>? receiveReply,
        Func<byte[], int, ServerQuerySnapshotDecodeTarget, Action<string>?, bool>? tryReadSnapshot)
    {
        snapshot.HostName = "";
        setError?.Invoke("");

        if (ensureWsa is not null && !ensureWsa())
        {
            setError?.Invoke("Couldn't initialize Windows sockets");
            return false;
        }

        if (tryBuildAddress is null || !tryBuildAddress(addrName, null))
            return false;

        var request = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(request, (uint)NetConstants.LauncherChallenge);
        if (sendRequest is null || !sendRequest(request))
        {
            setError?.Invoke("Failed to send query");
            return false;
        }

        var wait = waitForReply?.Invoke() ?? 0;
        if (wait <= 0)
        {
            setError?.Invoke(wait == 0 ? "Query timed out" : "Query wait failed");
            return false;
        }

        var reply = new byte[NetConstants.MaxMessageLength];
        var replySize = receiveReply?.Invoke(reply) ?? 0;
        if (replySize <= 0)
        {
            setError?.Invoke("Failed to read query reply");
            return false;
        }

        return tryReadSnapshot?.Invoke(reply, replySize, snapshot, setError) ?? false;
    }
}
