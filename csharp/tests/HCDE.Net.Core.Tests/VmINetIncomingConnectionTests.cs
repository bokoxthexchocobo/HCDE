namespace HCDE.Net.Core.Tests;

public class VmINetIncomingConnectionTests
{
    [Fact]
    public void CallsGuestHandlerWhenNotAuthority()
    {
        var guest = false;
        VmINetIncomingConnection.HandleIncomingConnectionNative(new HandleIncomingConnectionContext
        {
            IsLocalHcdeServiceAuthority = () => false,
            HandleGuestRuntimeHcdeService = () => guest = true,
        });
        Assert.True(guest);
    }
}
