namespace HCDE.Net.Core.Tests;

public class VmINetAuthorityLiveTests
{
    [Fact]
    public void IGetVisibleMaxClientsNative_SubtractsReservedSlot()
    {
        var ctx = new NetClientSlotContext
        {
            MaxClients = 4,
            Reserved = new ReservedServerSlotContext
            {
                GetHcdeServiceAuthoritySlot = () => 0,
                UsesDedicatedServerSlot = () => true,
            },
        };
        Assert.Equal(3, VmINetAuthorityLive.IGetVisibleMaxClientsNative(ctx));
    }

    [Fact]
    public void NetErrorNative_MapsConnRefused()
    {
        Assert.Equal("ECONNREFUSED", VmINetWinNetError.NetErrorNative(() => 10061));
    }
}
