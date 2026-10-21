namespace HCDE.Net.Core;

/// <summary>Public master-server API guards (<c>sv_master.cpp</c> 883–1092).</summary>
public static class VmSvMasterPublicApi
{
    public static bool ShouldInitMastersNative(bool advertise) => advertise;

    public static bool ShouldCreateSocketOnInitNative(int masterSocket) => masterSocket < 0;

    public static bool ShouldShutdownWhenDisabledNative(bool svUseMasters, int masterSocket) =>
        !svUseMasters && masterSocket >= 0;

    public static bool ShouldReinitWhenSocketMissingNative(bool svUseMasters, int masterSocket) =>
        svUseMasters && masterSocket < 0;

    public static bool ShouldRejectEmptyMasterNative(string masterIp) => string.IsNullOrEmpty(masterIp);

    public static bool ShouldRejectDuplicateMasterNative(bool alreadyListed) => alreadyListed;

    public static bool ShouldRejectUnresolvedMasterNative(bool resolved) => !resolved;

    public static ulong ElapsedSinceNative(ulong timestamp, ulong now) =>
        VmSvMasterResolve.ElapsedSecondsSinceNative(timestamp, now);

    public static ushort RemainingTtlNative(in MasterServerNms1State state, ulong now) =>
        VmSvMasterResolve.RemainingNms1TtlNative(state, now);

    public static bool ShouldReresolveAddressesNative(ulong now, ulong lastResolution, ulong intervalMs, bool force) =>
        force || now - lastResolution >= intervalMs;

    public static bool ShouldRunKeepAliveNative(ulong now, ulong lastKeepAlive, ulong intervalMs, bool force) =>
        force || now - lastKeepAlive >= intervalMs;

    public static bool ShouldSkipUpdateWhenDisabledNative(bool svUseMasters, int masterSocket, int masterCount) =>
        !svUseMasters || masterSocket < 0 || masterCount == 0;

    public static bool IsAdvertisingEnabledNative(bool svUseMasters) => svUseMasters;
}
