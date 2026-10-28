namespace HCDE.Playsim;

/// <summary>Native <c>G_Ticker</c> game-action loop and player lifecycle for <see cref="HeadlessGameSession"/>.</summary>
public sealed class HeadlessGameTicker
{
    private readonly HeadlessGameSession _session;
    private readonly Dictionary<byte, HeadlessPlayerState> _playerStates = new();

    public HeadlessGameTicker(HeadlessGameSession session) => _session = session;

    public IReadOnlyDictionary<byte, HeadlessPlayerState> PlayerStates => _playerStates;

    public void MarkEnter(byte playerNum) => _playerStates[playerNum] = HeadlessPlayerState.Enter;

    public void Tick()
    {
        if (_session.State != HeadlessGameState.Level || _session.Simulation is null)
        {
            _session.Tick();
            return;
        }

        HeadlessGameFlow.TryConsumeReloadRequest(_session);
        HeadlessGameFlow.ProcessPlayerLifecycle(_session.Simulation, _playerStates);
        foreach (var player in _session.Simulation.Players)
        {
            if (!player.Destroyed && !player.IsDead && _playerStates.GetValueOrDefault(player.PlayerNum) != HeadlessPlayerState.Live)
                _playerStates[player.PlayerNum] = HeadlessPlayerState.Live;
        }

        _session.Tick();
    }
}
