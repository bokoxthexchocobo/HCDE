namespace HCDE.Playsim;

/// <summary>Native player-start bookkeeping from <c>SpawnMapThing</c> (deathmatch starts and coop player slots).</summary>
public sealed class MapThingPlayerStartRegistrar
{
    public sealed record PlayerStart(int PlayerNumber, int PositionArg, double X, double Y, double Z, int Angle);

    private readonly List<PlayerStart> _deathmatchStarts = new();
    private readonly PlayerStart?[] _playerStarts;
    private readonly List<PlayerStart> _allPlayerStarts = new();

    public MapThingPlayerStartRegistrar(int maxPlayers)
    {
        if (maxPlayers <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPlayers));

        _playerStarts = new PlayerStart?[maxPlayers];
    }

    public IReadOnlyList<PlayerStart> DeathmatchStarts => _deathmatchStarts;

    public IReadOnlyList<PlayerStart> AllPlayerStarts => _allPlayerStarts;

    public bool TryRegisterDeathmatchStart(PlayerStart start)
    {
        _deathmatchStarts.Add(start);
        return true;
    }

    public bool TryRegisterPlayerStart(int playerIndex, int requestedPosition, PlayerStart start, bool randomPlayerStarts, out PlayerStart? selectedForSpawn)
    {
        selectedForSpawn = null;
        if (playerIndex < 0 || playerIndex >= _playerStarts.Length)
            return false;

        if (start.PositionArg != requestedPosition)
            return false;

        _playerStarts[playerIndex] = start;
        if (randomPlayerStarts)
        {
            _allPlayerStarts.Add(start);
        }
        else
        {
            var playerNumber = start.PlayerNumber;
            var replaced = false;
            for (var i = 0; i < _allPlayerStarts.Count; i++)
            {
                if (_allPlayerStarts[i].PlayerNumber == playerNumber)
                {
                    _allPlayerStarts[i] = start;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
                _allPlayerStarts.Add(start);
        }

        selectedForSpawn = start;
        return true;
    }

    public PlayerStart? GetPlayerStart(int playerIndex) =>
        playerIndex >= 0 && playerIndex < _playerStarts.Length ? _playerStarts[playerIndex] : null;
}
