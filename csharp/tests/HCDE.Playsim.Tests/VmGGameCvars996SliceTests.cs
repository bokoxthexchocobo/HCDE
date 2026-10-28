namespace HCDE.Playsim.Tests;

public class VmGGameCvars996SliceTests
{
    [Fact]
    public void DisplayNameTagsClamp()
    {
        Assert.Equal(0, VmGGameCvars996Slice.NormalizeDisplayNameTags(-1));
        Assert.Equal(2, VmGGameCvars996Slice.NormalizeDisplayNameTags(2));
        Assert.True(VmGGameCvars996Slice.ShouldClampDisplayNameTags(4));
    }

    [Fact]
    public void SvGameTypeRules()
    {
        Assert.True(VmGGameCvars996Slice.ShouldRejectSvGameType(5));
        Assert.Equal(0, VmGGameCvars996Slice.ClampSvGameType(9));

        var dm = VmGGameCvars996Slice.ResolveMultiplayerRulesFromSvGameType(1);
        Assert.True(dm.Deathmatch);
        Assert.False(dm.Teamplay);

        var team = VmGGameCvars996Slice.ResolveMultiplayerRulesFromSvGameType(2);
        Assert.True(team.Deathmatch && team.Teamplay);

        var ctf = VmGGameCvars996Slice.ResolveMultiplayerRulesFromSvGameType(3);
        Assert.True(ctf.UsesCtfPlaceholderRules);

        var invasion = VmGGameCvars996Slice.ResolveMultiplayerRulesFromSvGameType(4);
        Assert.True(VmGGameCvars996Slice.IsInvasionGameType(4));
        Assert.False(invasion.Deathmatch);
    }

    [Fact]
    public void SessionFlags()
    {
        var flags = VmGGameCvars996Slice.CollectGameSessionFlags(
            sendPause: true,
            sendSave: false,
            sendTurn180: true,
            userGame: true,
            inSave: true);
        Assert.True(flags.BlocksExitCommands);
        Assert.True(flags.HasPendingNetworkEvents);
        Assert.True(VmGGameCvars996Slice.UsesTeamDeathmatchRules(
            VmGGameCvars996Slice.SvGameTypeCtfPlaceholder));
    }
}
