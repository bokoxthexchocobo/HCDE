using HCDE.Net.Transport;

namespace HCDE.Net.Core;

/// <summary><c>TryReadServerQuerySnapshot</c> decode path (<c>i_net.cpp</c> 1263–1466).</summary>
public static class VmINetQueryDecode
{
    public const byte InvsDisabled = 0;

    public static bool QueryReplyTooShort(int length) => length < 8;

    public static bool QueryHeaderChallengeValid(uint challenge) =>
        challenge == (uint)NetConstants.MsgChallenge;

    public static bool TryReadServerQuerySnapshotNative(
        ReadOnlySpan<byte> data,
        int length,
        ServerQuerySnapshotDecodeTarget snapshot,
        Action<string>? setError)
    {
        if (QueryReplyTooShort(length))
        {
            setError?.Invoke("Query reply was too short");
            return false;
        }

        var challenge = VmINetPregameService.ReadBe32Native(data);
        if (!QueryHeaderChallengeValid(challenge))
        {
            setError?.Invoke($"Unexpected query reply header: {challenge}");
            return false;
        }

        var offset = 8;
        byte deathmatch = 0, skill = 0, teamplay = 0;
        if (!VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var hostName)
            || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var playerCount)
            || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var maxPlayers)
            || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var mapName)
            || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var sessionState)
            || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out deathmatch)
            || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out skill)
            || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out teamplay)
            || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var timeLeft)
            || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var fragLimit)
            || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var version)
            || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var gitHash))
        {
            setError?.Invoke("Query reply was truncated");
            return false;
        }

        snapshot.HostName = hostName;
        snapshot.PlayerCount = playerCount;
        snapshot.MaxPlayers = maxPlayers;
        snapshot.MapName = mapName;
        snapshot.SessionState = sessionState;
        snapshot.Deathmatch = deathmatch != 0;
        snapshot.Skill = skill;
        snapshot.Teamplay = teamplay != 0;
        snapshot.TimeLeft = timeLeft;
        snapshot.FragLimit = fragLimit;
        snapshot.Version = version;
        snapshot.GitHash = gitHash;
        snapshot.GameMode = 0;
        snapshot.GameModeName = VmINetConnectQuery.ServerGameModeNameNative(
            snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay);
        snapshot.InvasionState = InvsDisabled;
        snapshot.InvasionStateTics = 0;
        snapshot.InvasionStateName = VmINetConnectQuery.ServerInvasionStateNameNative(snapshot.InvasionState);
        snapshot.InvasionWave = 0;
        snapshot.InvasionMaxWaves = 0;
        snapshot.InvasionWaveBudget = 0;
        snapshot.InvasionWaveSpawned = 0;
        snapshot.InvasionWaveCleared = 0;
        snapshot.InvasionWaveFlags = 0;
        snapshot.InvasionSpawnSpotCount = 0;
        snapshot.InvasionSpawnActiveSpotCount = 0;
        snapshot.InvasionSpawnPlanBudget = 0;
        snapshot.InvasionSpawnActiveTag = 0;
        snapshot.InvasionSpawnFlags = 0;
        snapshot.InvasionActiveMonsters = 0;

        if (!VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var listedPlayers))
        {
            setError?.Invoke("Query reply was truncated");
            return false;
        }
        snapshot.PlayerCount = listedPlayers;
        snapshot.Players.Clear();

        for (var i = 0; i < listedPlayers; i++)
        {
            if (!VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var playerName)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var ping)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var frags)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var kills)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var deaths))
            {
                setError?.Invoke("Query player list was truncated");
                return false;
            }

            snapshot.Players.Add(new ServerQueryPlayerRow
            {
                Name = playerName,
                Ping = ping,
                Frags = (short)frags,
                Kills = (short)kills,
                Deaths = (short)deaths,
            });
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var gameName))
            {
                setError?.Invoke("Query game name was truncated");
                return false;
            }
            snapshot.GameName = gameName;
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var gameMode)
                || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var gameModeName))
            {
                setError?.Invoke("Query game mode was truncated");
                return false;
            }
            snapshot.GameMode = gameMode;
            snapshot.GameModeName = !string.IsNullOrEmpty(gameModeName)
                ? gameModeName
                : VmINetConnectQuery.ServerGameModeNameNative(snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay);
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var invasionState)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionStateTics)
                || !VmINetSocketAddress.ReadQueryStringNative(data, ref offset, length, out var invasionStateName))
            {
                setError?.Invoke("Query invasion state was truncated");
                return false;
            }
            snapshot.InvasionState = invasionState;
            snapshot.InvasionStateTics = invasionStateTics;
            snapshot.InvasionStateName = !string.IsNullOrEmpty(invasionStateName)
                ? invasionStateName
                : VmINetConnectQuery.ServerInvasionStateNameNative(invasionState);
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionWave)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionMaxWaves)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionWaveBudget)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionWaveSpawned)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionWaveCleared)
                || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var invasionWaveFlags))
            {
                setError?.Invoke("Query invasion wave metadata was truncated");
                return false;
            }
            snapshot.InvasionWave = invasionWave;
            snapshot.InvasionMaxWaves = invasionMaxWaves;
            snapshot.InvasionWaveBudget = invasionWaveBudget;
            snapshot.InvasionWaveSpawned = invasionWaveSpawned;
            snapshot.InvasionWaveCleared = invasionWaveCleared;
            snapshot.InvasionWaveFlags = invasionWaveFlags;
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionSpawnSpotCount)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionSpawnActiveSpotCount)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionSpawnPlanBudget)
                || !VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionSpawnActiveTag)
                || !VmINetSocketAddress.ReadQueryByteNative(data, ref offset, length, out var invasionSpawnFlags))
            {
                setError?.Invoke("Query invasion spawn metadata was truncated");
                return false;
            }
            snapshot.InvasionSpawnSpotCount = invasionSpawnSpotCount;
            snapshot.InvasionSpawnActiveSpotCount = invasionSpawnActiveSpotCount;
            snapshot.InvasionSpawnPlanBudget = invasionSpawnPlanBudget;
            snapshot.InvasionSpawnActiveTag = invasionSpawnActiveTag;
            snapshot.InvasionSpawnFlags = invasionSpawnFlags;
        }

        if (offset < length)
        {
            if (!VmINetSocketAddress.ReadQueryShortNative(data, ref offset, length, out var invasionActiveMonsters))
            {
                setError?.Invoke("Query invasion active-monster metadata was truncated");
                return false;
            }
            snapshot.InvasionActiveMonsters = invasionActiveMonsters;
        }

        return true;
    }
}
