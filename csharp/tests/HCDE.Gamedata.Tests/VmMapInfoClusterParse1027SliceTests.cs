namespace HCDE.Gamedata.Tests;

public class VmMapInfoClusterParse1027SliceTests
{
    [Fact]
    public void ParseMusicToken_SplitsColonOrder()
    {
        VmMapInfoClusterParse1027Slice.ParseMusicToken("D_RUNNIN:2", false, 0, out var name, out var order);
        Assert.Equal("D_RUNNIN", name);
        Assert.Equal(2, order);
    }

    [Fact]
    public void ParseMusicToken_UsesTrailingNumberWhenNoColon()
    {
        VmMapInfoClusterParse1027Slice.ParseMusicToken("FIGHTER", true, 7, out var name, out var order);
        Assert.Equal("FIGHTER", name);
        Assert.Equal(7, order);
    }

    [Fact]
    public void TryParseCdIdHex_ParsesHexString()
    {
        Assert.True(VmMapInfoClusterParse1027Slice.TryParseCdIdHex("1a2b", out var id));
        Assert.Equal(0x1a2bu, id);
    }

    [Fact]
    public void ApplyCutsceneKeyword_DeleteClearsVideo()
    {
        var cut = new VmMapInfoClusterParse1027Slice.CutsceneDef { Video = "intro.lmp" };
        Assert.True(VmMapInfoClusterParse1027Slice.ApplyCutsceneKeyword(cut, "delete", "", 0, allowFunction: true));
        Assert.Equal("none", cut.Function);
        Assert.Equal("", cut.Video);
    }

    [Fact]
    public void TryApplyDefaultClusterStringLabel_MatchesEnterDefault()
    {
        Assert.True(VmMapInfoClusterParse1027Slice.TryApplyDefaultClusterStringLabel(
            5, "CLUSTERENTER", "CLUSTERENTER5", static (label, text) => label == text,
            out var resolved, out var flag));
        Assert.Equal("CLUSTERENTER5", resolved);
        Assert.Equal(VmMapInfoClusterParse1027Slice.ClusterLookupEnterText, flag);
    }

    [Fact]
    public void ApplyClusterProperty_SetsHubAndAllowIntermissionFlags()
    {
        var cluster = new VmMapInfoClusterParse1027Slice.ClusterDef { Cluster = 1 };
        Assert.True(VmMapInfoClusterParse1027Slice.ApplyClusterProperty(
            cluster, "hub", "", false, 0, false, null, static (_, _) => false));
        Assert.True(VmMapInfoClusterParse1027Slice.ApplyClusterProperty(
            cluster, "allowintermission", "", false, 0, false, null, static (_, _) => false));
        Assert.True((cluster.Flags & VmMapInfoClusterParse1027Slice.ClusterHub) != 0);
        Assert.True((cluster.Flags & VmMapInfoClusterParse1027Slice.ClusterAllowIntermission) != 0);
    }

    [Fact]
    public void ApplyClusterProperty_PicSetsFinalePicFlag()
    {
        var cluster = new VmMapInfoClusterParse1027Slice.ClusterDef { Cluster = 2 };
        Assert.True(VmMapInfoClusterParse1027Slice.ApplyClusterProperty(
            cluster, "pic", "CREDIT", false, 0, true, null, static (_, _) => false));
        Assert.Equal("CREDIT", cluster.FinaleFlat);
        Assert.True((cluster.Flags & VmMapInfoClusterParse1027Slice.ClusterFinalePic) != 0);
    }

    [Fact]
    public void BeginClusterParse_ResetsPriorFields()
    {
        var cluster = new VmMapInfoClusterParse1027Slice.ClusterDef
        {
            Cluster = 9,
            EnterText = "old",
            Flags = VmMapInfoClusterParse1027Slice.ClusterHub,
        };
        VmMapInfoClusterParse1027Slice.BeginClusterParse(3, cluster);
        Assert.Equal(3, cluster.Cluster);
        Assert.Equal("", cluster.EnterText);
        Assert.Equal(0, cluster.Flags);
    }
}
