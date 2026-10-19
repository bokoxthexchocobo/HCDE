namespace HCDE.Net.Core;

/// <summary>Native <c>Net_ResetPresentationEchoStateForClient</c> per-slot weapon echo baselines.</summary>
public sealed class NetClientPresentationEchoRegistry
{
    private readonly PresentationEchoLastState[] _lastStates;

    public NetClientPresentationEchoRegistry(int maxClients = NetClientDisconnectSession.MaxPlayers)
    {
        if (maxClients <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxClients));
        _lastStates = new PresentationEchoLastState[maxClients];
    }

    public void ResetClient(int clientNum)
    {
        if (clientNum < 0 || clientNum >= _lastStates.Length)
            return;

        _lastStates[clientNum] = default;
    }

    public void ResetAll()
    {
        for (var i = 0; i < _lastStates.Length; i++)
            _lastStates[i] = default;
    }

    public PresentationEchoLastState GetLastState(int clientNum) =>
        clientNum >= 0 && clientNum < _lastStates.Length ? _lastStates[clientNum] : default;

    public void CommitLastState(int clientNum, in PresentationEchoLastState state)
    {
        if (clientNum < 0 || clientNum >= _lastStates.Length)
            return;
        _lastStates[clientNum] = state;
    }
}
