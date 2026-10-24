namespace HCDE.Net.Core;

/// <summary>Native <c>DisconnectClient</c> slot teardown beyond trimmed chunk-13 credit.</summary>
public sealed class NetClientDisconnectTeardown
{
    public sealed class PlayerSlotFlags
    {
        public bool PlayingInGame { get; set; }
        public bool Waiting { get; set; }
        public bool Inconsistent { get; set; }
        public bool SettingsController { get; set; }
        public NetPlayerLifecycleState Lifecycle { get; set; } = NetPlayerLifecycleState.Live;
    }

    private readonly NetClientDisconnectSession _session;
    private readonly PlayerSlotFlags[] _players;
    private readonly NetClientChannelResetState[] _channels;

    public NetClientDisconnectTeardown(NetClientDisconnectSession session, int maxPlayers = NetClientDisconnectSession.MaxPlayers)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _players = new PlayerSlotFlags[maxPlayers];
        _channels = new NetClientChannelResetState[maxPlayers];
        for (var i = 0; i < maxPlayers; i++)
        {
            _players[i] = new PlayerSlotFlags();
            _channels[i] = new NetClientChannelResetState();
        }
    }

    public NetClientChannelResetState Channel(int client) => _channels[client];

    public PlayerSlotFlags Player(int client) => _players[client];

    public bool TryDisconnect(int clientNum, string reason, Action<int>? clearPresentationEcho = null)
    {
        if (clientNum < 0 || clientNum >= _players.Length)
            return false;

        if (!_session.IsInGame(clientNum))
            return false;

        _session.LateJoin.ClearLateJoinTargets(clientNum);
        var player = _players[clientNum];
        player.PlayingInGame = false;
        player.Waiting = false;
        player.Inconsistent = false;
        player.SettingsController = false;
        player.Lifecycle = NetPlayerLifecycleState.Gone;

        if (!_session.TryDisconnect(clientNum, clearPresentationEcho))
            return false;

        _channels[clientNum].Reset();
        return true;
    }

    public void ResetClientChannel(int clientNum)
    {
        if (clientNum < 0 || clientNum >= _channels.Length)
            return;

        _session.ResetClientState(clientNum);
        _channels[clientNum].Reset();
    }
}

public enum NetPlayerLifecycleState
{
    Live,
    Gone,
}
