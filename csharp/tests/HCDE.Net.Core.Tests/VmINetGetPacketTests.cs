namespace HCDE.Net.Core.Tests;

public class VmINetGetPacketTests
{
    [Fact]
    public void GetPacketNative_ClearsClientOnWouldBlock()
    {
        var ctx = new GetPacketNativeContext
        {
            ConsolePlayer = 0,
            WsaWouldBlock = 10035,
            RecvFrom = (_, _) => (VmINetGetPacket.SocketError, (0u, (ushort)0)),
            FindClient = _ => 1,
            GetLastSocketError = () => 10035,
        };
        VmINetGetPacket.GetPacketNative(ctx);
        Assert.Equal(-1, ctx.RemoteClient);
        Assert.Equal(0, ctx.NetBufferLength);
    }

    [Fact]
    public void GetPacketNative_ReturnsEarlyOnServerQuery()
    {
        var ctx = new GetPacketNativeContext
        {
            TransmitBuffer = [0, 0, 0, 0, 0],
            RecvFrom = (_, _) => (8, (0x7F000001u, (ushort)5029)),
            FindClient = _ => -1,
            TryHandleServerQuery = (_, _, _) => true,
            CaptureFromAddress = true,
        };
        VmINetGetPacket.GetPacketNative(ctx);
        Assert.Equal(-1, ctx.RemoteClient);
        Assert.Equal(0, ctx.NetBufferLength);
    }

    private static int SocketErrorCase() => VmINetGetPacket.SocketError;
}
