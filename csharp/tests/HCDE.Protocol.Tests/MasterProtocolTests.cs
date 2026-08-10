using System.Net;
using HCDE.Protocol;

namespace HCDE.Protocol.Tests;

public class MasterProtocolTests
{
    [Fact]
    public void ConstantsMatchCppHeader()
    {
        Assert.Equal(2u, MasterProtocol.Version);
        Assert.Equal("hcde.servebeer.com", MasterProtocol.DefaultMasterHost);
        Assert.Equal((ushort)15000, MasterProtocol.DefaultMasterPort);
        Assert.Equal(5560020u, MasterProtocol.ServerHeartbeatMarker);
        Assert.Equal(777123u, MasterProtocol.LauncherListQueryMarker);
        Assert.Equal((ushort)6, MasterProtocol.ServerHeartbeatPacketSize);
    }
}

public class MasterPacketTests
{
    [Fact]
    public void ServerHeartbeatRoundTrip()
    {
        var packet = MasterPackets.CreateServerHeartbeat(10666);
        Assert.True(MasterPackets.TryReadServerHeartbeat(packet, out var port));
        Assert.Equal((ushort)10666, port);
    }

    [Fact]
    public void LauncherListQueryRoundTrip()
    {
        var packet = MasterPackets.CreateLauncherListQuery();
        Assert.True(MasterPackets.TryReadLauncherListQuery(packet));
    }

    [Fact]
    public void MasterListResponseRoundTrip()
    {
        var servers = new[]
        {
            new MasterServerEntry(IPAddress.Parse("192.168.1.10"), 10666),
            new MasterServerEntry(IPAddress.Parse("10.0.0.5"), 10667),
        };

        var packet = MasterPackets.CreateMasterListResponse(servers);
        Assert.True(MasterPackets.TryReadMasterListResponse(packet, out var decoded));
        Assert.Equal(2, decoded.Count);
        Assert.Equal(servers[0].Address, decoded[0].Address);
        Assert.Equal(servers[0].Port, decoded[0].Port);
        Assert.Equal(servers[1].Address, decoded[1].Address);
        Assert.Equal(servers[1].Port, decoded[1].Port);
    }
}
