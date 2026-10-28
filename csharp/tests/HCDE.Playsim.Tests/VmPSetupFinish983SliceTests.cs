namespace HCDE.Playsim.Tests;

public class VmPSetupFinish983SliceTests
{
    [Fact]
    public void SetupFinish_HeightsecAndBackup()
    {
        Assert.True(VmPSetupFinish983Slice.HeightsecIgnoreScanApplies(new object(), ffloorCount: 2));
        Assert.False(VmPSetupFinish983Slice.HeightsecIgnoreScanApplies(null, ffloorCount: 2));
        Assert.True(VmPSetupFinish983Slice.Rendered3dFloorIgnoresHeightsec(
            VmPSetupFinish983Slice.FfExists | VmPSetupFinish983Slice.FfRenderplanes));
        var marked = VmPSetupFinish983Slice.ScanSectorHeightsecIgnoreFlags(
            new object(),
            [
                VmPSetupFinish983Slice.FfExists | VmPSetupFinish983Slice.FfRenderplanes,
                0,
            ],
            moreFlags: 0);
        Assert.Equal(VmPSetupFinish983Slice.SecmfIgnoreheightsec, marked);
        Assert.True(VmPSetupFinish983Slice.ShouldSnapshotLoadMapGeometry(10));
        Assert.True(VmPSetupFinish983Slice.ShouldEnableCompatLineOnSide(automapInitialized: true));
    }

    [Fact]
    public void LightningAndDynamicLights()
    {
        Assert.True(VmPSetupFinish983Slice.ShouldStartLightningForLevel(
            VmPSetupFinish983Slice.LevelStartlightning));
        var flags3 = VmPSetupFinish983Slice.MarkDynamicLightsCreated(0);
        Assert.True(VmPSetupFinish983Slice.ShouldInitializeDynamicLightsForActor(
            new object(),
            lightsCreatedFlagSet: true));
        Assert.NotEqual(0, flags3);
    }

    [Fact]
    public void InitShutdownAndDebugCommands()
    {
        Assert.True(VmPSetupFinish983Slice.ShutdownDestroysClientSideStatics(
            levelInShutdownLoop: true,
            clientSideListUsed: true));
        Assert.True(VmPSetupFinish983Slice.ShutdownClearsPlayerPsprites(new object(), playerAllocated: true));
        Assert.True(VmPSetupFinish983Slice.DumpGeometryLogsSector(1, commandActive: true));
        Assert.True(VmPSetupFinish983Slice.DumpGeometrySubsectorInRange(0, subsectorCount: 3));
        Assert.True(VmPSetupFinish983Slice.DumpGeometrySubsectorHacked(VmPSetupFinish983Slice.SubsectorHackedFlag));
        Assert.True(VmPSetupFinish983Slice.DumpGeometrySegLoopContinues(0, segCount: 2));
        Assert.True(VmPSetupFinish983Slice.DumpGeometrySegHasLinedef(new object(), segValid: true));
        Assert.True(VmPSetupFinish983Slice.DumpGeometrySegIsMiniseg(null, segValid: true));
        Assert.True(VmPSetupFinish983Slice.DumpGeometryLogsBackSectorViaPartner(
            partnerSeg: null,
            backsector: new object()));
        Assert.True(VmPSetupFinish983Slice.ListMapsectionsScansSubsectors(
            sectionIndex: 2,
            subsectorCount: 3,
            mapsectionId: 2));
    }
}
