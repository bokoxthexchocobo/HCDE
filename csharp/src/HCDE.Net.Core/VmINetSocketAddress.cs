namespace HCDE.Net.Core;

/// <summary>UDP socket helpers and query byte readers (<c>i_net.cpp</c> 1095–1261).</summary>
public static class VmINetSocketAddress
{
    public static int CreateUdpSocketNative(
        Func<int>? createSocket,
        Action<int>? tryDisableUdpConnReset,
        Action<string>? fatalError)
    {
        var socket = createSocket?.Invoke() ?? -1;
        if (socket < 0)
        {
            fatalError?.Invoke("Couldn't create socket");
            return -1;
        }
        tryDisableUdpConnReset?.Invoke(socket);
        return socket;
    }

    public static void BindToLocalPortNative(
        int socket,
        ushort port,
        Action<int, ushort>? bind,
        Action<string>? fatalError)
    {
        if (bind is null)
            return;
        try
        {
            bind(socket, port);
        }
        catch
        {
            fatalError?.Invoke("Couldn't bind to port");
        }
    }

    public static bool ValidateNetPasswordCvarNative(string value, int maxPasswordSize, Action<string>? printError)
    {
        if (value.Length + 1 > maxPasswordSize)
        {
            printError?.Invoke("Password cannot be greater than 255 characters\n");
            return false;
        }
        return true;
    }

    public static bool TryBuildAddressNative(
        string addrName,
        ushort defaultPort,
        Func<string, string?, bool>? tryResolveHost,
        out NetworkAddressParts address,
        Action<string>? setError)
    {
        address = default;
        var target = "";
        var port = defaultPort;
        var portSep = addrName.IndexOf(':');
        if (portSep >= 0)
        {
            target = addrName[..portSep];
            if (!ushort.TryParse(addrName[(portSep + 1)..], out var portConversion) || portConversion == 0)
            {
                setError?.Invoke($"Malformed port: {addrName[(portSep + 1)..]}");
                return false;
            }
            port = portConversion;
        }
        else
            target = addrName;

        if (tryResolveHost is null || !tryResolveHost(target, null))
        {
            setError?.Invoke($"getaddrinfo: Couldn't find {target}");
            return false;
        }

        address.Port = port;
        address.Host = target;
        return true;
    }

    public static void BuildAddressNative(
        string addrName,
        ushort defaultPort,
        Func<string, string?, bool>? tryResolveHost,
        Action<string>? fatalError)
    {
        if (!TryBuildAddressNative(addrName, defaultPort, tryResolveHost, out _, msg => fatalError?.Invoke(msg)))
            fatalError?.Invoke("address resolution failed");
    }

    public static bool ReadQueryByteNative(ReadOnlySpan<byte> data, ref int offset, int limit, out byte value)
    {
        value = 0;
        if (offset + 1 > limit)
            return false;
        value = data[offset++];
        return true;
    }

    public static bool ReadQueryShortNative(ReadOnlySpan<byte> data, ref int offset, int limit, out ushort value)
    {
        value = 0;
        if (offset + 2 > limit)
            return false;
        value = (ushort)(data[offset] << 8 | data[offset + 1]);
        offset += 2;
        return true;
    }

    public static bool ReadQueryStringNative(ReadOnlySpan<byte> data, ref int offset, int limit, out string value)
    {
        value = "";
        if (offset >= limit)
            return false;

        var start = offset;
        while (offset < limit && data[offset] != 0)
            offset++;

        if (offset >= limit)
            return false;

        value = System.Text.Encoding.UTF8.GetString(data.Slice(start, offset - start));
        offset++;
        return true;
    }
}

public struct NetworkAddressParts
{
    public string Host;
    public ushort Port;
}
