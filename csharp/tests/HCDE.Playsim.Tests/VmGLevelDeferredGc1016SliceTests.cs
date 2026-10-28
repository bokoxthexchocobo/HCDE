namespace HCDE.Playsim.Tests;

public class VmGLevelDeferredGc1016SliceTests
{
    [Fact]
    public void ReadVisitedPlayerClasses()
    {
        Assert.True(VmGLevelDeferredGc1016Slice.ShouldIteratePlayerClassSlot(0));
        Assert.False(VmGLevelDeferredGc1016Slice.ShouldIteratePlayerClassSlot(8));
        Assert.Equal(
            VmGLevelDeferredGc1016Slice.PlayerClassRestoreAction.UpdateDefault,
            VmGLevelDeferredGc1016Slice.ResolvePlayerClassRestoreAction(hasSavedKey: false));
        Assert.Equal(
            VmGLevelDeferredGc1016Slice.PlayerClassRestoreAction.DeserializeSaved,
            VmGLevelDeferredGc1016Slice.ResolvePlayerClassRestoreAction(hasSavedKey: true));
    }

    [Fact]
    public void AcsDeferredSaveLoadGates()
    {
        Assert.True(VmGLevelDeferredGc1016Slice.AnyWadLevelHasDeferred([0, 3, 0]));
        Assert.False(VmGLevelDeferredGc1016Slice.AnyWadLevelHasDeferred([0, 0]));
        Assert.True(VmGLevelDeferredGc1016Slice.ShouldWriteMapDeferredEntry(deferredSize: 2));
        Assert.True(VmGLevelDeferredGc1016Slice.IsUnknownDeferredMapKey(
            levelInfoFound: false,
            mapKey: "MAP01"));
    }

    [Fact]
    public void SectorMarkerPropagateSteps()
    {
        var sectorBatch = VmGLevelDeferredGc1016Slice.StepSectorPropagateMark(startIndex: 30, totalSectors: 70);
        Assert.Equal(32, sectorBatch.ItemsProcessed);
        Assert.Equal(62, sectorBatch.NextIndex);
        Assert.True(sectorBatch.MoreRemaining);
        Assert.True(VmGLevelDeferredGc1016Slice.ShouldBeginPolyobjectPropagatePass(
            sectorsFinished: true,
            polyobjectCount: 5));
        var polyBatch = VmGLevelDeferredGc1016Slice.StepPolyobjectPropagateMark(startIndex: 0, totalPolyobjects: 200);
        Assert.Equal(VmGLevelDeferredGc1016Slice.PolyStepSize, polyBatch.ItemsProcessed);
        Assert.True(VmGLevelDeferredGc1016Slice.ShouldBeginSideDefPropagatePass(
            sectorsFinished: true,
            polyobjectsFinished: true,
            sideCount: 10));
        Assert.Equal(9, VmGLevelDeferredGc1016Slice.CountSectorGcMarkTargets());
        Assert.Equal(3, VmGLevelDeferredGc1016Slice.CountSideTextureGcMarkTargets());
    }
}
