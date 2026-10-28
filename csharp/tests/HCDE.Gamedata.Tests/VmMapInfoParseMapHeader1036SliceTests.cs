namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapHeader1036SliceTests
{
    [Fact]
    public void AssignLevelNumFromMapName_UsesDefaultLevelNum()
    {
        var draft = new VmMapInfoHeaderDraft { MapName = "MAP01" };
        VmMapInfoParseMapHeader1036Slice.AssignLevelNumFromMapName(draft);
        Assert.Equal(1, draft.LevelNum);
        Assert.Equal(1, draft.Id24LevelNum);
    }

    [Fact]
    public void TryAssignHexenMapMusic_LookupByLevelNum()
    {
        var table = new Dictionary<int, string> { [1] = "HAPPY" };
        Assert.True(VmMapInfoParseMapHeader1036Slice.TryAssignHexenMapMusic(table, 1, out var music));
        Assert.Equal("HAPPY", music);
    }

    [Fact]
    public void ApplySharewareTeaserMap_OnlyWhenShareware()
    {
        var map = "E1M1";
        VmMapInfoParseMapHeader1036Slice.ApplySharewareTeaserMap(ref map, isShareware: true, "TEASER");
        Assert.Equal("TEASER", map);
    }

    [Fact]
    public void ShouldIgnoreExtendedEpisode_WhenMenuHackMissing()
    {
        Assert.True(VmMapInfoParseMapHeader1036Slice.ShouldIgnoreExtendedEpisode(
            extended: true,
            gameInfoFlags: 0));
    }

    [Fact]
    public void ShouldSkipOptionalEpisode_WhenMapMissing()
    {
        Assert.True(VmMapInfoParseMapHeader1036Slice.ShouldSkipOptionalEpisode(
            optional: true,
            remove: false,
            mapDataExists: false));
    }

    [Fact]
    public void FindEpisodeMapIndex_IsCaseInsensitive()
    {
        var maps = new List<string> { "MAP01" };
        Assert.Equal(0, VmMapInfoParseMapHeader1036Slice.FindEpisodeMapIndex(maps, "map01"));
    }

    [Fact]
    public void SynthesizeEpisodeNameFromPic_PrefixesDollar()
    {
        Assert.Equal("$PIC", VmMapInfoParseMapHeader1036Slice.SynthesizeEpisodeNameFromPic("PIC"));
    }

    [Fact]
    public void MergeEpisodeDraft_ReplacesExistingMap()
    {
        var maps = new List<string> { "MAP01" };
        var episodes = new List<VmMapInfoEpisodeDraft>
        {
            new() { Map = "MAP01", Name = "OLD" }
        };
        VmMapInfoParseMapHeader1036Slice.MergeEpisodeDraft(
            maps,
            episodes,
            new VmMapInfoEpisodeDraft { Map = "MAP01", Name = "NEW" });
        Assert.Equal("NEW", episodes[0].Name);
        Assert.Single(maps);
    }

    [Fact]
    public void ClassifyEpisodeBodyKeyword_RecognizesRemove()
    {
        Assert.Equal(
            VmMapInfoParseMapHeader1036Slice.EpisodeBodyKeyword.Remove,
            VmMapInfoParseMapHeader1036Slice.ClassifyEpisodeBodyKeyword("remove"));
    }
}
