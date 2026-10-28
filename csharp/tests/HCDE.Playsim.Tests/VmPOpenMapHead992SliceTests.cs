namespace HCDE.Playsim.Tests;

public class VmPOpenMapHead992SliceTests
{
    [Fact]
    public void ScanMapLumpIndex_MatchesAndRequiredGaps()
    {
        var result = VmPOpenMapHead992Slice.ScanMapLumpIndex(
            "THINGS",
            lastIndex: 0,
            needRequired: false,
            out var index);
        Assert.Equal(VmPOpenMapHead992Slice.MapLumpScanResult.Matched, result);
        Assert.Equal(1, index);

        result = VmPOpenMapHead992Slice.ScanMapLumpIndex(
            "UNKNOWN",
            lastIndex: 0,
            needRequired: false,
            out index);
        Assert.Equal(VmPOpenMapHead992Slice.MapLumpScanResult.RequiredMissing, result);
        Assert.Equal(VmPOpenMapHead992Slice.RequiredMissingIndex, index);

        result = VmPOpenMapHead992Slice.ScanMapLumpIndex(
            "PAST_END",
            lastIndex: 12,
            needRequired: false,
            out index);
        Assert.Equal(VmPOpenMapHead992Slice.MapLumpScanResult.EndOfMap, result);
    }

    [Fact]
    public void ExternalAndClassicMapPaths()
    {
        Assert.True(VmPOpenMapHead992Slice.IsExternalFileMapName("file:mywad.wad"));
        Assert.Equal("maps/MAP01.wad", VmPOpenMapHead992Slice.FormatMapsWadPath("MAP01"));
        Assert.True(VmPOpenMapHead992Slice.UsesClassicShortMapName("MAP01"));
        Assert.False(VmPOpenMapHead992Slice.UsesClassicShortMapName("maps/longname"));
    }

    [Fact]
    public void ShortNameLumpSelectionAndBuildProbe()
    {
        Assert.True(VmPOpenMapHead992Slice.ShouldPreferShortNameLump(
            lumpName: 5,
            lumpWad: 2,
            lumpMap: 1));
        Assert.True(VmPOpenMapHead992Slice.IsLumpFromDifferentContainer(lumpFile: 1, nextFile: 2));
        Assert.True(VmPOpenMapHead992Slice.ShouldAbortStandaloneNonBuildMap(
            VmPOpenMapHead992Slice.IsBuildMapProbeAlwaysFalse()));
        Assert.True(VmPOpenMapHead992Slice.MapLumpNameMatches("LINEDEFS2", "LINEDEFS"));
    }
}
