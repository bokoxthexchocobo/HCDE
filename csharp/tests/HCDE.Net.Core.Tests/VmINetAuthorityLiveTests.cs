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

    [Fact]
    public void IIsPlayableClientSlotNative_RejectsReservedSlot()
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
        Assert.False(VmINetAuthorityLive.IIsPlayableClientSlotNative(ctx, 0));
        Assert.True(VmINetAuthorityLive.IIsPlayableClientSlotNative(ctx, 1));
        Assert.True(VmINetAuthorityLive.IVisibleClientSlotValidNative(ctx, 2));
        Assert.False(VmINetAuthorityLive.IVisibleClientSlotValidNative(ctx, 3));
    }
}
