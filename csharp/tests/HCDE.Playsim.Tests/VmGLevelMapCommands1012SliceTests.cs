namespace HCDE.Playsim.Tests;

public class VmGLevelMapCommands1012SliceTests
{
    [Fact]
    public void MapCommand()
    {
        Assert.Equal(
            VmGLevelMapCommands1012Slice.SpOnlyCommandRejectReason.NetGame,
            VmGLevelMapCommands1012Slice.ResolveSpOnlyCommandRejectReason(netGame: true));
        Assert.True(VmGLevelMapCommands1012Slice.HasMapCommandTarget(argc: 2));
        Assert.Equal("MAP01", VmGLevelMapCommands1012Slice.ResolveMapNameArgument("*", "MAP01"));
        var coop = VmGLevelMapCommands1012Slice.ResolveMultiplayerNextFlags(
            VmGLevelMapCommands1012Slice.ResolveMultiplayerNextMode("coop"));
        Assert.False(coop.Deathmatch);
        Assert.True(coop.MultiplayerNext);
        Assert.True(VmGLevelMapCommands1012Slice.ShouldDeferNewGameForValidMap(
            mapDataValid: true,
            rejectedAsNetGame: false));
    }

    [Fact]
    public void RecordMapCommand()
    {
        Assert.True(VmGLevelMapCommands1012Slice.HasRecordMapCommandTarget(argc: 3));
        Assert.Equal(4, VmGLevelMapCommands1012Slice.ResolveRecordMapTraceAction());
        var dm = VmGLevelMapCommands1012Slice.ResolveMultiplayerNextFlags(
            VmGLevelMapCommands1012Slice.ResolveMultiplayerNextMode("dm"));
        Assert.True(dm.Deathmatch && dm.MultiplayerNext);
    }

    [Fact]
    public void OpenCommand()
    {
        Assert.True(VmGLevelMapCommands1012Slice.HasOpenCommandTarget(argc: 2));
        Assert.Equal("file:my.wad", VmGLevelMapCommands1012Slice.BuildOpenMapPath("my.wad"));
        var setup = VmGLevelMapCommands1012Slice.ResolveOpenMapDeferredSetup("my.wad", mapDataValid: true);
        Assert.Equal(-1, setup.Skill);
        Assert.Equal(VmGLevelMapCommands1012Slice.GaNewGame2, setup.TraceGameAction);
    }
}
