namespace HCDE.Net.Core;

/// <summary>NMS1 send/receive and wait-for-ack loops (<c>sv_master.cpp</c> 284–495).</summary>
public static class VmSvMasterNms1Wait
{
    public const int MasterNms1ResponseTimeoutMs = 750;
    public const ushort Nms1ErrorStaleEntry = 11;
    public const ushort Nms1ErrorTokenInvalid = 12;

    public enum Nms1HeartbeatResult : byte
    {
        Refreshed = 0,
        RetryLater = 1,
        NeedsRegister = 2,
    }

    public enum Nms1ParseResult : byte
    {
        Ok = 0,
        NotForRequest = 1,
        ErrorResponse = 2,
        Malformed = 3,
    }

    public static bool SendNms1PacketNative(
        int masterSocket,
        bool packetEmpty,
        byte[] packetData,
        int packetSize,
        string label,
        Action<byte[], int>? sendTo,
        Action<string>? debugMark,
        Action<string>? printError)
    {
        if (masterSocket < 0 || packetEmpty)
            return false;
        if (sendTo is null)
            return false;
        try
        {
            sendTo(packetData, packetSize);
        }
        catch
        {
            debugMark?.Invoke($"nms1 {label} send failed");
            printError?.Invoke($"Failed to contact master server: send failed");
            return false;
        }
        debugMark?.Invoke($"nms1 {label} sent");
        return true;
    }

    public static bool ReceiveNms1ReplyNative(Nms1MasterReplyContext ctx, ulong deadline, byte[] reply, out int replySize)
    {
        replySize = 0;
        for (;;)
        {
            var now = ctx.NowMs();
            if (now >= deadline)
                return false;

            var remaining = deadline - now;
            var selectResult = ctx.SelectRead(ctx.MasterSocket, remaining);
            if (selectResult < 0)
            {
                if (!ctx.IsWindows && ctx.LastSocketError() == ctx.ErrnoEintr)
                    continue;
                ctx.DebugMark("nms1 select failed");
                ctx.PrintError("Failed to read master server response: select failed");
                return false;
            }
            if (selectResult == 0)
                return false;

            var recv = ctx.RecvFrom(reply);
            replySize = recv.Size;
            ctx.FromAddress = recv.From;
            if (replySize < 0)
            {
                if (!ctx.IsWindows && ctx.LastSocketError() == ctx.ErrnoEintr)
                    continue;
                ctx.DebugMark("nms1 recv failed");
                ctx.PrintError("Failed to read master server response: recv failed");
                return false;
            }

            if (!ctx.SameAddress(ctx.ExpectedAddress, ctx.FromAddress))
            {
                ctx.DebugMark("nms1 ignored response from unexpected address");
                continue;
            }

            return true;
        }
    }

    public static void PrintNms1ErrorNative(
        string masterIp,
        string operation,
        ushort errorCode,
        string? errorText,
        Action<string>? print)
    {
        if (print is null)
            return;
        if (!string.IsNullOrEmpty(errorText))
            print($"Master server {masterIp} rejected NMS1 {operation}: {errorCode} ({errorText})");
        else
            print($"Master server {masterIp} rejected NMS1 {operation}: {errorCode}");
    }

    public static bool IsStaleNms1TokenErrorNative(ushort errorCode) =>
        errorCode == Nms1ErrorStaleEntry || errorCode == Nms1ErrorTokenInvalid;

    public static bool WaitForNms1ChallengeNative(Nms1WaitParseContext ctx, uint requestId, out byte[] challengeToken, out ushort ttlSeconds)
    {
        challengeToken = Array.Empty<byte>();
        ttlSeconds = 0;
        var deadline = ctx.NowMs() + (ulong)MasterNms1ResponseTimeoutMs;
        var reply = new byte[ctx.MaxPacketSize];
        for (;;)
        {
            if (!ReceiveNms1ReplyNative(ctx.Reply, deadline, reply, out var replySize))
                return false;
            var outcome = ctx.TryReadChallenge(reply, replySize, requestId);
            switch (outcome.Result)
            {
                case Nms1ParseResult.Ok:
                    challengeToken = outcome.Token;
                    ttlSeconds = outcome.TtlSeconds;
                    return true;
                case Nms1ParseResult.NotForRequest:
                    continue;
                case Nms1ParseResult.ErrorResponse:
                    ctx.SetError("challenge", outcome.ErrorCode, outcome.ErrorText);
                    PrintNms1ErrorNative(ctx.MasterIp, "challenge", outcome.ErrorCode, outcome.ErrorText, ctx.Print);
                    return false;
                case Nms1ParseResult.Malformed:
                    ctx.DebugMark("nms1 malformed challenge response");
                    continue;
                default:
                    continue;
            }
        }
    }

    public static bool WaitForNms1RegisterAckNative(
        Nms1WaitParseContext ctx,
        uint requestId,
        out byte[] entryToken,
        out ushort ttlSeconds)
    {
        entryToken = Array.Empty<byte>();
        ttlSeconds = 0;
        var deadline = ctx.NowMs() + (ulong)MasterNms1ResponseTimeoutMs;
        var reply = new byte[ctx.MaxPacketSize];
        for (;;)
        {
            if (!ReceiveNms1ReplyNative(ctx.Reply, deadline, reply, out var replySize))
                return false;
            var outcome = ctx.TryReadRegisterAck(reply, replySize, requestId);
            switch (outcome.Result)
            {
                case Nms1ParseResult.Ok:
                    entryToken = outcome.Token;
                    ttlSeconds = outcome.TtlSeconds;
                    return true;
                case Nms1ParseResult.NotForRequest:
                    continue;
                case Nms1ParseResult.ErrorResponse:
                    ctx.SetError("register", outcome.ErrorCode, outcome.ErrorText);
                    PrintNms1ErrorNative(ctx.MasterIp, "register", outcome.ErrorCode, outcome.ErrorText, ctx.Print);
                    return false;
                case Nms1ParseResult.Malformed:
                    ctx.DebugMark("nms1 malformed register ack");
                    continue;
                default:
                    continue;
            }
        }
    }

    public static Nms1HeartbeatResult WaitForNms1HeartbeatAckNative(Nms1WaitParseContext ctx, uint requestId, out ushort ttlSeconds)
    {
        ttlSeconds = 0;
        var deadline = ctx.NowMs() + (ulong)MasterNms1ResponseTimeoutMs;
        var reply = new byte[ctx.MaxPacketSize];
        for (;;)
        {
            if (!ReceiveNms1ReplyNative(ctx.Reply, deadline, reply, out var replySize))
                return Nms1HeartbeatResult.RetryLater;
            var outcome = ctx.TryReadHeartbeatAck(reply, replySize, requestId);
            switch (outcome.Result)
            {
                case Nms1ParseResult.Ok:
                    ttlSeconds = outcome.TtlSeconds;
                    return Nms1HeartbeatResult.Refreshed;
                case Nms1ParseResult.NotForRequest:
                    continue;
                case Nms1ParseResult.ErrorResponse:
                    ctx.SetError("heartbeat", outcome.ErrorCode, outcome.ErrorText);
                    PrintNms1ErrorNative(ctx.MasterIp, "heartbeat", outcome.ErrorCode, outcome.ErrorText, ctx.Print);
                    return IsStaleNms1TokenErrorNative(outcome.ErrorCode)
                        ? Nms1HeartbeatResult.NeedsRegister
                        : Nms1HeartbeatResult.RetryLater;
                case Nms1ParseResult.Malformed:
                    ctx.DebugMark("nms1 malformed heartbeat ack");
                    continue;
                default:
                    continue;
            }
        }
    }

    public static bool WaitForNms1UnregisterAckNative(Nms1WaitParseContext ctx, uint requestId)
    {
        var deadline = ctx.NowMs() + (ulong)MasterNms1ResponseTimeoutMs;
        var reply = new byte[ctx.MaxPacketSize];
        for (;;)
        {
            if (!ReceiveNms1ReplyNative(ctx.Reply, deadline, reply, out var replySize))
                return false;
            var outcome = ctx.TryReadUnregisterAck(reply, replySize, requestId);
            switch (outcome.Result)
            {
                case Nms1ParseResult.Ok:
                    return true;
                case Nms1ParseResult.NotForRequest:
                    continue;
                case Nms1ParseResult.ErrorResponse:
                    ctx.SetError("unregister", outcome.ErrorCode, outcome.ErrorText);
                    if (outcome.ErrorCode != Nms1ErrorStaleEntry)
                        PrintNms1ErrorNative(ctx.MasterIp, "unregister", outcome.ErrorCode, outcome.ErrorText, ctx.Print);
                    return outcome.ErrorCode == Nms1ErrorStaleEntry;
                case Nms1ParseResult.Malformed:
                    ctx.DebugMark("nms1 malformed unregister ack");
                    continue;
                default:
                    continue;
            }
        }
    }
}

public sealed class Nms1MasterReplyContext
{
    public required Func<ulong> NowMs { get; init; }
    public required int MasterSocket { get; init; }
    public required bool IsWindows { get; init; }
    public required int ErrnoEintr { get; init; }
    public required Func<int, ulong, int> SelectRead { get; init; }
    public required Func<byte[], (int Size, NetworkAddressParts From)> RecvFrom { get; init; }
    public required Func<int> LastSocketError { get; init; }
    public required NetworkAddressParts ExpectedAddress { get; init; }
    public NetworkAddressParts FromAddress { get; set; }
    public required Action<string> DebugMark { get; init; }
    public required Action<string> PrintError { get; init; }
    public required Func<NetworkAddressParts, NetworkAddressParts, bool> SameAddress { get; init; }
}

public sealed class Nms1WaitParseContext
{
    public required string MasterIp { get; init; }
    public required int MaxPacketSize { get; init; }
    public required Nms1MasterReplyContext Reply { get; init; }
    public required Func<ulong> NowMs { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Action<string>? Print { get; init; }
    public required Action<string, ushort, string?> SetError { get; init; }
    public required Func<byte[], int, uint, Nms1ParseOutcome> TryReadChallenge { get; init; }
    public required Func<byte[], int, uint, Nms1ParseOutcome> TryReadRegisterAck { get; init; }
    public required Func<byte[], int, uint, Nms1ParseOutcome> TryReadHeartbeatAck { get; init; }
    public required Func<byte[], int, uint, Nms1ParseOutcome> TryReadUnregisterAck { get; init; }
}

public readonly record struct Nms1ParseOutcome(
    VmSvMasterNms1Wait.Nms1ParseResult Result,
    ushort ErrorCode,
    string? ErrorText,
    byte[] Token,
    ushort TtlSeconds)
{
    public static Nms1ParseOutcome Ok(byte[] token, ushort ttl) =>
        new(VmSvMasterNms1Wait.Nms1ParseResult.Ok, 0, null, token, ttl);

    public static Nms1ParseOutcome HeartbeatOk(ushort ttl) =>
        new(VmSvMasterNms1Wait.Nms1ParseResult.Ok, 0, null, Array.Empty<byte>(), ttl);

    public static Nms1ParseOutcome Simple(VmSvMasterNms1Wait.Nms1ParseResult result) =>
        new(result, 0, null, Array.Empty<byte>(), 0);

    public static Nms1ParseOutcome Error(ushort code, string? text) =>
        new(VmSvMasterNms1Wait.Nms1ParseResult.ErrorResponse, code, text, Array.Empty<byte>(), 0);
}
