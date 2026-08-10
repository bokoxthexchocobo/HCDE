using System.Net;
using System.Net.Sockets;
using HCDE.Master;
using HCDE.Protocol;

namespace HCDE.Master.Tests;

public class MasterServerTests
{
    [Fact]
    public async Task HeartbeatAndQueryReturnRegisteredServer()
    {
        var options = new MasterServerOptions
        {
            BindAddress = "127.0.0.1",
            Port = 0,
            TtlSeconds = 30,
            MaxPackets = 2,
            Quiet = true,
        };

        var server = new MasterServer(options);
        var runTask = Task.Run(() => server.RunAsync());

        await WaitForAsync(() => server.BoundPort > 0);

        using var masterClient = new UdpClient();
        var heartbeat = MasterPackets.CreateServerHeartbeat(10666);
        await masterClient.SendAsync(heartbeat, heartbeat.Length, "127.0.0.1", server.BoundPort);

        var query = MasterPackets.CreateLauncherListQuery();
        await masterClient.SendAsync(query, query.Length, "127.0.0.1", server.BoundPort);

        var response = await masterClient.ReceiveAsync();
        Assert.True(MasterPackets.TryReadMasterListResponse(response.Buffer, out var servers));
        Assert.Contains(servers, entry => entry.Port == 10666);

        server.RequestStop();
        await runTask;
    }

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (!condition())
        {
            if (Environment.TickCount64 >= deadline)
                throw new TimeoutException("condition was not met in time");

            await Task.Delay(10);
        }
    }
}
