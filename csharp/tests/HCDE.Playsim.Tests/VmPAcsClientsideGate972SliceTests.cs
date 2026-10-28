namespace HCDE.Playsim.Tests;

public class VmPAcsClientsideGate972SliceTests
{
    [Fact]
    public void GetScriptOwner_UsesPlayerMoWhenActivatorHasPlayer()
    {
        var mo = new object();
        var activator = new object();
        Assert.Same(mo, VmPAcsClientsideGate972Slice.GetScriptOwner(activator, true, mo));
        Assert.Same(activator, VmPAcsClientsideGate972Slice.GetScriptOwner(activator, false, mo));
    }

    [Fact]
    public void ShouldIgnoreClientSideScript_SuppressesOtherPlayerScripts()
    {
        var owner = new object();
        Assert.True(VmPAcsClientsideGate972Slice.ShouldIgnoreClientSideScript(
            activatorNull: false,
            activatorIsClientSide: false,
            owner: owner,
            ownerHasPlayer: true,
            levelNull: false,
            isConsolePlayer: _ => false));
    }

    [Fact]
    public void ShouldIgnoreClientSideScript_AllowsWorldActivator()
    {
        Assert.False(VmPAcsClientsideGate972Slice.ShouldIgnoreClientSideScript(
            activatorNull: false,
            activatorIsClientSide: false,
            owner: new object(),
            ownerHasPlayer: false,
            levelNull: false,
            isConsolePlayer: _ => false));
        Assert.True(VmPAcsClientsideGate972Slice.ActivatorIsNonPlayerWorld(new object(), false));
    }

    [Fact]
    public void StringBuilderStackPolicy()
    {
        Assert.True(VmPAcsClientsideGate972Slice.StringBuilderShouldPush("x", 0));
        Assert.True(VmPAcsClientsideGate972Slice.StringBuilderPopSucceeded(1));
    }

    [Fact]
    public void HudDefaults_AndWaitStates()
    {
        Assert.Equal(320, VmPAcsClientsideGate972Slice.ResolveHudWidth(0));
        Assert.True(VmPAcsClientsideGate972Slice.ScriptStateIsWait(
            VmPAcsClientsideGate972Slice.ScriptScriptWait));
        Assert.True(VmPAcsClientsideGate972Slice.ScriptStateIsTerminalError(
            VmPAcsClientsideGate972Slice.ScriptDivideBy0));
    }
}
