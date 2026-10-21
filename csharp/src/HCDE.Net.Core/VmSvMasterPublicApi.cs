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

    public static bool AddMasterNeedsArgvNative(int argc) => argc <= 1;

    public static bool AdvertiseNeedsArgvNative(int argc) => argc <= 1;

    public static bool DelMasterNeedsArgvNative(int argc) => argc <= 1;

    public static bool ShouldUnregisterOnShutdownNative(int masterSocket) => masterSocket >= 0;

    public static bool ShouldAddDefaultMastersWhenEmptyNative(int masterCount) => masterCount == 0;

    public static bool ShouldUpdateAfterAddNative(bool resolved) => resolved;

    public static bool ShouldRemoveByPrefixMatchNative(bool prefixMatch) => prefixMatch;

    public static bool ShouldUnregisterBeforeRemoveNative(bool nms1Registered) => nms1Registered;

    public static string FormatRegisteredMasterLineNative(
        string masterIp,
        string inetAddr,
        ushort gamePort,
        ushort ttlRemaining,
        ulong refreshAgeSeconds,
        string? lastError)
    {
        if (!string.IsNullOrEmpty(lastError))
        {
            return $"{masterIp} [{inetAddr}] nms1=registered port={gamePort} ttl={ttlRemaining}s refreshed={refreshAgeSeconds}s ago last=\"{lastError}\"";
        }
        return $"{masterIp} [{inetAddr}] nms1=registered port={gamePort} ttl={ttlRemaining}s refreshed={refreshAgeSeconds}s ago";
    }

    public static string FormatPendingMasterLineNative(string masterIp, string inetAddr, string? lastError) =>
        string.IsNullOrEmpty(lastError)
            ? $"{masterIp} [{inetAddr}] nms1=pending"
            : $"{masterIp} [{inetAddr}] nms1=pending last=\"{lastError}\"";

    public static bool ShouldResetNms1OnAddressChangeNative(bool sameAddress) => !sameAddress;

    public static bool ShouldArchiveMasterNative(bool fpValid) => fpValid;

    public static string FormatArchiveMasterLineNative(string masterIp) => $"addmaster {masterIp}";

    public static bool ShouldSkipInitWhenDisabledNative(bool advertise) => !advertise;

    public static bool ShouldReturnAfterSocketCreateFailureNative(int masterSocket) => masterSocket < 0;

    public static bool ShouldInvokeStartupMastersNative(bool argsPresent) => argsPresent;

    public static bool ShouldCallUpdateMasterAfterAddNative(bool added) => added;

    public static bool ShouldCallUpdateMasterAfterRemoveNative(bool removed) => removed;

    public static bool ShouldListMastersHeaderNative() => true;

    public static string FormatRemoveFailureNative(string masterIp) =>
        $"Failed to remove master: {masterIp}, not in list";

    public static string FormatAddFailureEmptyNative() => "Failed to add master: empty address";

    public static string FormatAddFailureDuplicateNative(string masterIp) =>
        $"Master {masterIp} is already on the list";

    public static string FormatAddFailureResolveNative(string masterIp) =>
        $"Failed to resolve master server: {masterIp}, not added";

    public static string FormatAddedMasterNative(string masterIp, string inetAddr) =>
        $"Added master: {masterIp} [{inetAddr}]";

    public static bool ShouldUnregisterPreviousAddressNative(bool addressesMatch) => !addressesMatch;

    public static bool ShouldPrintReresolveFailureNative(bool resolved) => !resolved;

    public static ulong MasterCountNative(int count) => (ulong)count;

    public static bool TryParseMasterListIndexNative(int index, int count, out bool valid)
    {
        valid = index >= 0 && index < count;
        return valid;
    }

    public static void ApplyShutdownSocketNative(ref int masterSocket, Action<int>? closeSocket)
    {
        if (masterSocket < 0)
            return;
        closeSocket?.Invoke(masterSocket);
        masterSocket = -1;
    }

    public static bool ShouldEnableAdvertisingCcmdNative(bool newValue) => newValue;
}
