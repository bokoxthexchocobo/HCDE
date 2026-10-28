using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

/// <summary>Launcher query response path (<c>i_net.cpp</c> 904–1092).</summary>
public static class VmINetLauncherRespond
{
    public const ulong QuerySnapshotCacheIntervalMs = 200;
    public static uint LauncherChallenge => (uint)NetConstants.LauncherChallenge;
    public static uint ProtoChallenge => unchecked((uint)NetConstants.ProtoChallenge);
    public static uint MsgChallenge => (uint)NetConstants.MsgChallenge;
    public static uint OdamexQueryTagId => NetConstants.OdamexQueryTagId;

    public static ServerQuerySnapshotBuildResult GetLocalServerSnapshotNative(
        Func<ServerQuerySnapshotBuildResult> getCached) =>
        getCached();

    public static ServerQuerySnapshotBuildResult GetCachedServerQuerySnapshotNative(
        ref ServerQuerySnapshotCacheState state,
        ulong nowMs,
        Func<ServerQuerySnapshotBuildResult> buildSnapshot)
    {
        if (nowMs - state.LastSnapshotTime > QuerySnapshotCacheIntervalMs || state.LastSnapshotTime == 0)
        {
            state.CachedSnapshot = buildSnapshot();
            state.LastSnapshotTime = nowMs;
        }
        return state.CachedSnapshot;
    }

    public static bool WriteSessionTokenNative(Span<byte> netBuffer, int maxMsgLen, int offset, uint token)
    {
        if (offset + 4 > maxMsgLen)
            return false;
        VmINetPregameService.WriteBe32Native(netBuffer.Slice(offset, 4), token);
        return true;
    }

    public static uint ReadSessionTokenNative(ReadOnlySpan<byte> data, int offset) =>
        VmINetPregameService.ReadBe32Native(data.Slice(offset, 4));

    public static bool TryHandleServerQueryNative(
        uint challenge,
        int msgSize,
        int connectedClients,
        Action<int>? debugMark,
        Func<bool>? sendLauncherInfo)
    {
        if (msgSize < 4)
            return false;

        if (challenge == LauncherChallenge || challenge == ProtoChallenge)
        {
            debugMark?.Invoke(connectedClients);
            return sendLauncherInfo?.Invoke() ?? false;
        }

        if (((challenge >> 20) & 0x0FFFu) == OdamexQueryTagId)
        {
            debugMark?.Invoke(connectedClients);
            return sendLauncherInfo?.Invoke() ?? false;
        }

        return false;
    }

    public static bool EncodeLauncherInfoNative(
        ServerQuerySnapshotBuildResult snapshot,
        ReadOnlySpan<byte> request,
        int msgSize,
        Span<byte> output,
        out int length,
        uint msgChallenge,
        Func<ulong>? getTimeMs)
    {
        length = 0;
        var writer = new NetQueryWriter(output);
        var now = (uint)((getTimeMs?.Invoke() ?? 0) & 0xFFFFFFFF);

        if (!writer.WriteLong(msgChallenge) || !writer.WriteLong(now))
            return false;

        if (msgSize >= 8)
        {
            if (request.Length < 8)
                return false;
            if (!writer.WriteLong(VmINetPregameService.ReadBe32Native(request.Slice(4, 4))))
                return false;
        }

        if (!writer.WriteString(snapshot.HostName)
            || !writer.WriteByte(snapshot.PlayerCount)
            || !writer.WriteByte(snapshot.MaxPlayers)
            || !writer.WriteString(snapshot.MapName)
            || !writer.WriteString(snapshot.SessionState)
            || !writer.WriteByte(snapshot.Deathmatch ? (byte)1 : (byte)0)
            || !writer.WriteByte(snapshot.Skill)
            || !writer.WriteByte(snapshot.Teamplay ? (byte)1 : (byte)0)
            || !writer.WriteShort(snapshot.TimeLeft)
            || !writer.WriteShort(snapshot.FragLimit)
            || !writer.WriteString(snapshot.Version)
            || !writer.WriteString(snapshot.GitHash)
            || !writer.WriteByte((byte)snapshot.Players.Count))
            return false;

        foreach (var player in snapshot.Players)
        {
            if (!writer.WriteString(player.Name)
                || !writer.WriteShort(player.Ping)
                || !writer.WriteShort((ushort)player.Frags)
                || !writer.WriteShort((ushort)player.Kills)
                || !writer.WriteShort((ushort)player.Deaths))
                return false;
        }

        if (!writer.WriteString(snapshot.GameName)
            || !writer.WriteByte(snapshot.GameMode)
            || !writer.WriteString(snapshot.GameModeName)
            || !writer.WriteByte(snapshot.InvasionState)
            || !writer.WriteShort(snapshot.InvasionStateTics)
            || !writer.WriteString(snapshot.InvasionStateName)
            || !writer.WriteShort(snapshot.InvasionWave)
            || !writer.WriteShort(snapshot.InvasionMaxWaves)
            || !writer.WriteShort(snapshot.InvasionWaveBudget)
            || !writer.WriteShort(snapshot.InvasionWaveSpawned)
            || !writer.WriteShort(snapshot.InvasionWaveCleared)
            || !writer.WriteByte(snapshot.InvasionWaveFlags)
            || !writer.WriteShort(snapshot.InvasionSpawnSpotCount)
            || !writer.WriteShort(snapshot.InvasionSpawnActiveSpotCount)
            || !writer.WriteShort(snapshot.InvasionSpawnPlanBudget)
            || !writer.WriteShort(snapshot.InvasionSpawnActiveTag)
            || !writer.WriteByte(snapshot.InvasionSpawnFlags)
            || !writer.WriteShort(snapshot.InvasionActiveMonsters))
            return false;

        length = writer.Offset;
        return true;
    }

    public static bool SendLauncherInfoNative(
        ServerQuerySnapshotBuildResult snapshot,
        ReadOnlySpan<byte> request,
        int msgSize,
        Span<byte> buffer,
        uint msgChallenge,
        Func<ulong>? getTimeMs,
        Func<byte[], int, bool>? sendTo)
    {
        if (!EncodeLauncherInfoNative(snapshot, request, msgSize, buffer, out var length, msgChallenge, getTimeMs))
            return false;
        return sendTo?.Invoke(buffer[..length].ToArray(), length) ?? false;
    }

    private ref struct NetQueryWriter
    {
        private readonly Span<byte> _buffer;
        public int Offset { get; private set; }

        public NetQueryWriter(Span<byte> buffer)
        {
            _buffer = buffer;
            Offset = 0;
        }

        public bool WriteByte(byte value)
        {
            if (Offset + 1 > _buffer.Length)
                return false;
            _buffer[Offset++] = value;
            return true;
        }

        public bool WriteShort(ushort value)
        {
            if (Offset + 2 > _buffer.Length)
                return false;
            BinaryPrimitives.WriteUInt16BigEndian(_buffer.Slice(Offset, 2), value);
            Offset += 2;
            return true;
        }

        public bool WriteLong(uint value)
        {
            if (Offset + 4 > _buffer.Length)
                return false;
            BinaryPrimitives.WriteUInt32BigEndian(_buffer.Slice(Offset, 4), value);
            Offset += 4;
            return true;
        }

        public bool WriteString(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            if (Offset + bytes.Length + 1 > _buffer.Length)
                return false;
            bytes.CopyTo(_buffer.Slice(Offset));
            Offset += bytes.Length;
            _buffer[Offset++] = 0;
            return true;
        }
    }
}
