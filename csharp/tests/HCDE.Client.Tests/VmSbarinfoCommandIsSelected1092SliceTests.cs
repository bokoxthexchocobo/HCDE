namespace HCDE.Client.Tests;

public class VmSbarinfoCommandIsSelected1092SliceTests
{
    [Fact]
    public void ShouldReplaceIsSelectedWeaponClass_WhenNotWeaponKind()
    {
        Assert.True(VmSbarinfoCommandIsSelected1092Slice.ShouldReplaceIsSelectedWeaponClass(
            classFound: true,
            isWeaponKind: false));
    }

    [Fact]
    public void ResolveIsSelectedSecondWeaponParse_RequiresComma()
    {
        var (parseSecond, requireToken) = VmSbarinfoCommandIsSelected1092Slice.ResolveIsSelectedSecondWeaponParse(
            commaAfterFirstWeapon: true);
        Assert.True(parseSecond);
        Assert.True(requireToken);
    }

    [Fact]
    public void ResolveIsSelectedFlowControlTruth_MatchesSecondSlot()
    {
        Assert.True(VmSbarinfoCommandIsSelected1092Slice.ResolveIsSelectedFlowControlTruth(
            readyWeaponPresent: true,
            slot0MatchesReady: false,
            secondSlotConfigured: true,
            slot1MatchesReady: true));
    }

    [Fact]
    public void TryMatchPlayerClassDisplayName_IsCaseInsensitive()
    {
        Assert.True(VmSbarinfoCommandPlayerClass1092Slice.TryMatchPlayerClassDisplayName(
            "Marine",
            "marine"));
    }

    [Fact]
    public void ResolvePlayerClassFlowControlTruthFromScan_AbortsWhenClassMissing()
    {
        Assert.False(VmSbarinfoCommandPlayerClass1092Slice.ResolvePlayerClassFlowControlTruthFromScan(
            playerClassMissing: true,
            listedClassIds: [1, 2],
            spawnClassId: 1));
    }

    [Fact]
    public void ResolvePlayerClassFlowControlTruthFromScan_FindsSpawnClass()
    {
        Assert.True(VmSbarinfoCommandPlayerClass1092Slice.ResolvePlayerClassFlowControlTruthFromScan(
            playerClassMissing: false,
            listedClassIds: [1, 2, 3],
            spawnClassId: 2));
    }

    [Fact]
    public void ResolvePlayerClassParseListAdvance_StopsWithoutComma()
    {
        var (cont, req) = VmSbarinfoCommandPlayerClass1092Slice.ResolvePlayerClassParseListAdvance(
            commaAfterEntry: false);
        Assert.False(cont);
        Assert.False(req);
    }

    [Fact]
    public void ResolvePlayerTypeParseHeadToken_AcceptsStringConst()
    {
        var (mustId, acceptedString) = VmSbarinfoCommandPlayerType1092Slice.ResolvePlayerTypeParseHeadToken(
            stringConstPresent: true);
        Assert.False(mustId);
        Assert.True(acceptedString);
    }

    [Fact]
    public void ResolvePlayerTypeClassListCountAfterLookup_IncrementsWhenFound()
    {
        Assert.Equal(
            2,
            VmSbarinfoCommandPlayerType1092Slice.ResolvePlayerTypeClassListCountAfterLookup(
                priorCount: 1,
                classLookupSucceeded: true));
    }

    [Fact]
    public void CreateIsSelectedWeaponSlotDefaults_NullsBothSlots()
    {
        var (w0, w1) = VmSbarinfoCommandIsSelected1092Slice.CreateIsSelectedWeaponSlotDefaults();
        Assert.Null(w0);
        Assert.Null(w1);
    }
}
