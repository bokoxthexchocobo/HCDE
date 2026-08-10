using System.Net;
using System.Net.Sockets;

namespace HCDE.Net.Transport;

public readonly record struct NetworkEndpoint(IPAddress Address, int Port)
{
    public static bool TryParse(string addressText, out NetworkEndpoint endpoint, int defaultPort = NetConstants.DefaultGamePort)
    {
        endpoint = default;
        if (string.IsNullOrWhiteSpace(addressText))
            return false;

        var host = addressText;
        var port = defaultPort;
        var colon = addressText.LastIndexOf(':');
        if (colon > 0 && colon < addressText.Length - 1)
        {
            host = addressText[..colon];
            if (!int.TryParse(addressText[(colon + 1)..], out port) || port is <= 0 or > 65535)
                return false;
        }

        if (!IPAddress.TryParse(host, out var address))
        {
            try
            {
                var entries = Dns.GetHostAddresses(host);
                address = entries.FirstOrDefault(entry => entry.AddressFamily == AddressFamily.InterNetwork);
                if (address is null)
                    return false;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        endpoint = new NetworkEndpoint(address, port);
        return true;
    }

    public IPEndPoint ToEndPoint() => new(Address, Port);

    public override string ToString() => $"{Address}:{Port}";
}
