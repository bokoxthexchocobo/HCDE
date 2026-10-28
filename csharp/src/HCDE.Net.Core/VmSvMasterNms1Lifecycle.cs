namespace HCDE.Net.Core;

/// <summary>NMS1 register / unregister / heartbeat (<c>sv_master.cpp</c> 497–672).</summary>
public static class VmSvMasterNms1Lifecycle
{
    public const ulong RegisterRetryMs = 30000;

    public static bool TryRegisterNms1MasterNative(TryRegisterNms1MasterContext ctx, ref MasterServerNms1State master, bool force)
    {
        if (ctx.MasterSocket < 0 || master.Registered)
            return master.Registered;

        var now = ctx.NowMs();
        if (!force
            && master.LastRegisterAttempt != 0
            && now - master.LastRegisterAttempt < RegisterRetryMs)
            return false;

        ctx.ClearError(ref master);
        master.LastRegisterAttempt = now;

        var challengeRequestId = VmSvMasterResolve.NextNms1RequestIdNative(ref master);
        var challengePacket = ctx.WriteChallengeRequest(challengeRequestId);
        if (challengePacket is null
            || !ctx.SendPacket(challengePacket, "challenge"))
        {
            ctx.SetErrorText(ref master, "challenge send failed");
            return false;
        }

        if (!VmSvMasterNms1Wait.WaitForNms1ChallengeNative(
                ctx.Wait, challengeRequestId, out var challengeToken, out _))
        {
            if (string.IsNullOrEmpty(master.LastError))
                ctx.SetErrorText(ref master, "challenge timeout");
            ctx.DebugMark("nms1 challenge timed out");
            return false;
        }

        var snapshot = ctx.GetLocalSnapshot();
        var buildLabel = VmSvMasterResolve.MakeNms1TextNative(snapshot.BuildLabel, ctx.MaxBuildLabelBytes, "HCDE");
        var displayName = VmSvMasterResolve.MakeNms1TextNative(snapshot.DisplayName, ctx.MaxDisplayNameBytes, "HCDE server");
        var gameName = VmSvMasterResolve.MakeNms1TextNative(snapshot.GameName, ctx.MaxGameNameBytes, "HCDE");
        var mapName = VmSvMasterResolve.MakeNms1TextNative(snapshot.MapName, ctx.MaxMapNameBytes, "unknown");
        var gamePort = ctx.GetAdvertisedGamePort();

        var registerRequestId = VmSvMasterResolve.NextNms1RequestIdNative(ref master);
        var registerPacket = ctx.WriteRegisterRequest(
            registerRequestId,
            challengeToken,
            gamePort,
            snapshot.PlayerCount,
            snapshot.MaxPlayers,
            buildLabel,
            displayName,
            gameName,
            mapName);
        if (registerPacket is null
            || !ctx.SendPacket(registerPacket, "register"))
        {
            ctx.SetErrorText(ref master, "register send failed");
            return false;
        }

        if (!VmSvMasterNms1Wait.WaitForNms1RegisterAckNative(ctx.Wait, registerRequestId, out var entryToken, out var entryTtl))
        {
            if (string.IsNullOrEmpty(master.LastError))
                ctx.SetErrorText(ref master, "register timeout");
            ctx.DebugMark("nms1 register timed out");
            return false;
        }

        master.EntryToken = entryToken;
        master.GamePort = gamePort;
        master.TtlSeconds = entryTtl;
        master.LastRefresh = ctx.NowMs();
        master.Registered = true;
        ctx.ClearError(ref master);
        ctx.Print($"Registered with HCDE master server {ctx.MasterIp} (ttl={entryTtl})");
        ctx.DebugMark($"nms1 registered ttl={entryTtl}");
        return true;
    }

    public static bool TryUnregisterNms1MasterNative(TryUnregisterNms1MasterContext ctx, ref MasterServerNms1State master)
    {
        if (ctx.MasterSocket < 0 || !master.Registered)
            return false;

        var gamePort = master.GamePort != 0 ? master.GamePort : ctx.GetAdvertisedGamePort();
        var requestId = VmSvMasterResolve.NextNms1RequestIdNative(ref master);
        var packet = ctx.WriteUnregisterRequest(requestId, gamePort, master.EntryToken);
        if (packet is null
            || !ctx.SendPacket(packet, "unregister"))
        {
            ctx.SetErrorText(ref master, "unregister send failed");
            VmSvMasterResolve.ResetNms1RegistrationNative(ref master);
            return false;
        }

        var ok = VmSvMasterNms1Wait.WaitForNms1UnregisterAckNative(ctx.Wait, requestId);
        if (ok)
            ctx.DebugMark("nms1 unregistered");
        else
        {
            if (string.IsNullOrEmpty(master.LastError))
                ctx.SetErrorText(ref master, "unregister timeout");
            ctx.DebugMark("nms1 unregister timed out");
        }

        VmSvMasterResolve.ResetNms1RegistrationNative(ref master);
        return ok;
    }

    public static VmSvMasterNms1Wait.Nms1HeartbeatResult TryHeartbeatNms1MasterNative(
        TryHeartbeatNms1MasterContext ctx,
        ref MasterServerNms1State master)
    {
        if (ctx.MasterSocket < 0 || !master.Registered)
            return VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister;

        ctx.ClearError(ref master);
        var advertisedPort = ctx.GetAdvertisedGamePort();
        if (master.GamePort != 0 && master.GamePort != advertisedPort)
        {
            TryUnregisterNms1MasterNative(ctx.Unregister, ref master);
            ctx.DebugMark("nms1 port changed, re-register");
            return VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister;
        }

        var snapshot = ctx.GetLocalSnapshot();
        var gamePort = master.GamePort != 0 ? master.GamePort : advertisedPort;
        var requestId = VmSvMasterResolve.NextNms1RequestIdNative(ref master);
        var packet = ctx.WriteHeartbeatRequest(
            requestId, gamePort, master.EntryToken, snapshot.PlayerCount, snapshot.MaxPlayers);
        if (packet is null
            || !ctx.SendPacket(packet, "heartbeat"))
        {
            ctx.SetErrorText(ref master, "heartbeat send failed");
            return VmSvMasterNms1Wait.Nms1HeartbeatResult.RetryLater;
        }

        var result = VmSvMasterNms1Wait.WaitForNms1HeartbeatAckNative(ctx.Wait, requestId, out var ttlSeconds);
        if (result == VmSvMasterNms1Wait.Nms1HeartbeatResult.Refreshed)
        {
            master.TtlSeconds = ttlSeconds;
            master.LastRefresh = ctx.NowMs();
            ctx.ClearError(ref master);
            ctx.DebugMark($"nms1 heartbeat ack ttl={ttlSeconds}");
        }
        else if (result == VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister)
        {
            VmSvMasterResolve.ResetNms1RegistrationNative(ref master);
            ctx.DebugMark("nms1 heartbeat requested re-register");
        }
        else
        {
            if (string.IsNullOrEmpty(master.LastError))
                ctx.SetErrorText(ref master, "heartbeat timeout");
            ctx.DebugMark("nms1 heartbeat timed out");
        }

        return result;
    }
}

public readonly record struct MasterServerSnapshotParts(
    string? BuildLabel,
    string? DisplayName,
    string? GameName,
    string? MapName,
    ushort PlayerCount,
    ushort MaxPlayers);

public sealed class TryRegisterNms1MasterContext
{
    public required int MasterSocket { get; init; }
    public required string MasterIp { get; init; }
    public required Func<ulong> NowMs { get; init; }
    public required Nms1WaitParseContext Wait { get; init; }
    public required Func<MasterServerSnapshotParts> GetLocalSnapshot { get; init; }
    public required Func<ushort> GetAdvertisedGamePort { get; init; }
    public required Func<uint, byte[]?> WriteChallengeRequest { get; init; }
    public required Func<byte[], string, bool> SendPacket { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Action<string> Print { get; init; }
    public required Nms1ClearErrorDelegate ClearError { get; init; }
    public required Nms1SetErrorDelegate SetErrorText { get; init; }
    public int MaxBuildLabelBytes { get; init; }
    public int MaxDisplayNameBytes { get; init; }
    public int MaxGameNameBytes { get; init; }
    public int MaxMapNameBytes { get; init; }
    public required Func<uint, byte[], ushort, ushort, ushort, string, string, string, string, byte[]?> WriteRegisterRequest { get; init; }
}

public sealed class TryUnregisterNms1MasterContext
{
    public required int MasterSocket { get; init; }
    public required Func<ushort> GetAdvertisedGamePort { get; init; }
    public required Nms1WaitParseContext Wait { get; init; }
    public required Func<uint, ushort, byte[], byte[]?> WriteUnregisterRequest { get; init; }
    public required Func<byte[], string, bool> SendPacket { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Nms1SetErrorDelegate SetErrorText { get; init; }
}

public sealed class TryHeartbeatNms1MasterContext
{
    public required int MasterSocket { get; init; }
    public required TryUnregisterNms1MasterContext Unregister { get; init; }
    public required Func<ulong> NowMs { get; init; }
    public required Func<ushort> GetAdvertisedGamePort { get; init; }
    public required Func<MasterServerSnapshotParts> GetLocalSnapshot { get; init; }
    public required Nms1WaitParseContext Wait { get; init; }
    public required Func<uint, ushort, byte[], ushort, ushort, byte[]?> WriteHeartbeatRequest { get; init; }
    public required Func<byte[], string, bool> SendPacket { get; init; }
    public required Action<string> DebugMark { get; init; }
    public required Nms1ClearErrorDelegate ClearError { get; init; }
    public required Nms1SetErrorDelegate SetErrorText { get; init; }
}

public delegate void Nms1ClearErrorDelegate(ref MasterServerNms1State master);

public delegate void Nms1SetErrorDelegate(ref MasterServerNms1State master, string message);
