namespace HCDE.Net.Core.Tests;

public class VmINetInitNetworkTests
{
    [Fact]
    public void IInitNetworkNative_SinglePlayerPath()
    {
        var started = false;
        Assert.True(VmINetInitNetwork.IInitNetworkNative(new IInitNetworkContext
        {
            InitServerModeFromArgs = () => { },
            IsDedicatedServer = () => false,
            IsDedicatedJoin = () => false,
            SetDedicatedServerMode = v => { },
            SetDedicatedJoinMode = _ => { },
            ResetGuestSetupProgress = () => { },
            ShouldSuppressRoomUi = () => false,
            SetSilentNetStart = _ => { },
            TryParseDup = _ => (false, false, 0),
            TryParsePort = _ => (false, false, 0),
            GenerateGameId = () => { },
            SetTicDup = _ => { },
            AddNetworkClient = _ => { },
            SetHostReady = _ => { },
            SetupUserInfo = () => { },
            SetGameStarted = v => started = v,
        }));
        Assert.True(started);
    }

    [Fact]
    public void IGetReservedServerSlotNative_ReturnsAuthorityForDedicated()
    {
        var slot = VmINetInitNetwork.IGetReservedServerSlotNative(new ReservedServerSlotContext
        {
            GetHcdeServiceAuthoritySlot = () => 0,
            UsesDedicatedServerSlot = () => true,
        });
        Assert.Equal(0, slot);
    }
}
