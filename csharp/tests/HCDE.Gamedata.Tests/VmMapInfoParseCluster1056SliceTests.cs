namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseCluster1056SliceTests
{
    [Fact]
    public void TryRemapHexenClusterExitText_SetsLookupFlag()
    {
        var flags = VmMapInfoClusterParse1027Slice.ClusterExitTextInLump;
        var exit = "CLUSMSG";
        Assert.True(VmMapInfoParseCluster1056Slice.TryRemapHexenClusterExitText(
            exitTextInLump: true,
            lumpIndex: 1,
            "HEXEN.WAD",
            exit,
            _ => true,
            ref flags,
            ref exit));
        Assert.Equal("TXT_HEXEN_CLUSMSG", exit);
        Assert.True((flags & VmMapInfoClusterParse1027Slice.ClusterLookupExitText) != 0);
    }

    [Fact]
    public void ResolveNumericNextMapToken_HexenUsesWarpTrans()
    {
        Assert.Equal("&wt@07", VmMapInfoParseCluster1056Slice.ResolveNumericNextMapToken(7, hexenHack: true));
        Assert.Equal("MAP07", VmMapInfoParseCluster1056Slice.ResolveNumericNextMapToken(7, hexenHack: false));
    }

    [Fact]
    public void NormalizeHcdeShadowQualityCap_SnapsToStep()
    {
        Assert.Equal(512, VmMapInfoParseCluster1056Slice.NormalizeHcdeShadowQualityCap(600));
        Assert.Equal(128, VmMapInfoParseCluster1056Slice.NormalizeHcdeShadowQualityCap(10));
    }

    [Fact]
    public void ClassifyMapOptionKeyword_RecognizesSecretAlias()
    {
        Assert.Equal(
            VmMapInfoParseCluster1056Slice.MapOptionKeyword.Secret,
            VmMapInfoParseCluster1056Slice.ClassifyMapOptionKeyword("secret"));
    }

    [Fact]
    public void MapOptionUsesParseNextMap_ForNextAndSecretVariants()
    {
        Assert.True(VmMapInfoParseCluster1056Slice.MapOptionUsesParseNextMap(
            VmMapInfoParseCluster1056Slice.MapOptionKeyword.SecretNext));
    }

    [Fact]
    public void ShouldErrorClusterDefinitionEof_NewFormatOnly()
    {
        Assert.True(VmMapInfoParseCluster1056Slice.ShouldErrorClusterDefinitionEof(
            VmMapInfoParseParser1054Slice.FmtNew, scannerAtEnd: true));
    }

    [Fact]
    public void ClusterLoopAfterCloseBraceAttempt_BreaksOnCloseBrace()
    {
        var (handle, breakLoop) = VmMapInfoParseCluster1056Slice.ClusterLoopAfterCloseBraceAttempt(parsedCloseBrace: true);
        Assert.False(handle);
        Assert.True(breakLoop);
    }

    [Fact]
    public void ApplyClusterCutsceneSlot_DelegatesIntroKeyword()
    {
        var cluster = VmMapInfoClusterParse1027Slice.CreateFreshCluster(1);
        var cut = new VmMapInfoClusterParse1027Slice.CutsceneDef { Video = "INTRO" };
        Assert.True(VmMapInfoParseCluster1056Slice.ApplyClusterCutsceneSlot(cluster, "intro", cut));
        Assert.Equal("INTRO", cluster.Intro.Video);
    }
}
