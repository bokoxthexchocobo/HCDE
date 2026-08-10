using System.Buffers.Binary;
using System.Text;

namespace HCDE.Net.Transport;

public static class ServerQueryCodec
{
    public static byte[] CreateLauncherChallengeRequest(uint echoToken = 0)
    {
        var packet = new byte[echoToken == 0 ? 4 : 8];
        BinaryPrimitives.WriteUInt32BigEndian(packet, (uint)NetConstants.LauncherChallenge);
        if (echoToken != 0)
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(4), echoToken);
        return packet;
    }

    public static bool TryWriteResponse(ServerQuerySnapshot snapshot, uint echoToken, Span<byte> buffer, out int length)
    {
        var writer = new QueryWriter(buffer);
        if (!writer.WriteUInt32((uint)NetConstants.MsgChallenge)
            || !writer.WriteUInt32((uint)(Environment.TickCount64 & 0xFFFFFFFF))
            || (echoToken != 0 && !writer.WriteUInt32(echoToken))
            || !writer.WriteString(snapshot.HostName)
            || !writer.WriteByte(snapshot.PlayerCount)
            || !writer.WriteByte(snapshot.MaxPlayers)
            || !writer.WriteString(snapshot.MapName)
            || !writer.WriteString(snapshot.SessionState)
            || !writer.WriteByte((byte)(snapshot.Deathmatch ? 1 : 0))
            || !writer.WriteByte(snapshot.Skill)
            || !writer.WriteByte((byte)(snapshot.Teamplay ? 1 : 0))
            || !writer.WriteUInt16(snapshot.TimeLeft)
            || !writer.WriteUInt16(snapshot.FragLimit)
            || !writer.WriteString(snapshot.Version)
            || !writer.WriteString(snapshot.GitHash)
            || !writer.WriteByte((byte)snapshot.Players.Count))
        {
            length = 0;
            return false;
        }

        foreach (var player in snapshot.Players)
        {
            if (!writer.WriteString(player.Name)
                || !writer.WriteUInt16(player.Ping)
                || !writer.WriteUInt16((ushort)player.Frags)
                || !writer.WriteUInt16((ushort)player.Kills)
                || !writer.WriteUInt16((ushort)player.Deaths))
            {
                length = 0;
                return false;
            }
        }

        if (!writer.WriteString(snapshot.GameName)
            || !writer.WriteByte(snapshot.GameMode)
            || !writer.WriteString(snapshot.GameModeName)
            || !writer.WriteByte(snapshot.InvasionState)
            || !writer.WriteUInt16(snapshot.InvasionStateTics)
            || !writer.WriteString(snapshot.InvasionStateName)
            || !writer.WriteUInt16(snapshot.InvasionWave)
            || !writer.WriteUInt16(snapshot.InvasionMaxWaves)
            || !writer.WriteUInt16(snapshot.InvasionWaveBudget)
            || !writer.WriteUInt16(snapshot.InvasionWaveSpawned)
            || !writer.WriteUInt16(snapshot.InvasionWaveCleared)
            || !writer.WriteByte(snapshot.InvasionWaveFlags)
            || !writer.WriteUInt16(snapshot.InvasionSpawnSpotCount)
            || !writer.WriteUInt16(snapshot.InvasionSpawnActiveSpotCount)
            || !writer.WriteUInt16(snapshot.InvasionSpawnPlanBudget)
            || !writer.WriteUInt16(snapshot.InvasionSpawnActiveTag)
            || !writer.WriteByte(snapshot.InvasionSpawnFlags)
            || !writer.WriteUInt16(snapshot.InvasionActiveMonsters))
        {
            length = 0;
            return false;
        }

        length = writer.Offset;
        return true;
    }

    public static bool TryReadResponse(ReadOnlySpan<byte> data, out ServerQuerySnapshot snapshot, out string? error)
    {
        snapshot = new ServerQuerySnapshot();
        error = null;

        if (data.Length < 8)
        {
            error = "Query reply was too short";
            return false;
        }

        var challenge = BinaryPrimitives.ReadUInt32BigEndian(data);
        if (challenge != (uint)NetConstants.MsgChallenge)
        {
            error = $"Unexpected query reply header: {challenge}";
            return false;
        }

        var offset = 8;
        string hostName, mapName, sessionState, version, gitHash;
        byte playerCount, maxPlayers, skill;
        ushort timeLeft, fragLimit;

        if (!TryReadString(data, ref offset, out hostName)
            || !TryReadByte(data, ref offset, out playerCount)
            || !TryReadByte(data, ref offset, out maxPlayers)
            || !TryReadString(data, ref offset, out mapName)
            || !TryReadString(data, ref offset, out sessionState)
            || !TryReadByte(data, ref offset, out var deathmatch)
            || !TryReadByte(data, ref offset, out skill)
            || !TryReadByte(data, ref offset, out var teamplay)
            || !TryReadUInt16(data, ref offset, out timeLeft)
            || !TryReadUInt16(data, ref offset, out fragLimit)
            || !TryReadString(data, ref offset, out version)
            || !TryReadString(data, ref offset, out gitHash))
        {
            error = "Query reply was truncated";
            return false;
        }

        snapshot.HostName = hostName;
        snapshot.PlayerCount = playerCount;
        snapshot.MaxPlayers = maxPlayers;
        snapshot.MapName = mapName;
        snapshot.SessionState = sessionState;
        snapshot.Skill = skill;
        snapshot.TimeLeft = timeLeft;
        snapshot.FragLimit = fragLimit;
        snapshot.Version = version;
        snapshot.GitHash = gitHash;
        snapshot.Deathmatch = deathmatch != 0;
        snapshot.Teamplay = teamplay != 0;

        if (!TryReadByte(data, ref offset, out var listedPlayers))
        {
            error = "Query reply was truncated";
            return false;
        }

        snapshot.Players.Clear();
        for (var i = 0; i < listedPlayers; i++)
        {
            var player = new ServerQueryPlayer();
            string playerName;
            ushort ping, frags, kills, deaths;
            if (!TryReadString(data, ref offset, out playerName)
                || !TryReadUInt16(data, ref offset, out ping)
                || !TryReadUInt16(data, ref offset, out frags)
                || !TryReadUInt16(data, ref offset, out kills)
                || !TryReadUInt16(data, ref offset, out deaths))
            {
                error = "Query player list was truncated";
                return false;
            }

            player.Name = playerName;
            player.Ping = ping;
            player.Frags = (short)frags;
            player.Kills = (short)kills;
            player.Deaths = (short)deaths;
            snapshot.Players.Add(player);
        }

        if (offset < data.Length && TryReadString(data, ref offset, out var gameName))
            snapshot.GameName = gameName;

        if (offset < data.Length
            && TryReadByte(data, ref offset, out var gameMode)
            && TryReadString(data, ref offset, out var gameModeName))
        {
            snapshot.GameMode = gameMode;
            snapshot.GameModeName = gameModeName;
        }

        if (offset < data.Length
            && TryReadByte(data, ref offset, out var invasionState)
            && TryReadUInt16(data, ref offset, out var invasionStateTics)
            && TryReadString(data, ref offset, out var invasionStateName)
            && TryReadUInt16(data, ref offset, out var invasionWave)
            && TryReadUInt16(data, ref offset, out var invasionMaxWaves)
            && TryReadUInt16(data, ref offset, out var invasionWaveBudget)
            && TryReadUInt16(data, ref offset, out var invasionWaveSpawned)
            && TryReadUInt16(data, ref offset, out var invasionWaveCleared)
            && TryReadByte(data, ref offset, out var invasionWaveFlags)
            && TryReadUInt16(data, ref offset, out var invasionSpawnSpotCount)
            && TryReadUInt16(data, ref offset, out var invasionSpawnActiveSpotCount)
            && TryReadUInt16(data, ref offset, out var invasionSpawnPlanBudget)
            && TryReadUInt16(data, ref offset, out var invasionSpawnActiveTag)
            && TryReadByte(data, ref offset, out var invasionSpawnFlags)
            && TryReadUInt16(data, ref offset, out var invasionActiveMonsters))
        {
            snapshot.InvasionState = invasionState;
            snapshot.InvasionStateTics = invasionStateTics;
            snapshot.InvasionStateName = invasionStateName;
            snapshot.InvasionWave = invasionWave;
            snapshot.InvasionMaxWaves = invasionMaxWaves;
            snapshot.InvasionWaveBudget = invasionWaveBudget;
            snapshot.InvasionWaveSpawned = invasionWaveSpawned;
            snapshot.InvasionWaveCleared = invasionWaveCleared;
            snapshot.InvasionWaveFlags = invasionWaveFlags;
            snapshot.InvasionSpawnSpotCount = invasionSpawnSpotCount;
            snapshot.InvasionSpawnActiveSpotCount = invasionSpawnActiveSpotCount;
            snapshot.InvasionSpawnPlanBudget = invasionSpawnPlanBudget;
            snapshot.InvasionSpawnActiveTag = invasionSpawnActiveTag;
            snapshot.InvasionSpawnFlags = invasionSpawnFlags;
            snapshot.InvasionActiveMonsters = invasionActiveMonsters;
        }

        return true;
    }

    private static bool TryReadByte(ReadOnlySpan<byte> data, ref int offset, out byte value)
    {
        value = 0;
        if (offset + 1 > data.Length)
            return false;
        value = data[offset++];
        return true;
    }

    private static bool TryReadUInt16(ReadOnlySpan<byte> data, ref int offset, out ushort value)
    {
        value = 0;
        if (offset + 2 > data.Length)
            return false;
        value = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
        offset += 2;
        return true;
    }

    private static bool TryReadString(ReadOnlySpan<byte> data, ref int offset, out string value)
    {
        value = string.Empty;
        if (offset >= data.Length)
            return false;

        var start = offset;
        while (offset < data.Length && data[offset] != 0)
            offset++;

        if (offset >= data.Length)
            return false;

        value = Encoding.UTF8.GetString(data[start..offset]);
        offset++;
        return true;
    }

    private ref struct QueryWriter
    {
        private readonly Span<byte> _buffer;
        public int Offset { get; private set; }

        public QueryWriter(Span<byte> buffer)
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

        public bool WriteUInt16(ushort value)
        {
            if (Offset + 2 > _buffer.Length)
                return false;
            BinaryPrimitives.WriteUInt16BigEndian(_buffer[Offset..], value);
            Offset += 2;
            return true;
        }

        public bool WriteUInt32(uint value)
        {
            if (Offset + 4 > _buffer.Length)
                return false;
            BinaryPrimitives.WriteUInt32BigEndian(_buffer[Offset..], value);
            Offset += 4;
            return true;
        }

        public bool WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (Offset + bytes.Length + 1 > _buffer.Length)
                return false;
            bytes.CopyTo(_buffer[Offset..]);
            Offset += bytes.Length;
            _buffer[Offset++] = 0;
            return true;
        }
    }
}
