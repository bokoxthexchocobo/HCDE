namespace HCDE.Net.Core;

/// <summary>NMS1 register / unregister / heartbeat orchestration guards (<c>sv_master.cpp</c> 497–672).</summary>
public static class VmSvMasterNms1Lifecycle
{
    public const ulong RegisterRetryMs = 30000;

    public static bool ShouldSkipRegisterAttemptNative(
        in MasterServerNms1State state,
        bool force,
        ulong now) =>
        !force
        && state.LastRegisterAttempt != 0
        && now - state.LastRegisterAttempt < RegisterRetryMs;

    public static bool ShouldReRegisterOnPortChangeNative(ushort storedPort, ushort advertisedPort) =>
        storedPort != 0 && storedPort != advertisedPort;

    public static VmSvMasterNms1Wait.Nms1HeartbeatResult PortChangeHeartbeatResultNative() =>
        VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister;

    public static bool CanHeartbeatNative(int masterSocket, bool registered) =>
        masterSocket >= 0 && registered;

    public static ushort ResolveHeartbeatGamePortNative(in MasterServerNms1State state, ushort advertisedPort) =>
        state.GamePort != 0 ? state.GamePort : advertisedPort;

    public static bool CanRegisterNative(int masterSocket, bool alreadyRegistered) =>
        masterSocket >= 0 && !alreadyRegistered;

    public static bool ShouldAbortRegisterOnChallengeTimeoutNative(string lastError) =>
        string.IsNullOrEmpty(lastError);

    public static void MarkRegisterAttemptNative(ref MasterServerNms1State state, ulong now) =>
        state.LastRegisterAttempt = now;

    public static void ApplySuccessfulRegistrationNative(
        ref MasterServerNms1State state,
        byte[] entryToken,
        ushort gamePort,
        ushort ttlSeconds,
        ulong now)
    {
        state.EntryToken = entryToken;
        state.GamePort = gamePort;
        state.TtlSeconds = ttlSeconds;
        state.LastRefresh = now;
        state.Registered = true;
        state.LastError = "";
    }

    public static bool ShouldSendUnregisterNative(int masterSocket, bool registered) =>
        masterSocket >= 0 && registered;

    public static ushort ResolveUnregisterGamePortNative(in MasterServerNms1State state, ushort advertisedPort) =>
        state.GamePort != 0 ? state.GamePort : advertisedPort;

    public static VmSvMasterNms1Wait.Nms1HeartbeatResult MapHeartbeatSendFailureNative() =>
        VmSvMasterNms1Wait.Nms1HeartbeatResult.RetryLater;

    public static void ApplyHeartbeatRefreshNative(ref MasterServerNms1State state, ushort ttlSeconds, ulong now)
    {
        state.TtlSeconds = ttlSeconds;
        state.LastRefresh = now;
        state.LastError = "";
    }

    public static void ApplyHeartbeatNeedsRegisterNative(ref MasterServerNms1State state)
    {
        VmSvMasterResolve.ResetNms1RegistrationNative(ref state);
    }

    public static void SetHeartbeatTimeoutErrorNative(ref MasterServerNms1State state)
    {
        if (string.IsNullOrEmpty(state.LastError))
            state.LastError = "heartbeat timeout";
    }

    public static bool ShouldTreatRegisterAsSuccessNative(bool registered) => registered;

    public static bool ShouldClearErrorOnHeartbeatRefreshNative(
        VmSvMasterNms1Wait.Nms1HeartbeatResult result) =>
        result == VmSvMasterNms1Wait.Nms1HeartbeatResult.Refreshed;

    public static bool ShouldResetOnHeartbeatNeedsRegisterNative(
        VmSvMasterNms1Wait.Nms1HeartbeatResult result) =>
        result == VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister;

    public static string DefaultChallengeTimeoutMessageNative() => "challenge timeout";

    public static string DefaultRegisterTimeoutMessageNative() => "register timeout";

    public static string DefaultUnregisterTimeoutMessageNative() => "unregister timeout";

    public static bool ShouldReturnEarlyWhenSocketInvalidNative(int masterSocket) => masterSocket < 0;

    public static bool ShouldReturnRegisteredWhenAlreadyRegisteredNative(bool registered) => registered;
}
