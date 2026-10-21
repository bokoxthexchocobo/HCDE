namespace HCDE.Net.Core;

/// <summary>NMS1 reply wait loops and stale-token policy (<c>sv_master.cpp</c> 356–464).</summary>
public static class VmSvMasterNms1Wait
{
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

    public static bool IsStaleNms1TokenErrorNative(ushort errorCode) =>
        errorCode == Nms1ErrorStaleEntry || errorCode == Nms1ErrorTokenInvalid;

    public static Nms1HeartbeatResult MapHeartbeatAckNative(ushort errorCode, bool isError)
    {
        if (!isError)
            return Nms1HeartbeatResult.Refreshed;
        return IsStaleNms1TokenErrorNative(errorCode)
            ? Nms1HeartbeatResult.NeedsRegister
            : Nms1HeartbeatResult.RetryLater;
    }

    public static bool ShouldContinueAfterNotForRequestNative(Nms1ParseResult result) =>
        result == Nms1ParseResult.NotForRequest || result == Nms1ParseResult.Malformed;

    public static bool ShouldFailChallengeNative(Nms1ParseResult result) =>
        result == Nms1ParseResult.ErrorResponse;

    public static bool ShouldAcceptUnregisterStaleNative(ushort errorCode) =>
        errorCode == Nms1ErrorStaleEntry;

    public static bool ShouldRetrySelectInterruptNative(bool isWindows, int errnoValue) =>
        !isWindows && errnoValue == 4; // EINTR on POSIX

    public static bool IsSelectTimeoutNative(int selectResult) => selectResult == 0;

    public static bool IsSelectErrorNative(int selectResult) => selectResult < 0;

    public static bool ShouldIgnoreUnexpectedReplyNative(
        bool sameAddress,
        Nms1ParseResult result) =>
        !sameAddress || ShouldContinueAfterNotForRequestNative(result);

    public static Nms1HeartbeatResult MapChallengeParseNative(Nms1ParseResult result, ushort errorCode)
    {
        if (result == Nms1ParseResult.Ok)
            return Nms1HeartbeatResult.Refreshed;
        if (result == Nms1ParseResult.ErrorResponse)
            return Nms1HeartbeatResult.RetryLater;
        return Nms1HeartbeatResult.RetryLater;
    }

    public static bool ShouldPrintUnregisterStaleNative(ushort errorCode) =>
        errorCode != Nms1ErrorStaleEntry;

    public static ulong ComputeSelectTimeoutNative(ulong deadlineMs, ulong nowMs)
    {
        if (nowMs >= deadlineMs)
            return 0;
        return deadlineMs - nowMs;
    }

    public static (long Seconds, long Microseconds) SplitSelectTimeoutNative(ulong remainingMs)
    {
        var seconds = (long)(remainingMs / 1000);
        var micros = (long)((remainingMs % 1000) * 1000);
        return (seconds, micros);
    }
}
