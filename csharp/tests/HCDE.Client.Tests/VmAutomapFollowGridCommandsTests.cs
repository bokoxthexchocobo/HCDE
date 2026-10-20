namespace HCDE.Client.Tests;

public class VmAutomapFollowGridCommandsTests
{
    [Fact]
    public void ToggleFollow_FlipsState()
    {
        Assert.False(VmAutomapFollowGridCommands.ToggleFollow(true));
        Assert.True(VmAutomapFollowGridCommands.ToggleFollow(false));
    }

    [Fact]
    public void ResolveFollowMessageKey_UsesOnString()
    {
        Assert.Equal("AMSTR_FOLLOWON", VmAutomapFollowGridCommands.ResolveFollowMessageKey(true));
    }

    [Fact]
    public void ResolveGridMessageKey_UsesOffString()
    {
        Assert.Equal("AMSTR_GRIDOFF", VmAutomapFollowGridCommands.ResolveGridMessageKey(false));
    }
}
