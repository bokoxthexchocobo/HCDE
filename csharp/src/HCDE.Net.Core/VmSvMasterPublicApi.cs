namespace HCDE.Net.Core;

/// <summary>Public master-server API guards (<c>sv_master.cpp</c> 837–1092).</summary>
public static class VmSvMasterPublicApi
{
    public static bool ShouldCreateSocketOnInitNative(int masterSocket) => masterSocket < 0;

    public static bool ShouldShutdownWhenDisabledNative(bool svUseMasters, int masterSocket) =>
        !svUseMasters && masterSocket >= 0;

    public static bool ShouldReinitWhenSocketMissingNative(bool svUseMasters, int masterSocket) =>
        svUseMasters && masterSocket < 0;

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

    public static int GetMasterCountNative(int masterCount) => masterCount;

    public static bool CcmdNeedsHostArgumentNative(int argc) => argc <= 1;

    public static bool ShouldUnregisterOnShutdownNative(int masterSocket) => masterSocket >= 0;

    public static bool ShouldAddDefaultMastersWhenEmptyNative(int masterCount) => masterCount == 0;

    public static bool ShouldResetNms1OnAddressChangeNative(bool sameAddress) => !sameAddress;

    public static bool ShouldSkipInitWhenDisabledNative(bool advertise) => !advertise;

    public static bool ShouldReturnAfterSocketCreateFailureNative(int masterSocket) => masterSocket < 0;

    public static bool ShouldUnregisterPreviousAddressNative(bool addressesMatch) => !addressesMatch;

    public static bool ShouldPrintReresolveFailureNative(bool resolved) => !resolved;

    public static bool TryValidateAddMasterNative(
        string masterIp,
        bool alreadyListed,
        bool resolved,
        out string failureMessage)
    {
        if (string.IsNullOrEmpty(masterIp))
        {
            failureMessage = FormatAddFailureEmptyNative();
            return false;
        }
        if (alreadyListed)
        {
            failureMessage = FormatAddFailureDuplicateNative(masterIp);
            return false;
        }
        if (!resolved)
        {
            failureMessage = FormatAddFailureResolveNative(masterIp);
            return false;
        }
        failureMessage = "";
        return true;
    }

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

    public static string FormatListMastersHeaderNative() =>
        "Use addmaster/delmaster commands to modify this list";

    public static string FormatArchiveMasterLineNative(string masterIp) => $"addmaster {masterIp}";

    public static string FormatRemoveFailureNative(string masterIp) =>
        $"Failed to remove master: {masterIp}, not in list";

    public static string FormatAddFailureEmptyNative() => "Failed to add master: empty address";

    public static string FormatAddFailureDuplicateNative(string masterIp) =>
        $"Master {masterIp} is already on the list";

    public static string FormatAddFailureResolveNative(string masterIp) =>
        $"Failed to resolve master server: {masterIp}, not added";

    public static string FormatAddedMasterNative(string masterIp, string inetAddr) =>
        $"Added master: {masterIp} [{inetAddr}]";

    public static string FormatRegisteredExpiredStateNative(ushort ttlRemaining) =>
        ttlRemaining == 0 ? "registered-expired" : "registered";

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

    public static void RunAddMasterCcmdNative(int argc, Action<string> printUsage, Action<string> addMaster, string? arg1)
    {
        if (CcmdNeedsHostArgumentNative(argc))
        {
            printUsage("addmaster <host[:port]>");
            return;
        }
        if (!string.IsNullOrEmpty(arg1))
            addMaster(arg1);
    }

    public static void RunAdvertiseCcmdNative(
        int argc,
        Action<string> addMaster,
        Action enableAdvertising,
        Action initMasters,
        Func<int, string?> getArg)
    {
        enableAdvertising();
        for (var i = 1; i < argc; i++)
        {
            var arg = getArg(i);
            if (!string.IsNullOrEmpty(arg))
                addMaster(arg);
        }
        initMasters();
    }

    public static void RunUnadvertiseCcmdNative(Action disableAdvertising, Action shutdownMasters)
    {
        disableAdvertising();
        shutdownMasters();
    }

    public static void RunDelMasterCcmdNative(int argc, Action<string> printUsage, Action<string> removeMaster, string? arg1)
    {
        if (CcmdNeedsHostArgumentNative(argc))
        {
            printUsage("delmaster <host[:port]>");
            return;
        }
        if (!string.IsNullOrEmpty(arg1))
            removeMaster(arg1);
    }

    public static void RunMastersCcmdNative(Action listMasters) => listMasters();

    public static bool InitMastersNative(InitMastersContext ctx)
    {
        var advertise = ctx.ResolveStartupAdvertising();
        ctx.DebugMark($"init enabled={(advertise ? 1 : 0)}");
        if (!advertise)
        {
            ctx.Print("Master advertising is disabled because sv_usemasters is 0");
            return false;
        }
        if (ShouldCreateSocketOnInitNative(ctx.MasterSocket))
        {
            ctx.MasterSocket = ctx.CreateMasterSocket();
            if (ShouldReturnAfterSocketCreateFailureNative(ctx.MasterSocket))
                return false;
        }
        ctx.AddStartupMasters();
        if (ShouldAddDefaultMastersWhenEmptyNative(ctx.MasterCount))
        {
            foreach (var def in ctx.DefaultMasters)
                ctx.AddMaster(def);
        }
        ctx.UpdateMaster(true);
        return true;
    }

    public static void ShutdownMastersNative(ref int masterSocket, Action unregisterAll, Action<int> closeSocket)
    {
        if (!ShouldUnregisterOnShutdownNative(masterSocket))
            return;
        unregisterAll();
        closeSocket(masterSocket);
        masterSocket = -1;
    }

    public static void UpdateMasterServersNative(UpdateMasterServersContext ctx)
    {
        if (!ctx.SvUseMasters)
        {
            if (ShouldShutdownWhenDisabledNative(ctx.SvUseMasters, ctx.MasterSocket))
                ctx.ShutdownMasters();
            return;
        }
        if (ShouldReinitWhenSocketMissingNative(ctx.SvUseMasters, ctx.MasterSocket))
        {
            ctx.InitMasters();
            return;
        }
        ctx.UpdateMasterServersInternal(false);
    }

    public static bool AddMasterNative(AddMasterContext ctx, string masterIp)
    {
        if (string.IsNullOrEmpty(masterIp))
        {
            ctx.Print(FormatAddFailureEmptyNative());
            return false;
        }
        if (ctx.MasterListContains(masterIp))
        {
            ctx.Print(FormatAddFailureDuplicateNative(masterIp));
            return false;
        }
        var resolved = ctx.ResolveMaster(masterIp);
        if (!resolved.Ok)
        {
            ctx.Print(FormatAddFailureResolveNative(masterIp));
            return false;
        }
        ctx.Print(FormatAddedMasterNative(masterIp, resolved.InetAddr));
        ctx.DebugMark($"add {masterIp}");
        ctx.PushMaster(resolved.Master);
        ctx.UpdateMaster(true);
        return true;
    }

    public static bool RemoveMasterNative(RemoveMasterContext ctx, string masterIp)
    {
        for (var index = 0; index < ctx.MasterCount; index++)
        {
            if (!ctx.MasterMatchesPrefix(index, masterIp))
                continue;
            ctx.TryUnregister(index);
            ctx.Print($"Removed master server: {ctx.GetMasterIp(index)}");
            ctx.DebugMark($"remove {ctx.GetMasterIp(index)}");
            ctx.RemoveAt(index);
            ctx.UpdateMaster(true);
            return true;
        }
        ctx.Print(FormatRemoveFailureNative(masterIp));
        return false;
    }

    public static void ListMastersNative(ListMastersContext ctx)
    {
        ctx.Print(FormatListMastersHeaderNative());
        var now = ctx.NowMs();
        for (var i = 0; i < ctx.MasterCount; i++)
        {
            var line = ctx.FormatMasterLine(i, now);
            if (!string.IsNullOrEmpty(line))
                ctx.Print(line);
        }
    }

    public static void ArchiveMastersNative(ArchiveMastersContext ctx)
    {
        if (ctx.Output is null)
            return;
        for (var i = 0; i < ctx.MasterCount; i++)
            ctx.WriteLine(FormatArchiveMasterLineNative(ctx.GetMasterIp(i)));
    }

    public static void UpdateMasterNative(UpdateMasterContext ctx, bool force)
    {
        if (ShouldSkipUpdateWhenDisabledNative(ctx.SvUseMasters, ctx.MasterSocket, ctx.MasterCount))
            return;
        var now = ctx.NowMs();
        if (ShouldReresolveAddressesNative(now, ctx.LastAddressResolution, ctx.ReresolveIntervalMs, force))
        {
            ctx.DebugMark($"re-resolve {ctx.MasterCount} masters");
            for (var i = 0; i < ctx.MasterCount; i++)
                ctx.ReresolveMaster(i);
            ctx.LastAddressResolution = now;
        }
        if (ShouldRunKeepAliveNative(now, ctx.LastKeepAlive, ctx.KeepAliveIntervalMs, force))
        {
            ctx.DebugMark($"heartbeat cycle {ctx.MasterCount} masters");
            ctx.UpdateMasterServersInternal(force);
            ctx.LastKeepAlive = now;
        }
    }
}

public sealed class InitMastersContext
{
    public required Func<bool> ResolveStartupAdvertising { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Action<string> Print { get; init; }
    public int MasterSocket { get; set; }
    public required Func<int> CreateMasterSocket { get; init; }
    public required Action AddStartupMasters { get; init; }
    public required int MasterCount { get; init; }
    public required IReadOnlyList<string> DefaultMasters { get; init; }
    public required Action<string> AddMaster { get; init; }
    public required Action<bool> UpdateMaster { get; init; }
}

public sealed class UpdateMasterServersContext
{
    public required bool SvUseMasters { get; init; }
    public required int MasterSocket { get; init; }
    public required Action ShutdownMasters { get; init; }
    public required Action InitMasters { get; init; }
    public required Action<bool> UpdateMasterServersInternal { get; init; }
}

public sealed class AddMasterContext
{
    public required Action<string> Print { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Func<string, bool> MasterListContains { get; init; }
    public required Func<string, ResolvedMasterAddResult> ResolveMaster { get; init; }
    public required Action<object> PushMaster { get; init; }
    public required Action<bool> UpdateMaster { get; init; }
}

public sealed class RemoveMasterContext
{
    public required int MasterCount { get; init; }
    public required Func<int, string, bool> MasterMatchesPrefix { get; init; }
    public required Action<int> TryUnregister { get; init; }
    public required Func<int, string> GetMasterIp { get; init; }
    public required Action<string> Print { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Action<int> RemoveAt { get; init; }
    public required Action<bool> UpdateMaster { get; init; }
}

public sealed class ListMastersContext
{
    public required int MasterCount { get; init; }
    public required Action<string> Print { get; init; }
    public required Func<ulong> NowMs { get; init; }
    public required Func<int, ulong, string?> FormatMasterLine { get; init; }
}

public sealed class ArchiveMastersContext
{
    public required TextWriter? Output { get; init; }
    public required int MasterCount { get; init; }
    public required Func<int, string> GetMasterIp { get; init; }
    public required Action<string> WriteLine { get; init; }
}

public readonly record struct ResolvedMasterAddResult(bool Ok, string InetAddr, object Master);

public sealed class UpdateMasterContext
{
    public required bool SvUseMasters { get; init; }
    public required int MasterSocket { get; init; }
    public required int MasterCount { get; init; }
    public required Func<ulong> NowMs { get; init; }
    public ulong LastAddressResolution { get; set; }
    public ulong LastKeepAlive { get; set; }
    public ulong ReresolveIntervalMs { get; init; }
    public ulong KeepAliveIntervalMs { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Action<int> ReresolveMaster { get; init; }
    public required Action<bool> UpdateMasterServersInternal { get; init; }
}
