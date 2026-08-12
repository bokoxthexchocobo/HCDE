using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public readonly struct LiveAuthorityClient
{
    public LiveAuthorityClient(NetworkEndpoint endpoint, int clientSlot)
    {
        Endpoint = endpoint;
        ClientSlot = clientSlot;
    }

    public NetworkEndpoint Endpoint { get; }
    public int ClientSlot { get; }
}

public sealed class LiveAuthorityClientRegistry
{
    private readonly List<LiveAuthorityClient> _clients = new();

    public IReadOnlyList<LiveAuthorityClient> Clients => _clients;

    public void Track(NetworkEndpoint endpoint, int clientSlot)
    {
        for (var i = 0; i < _clients.Count; i++)
        {
            if (_clients[i].ClientSlot == clientSlot)
            {
                _clients[i] = new LiveAuthorityClient(endpoint, clientSlot);
                return;
            }
        }

        _clients.Add(new LiveAuthorityClient(endpoint, clientSlot));
    }

    public bool Remove(int clientSlot)
    {
        for (var i = 0; i < _clients.Count; i++)
        {
            if (_clients[i].ClientSlot != clientSlot)
                continue;

            _clients.RemoveAt(i);
            return true;
        }

        return false;
    }

    public void Clear() => _clients.Clear();
}
