namespace HCDE.Playsim.Tests;

public class VmGLevelMarkTickMusic1017SliceTests
{
    [Fact]
    public void PropagateMarkTailAndGrayReinsert()
    {
        Assert.Equal(720, VmGLevelMarkTickMusic1017Slice.AccumulateSidePropagateMarkedBytes(
            sidesProcessed: 3,
            sideStructureSize: 240));
        Assert.Equal(9, VmGLevelMarkTickMusic1017Slice.SideTextureMarksInBatch(sidesProcessed: 3));
        var plan = VmGLevelMarkTickMusic1017Slice.PlanSectorMarkerGrayReinsert(
            moreTodo: true,
            markedBytesThisPass: 100);
        Assert.True(plan.ReinsertOnGrayList);
        Assert.True(plan.ChainAsGrayHead);
    }

    [Fact]
    public void LevelTickMarkAndScroller()
    {
        Assert.True(VmGLevelMarkTickMusic1017Slice.ShouldClearScrollCarryBuffer(scrollEntryCount: 2));
        Assert.True(VmGLevelMarkTickMusic1017Slice.ShouldCreateSectorMarker(
            markerIsNull: true,
            sectorCount: 1,
            polyobjectCount: 0,
            sideCount: 0));
        Assert.True(VmGLevelMarkTickMusic1017Slice.ShouldNullSectorMarkerWhenEmpty(0, 0, 0));
        Assert.True(VmGLevelMarkTickMusic1017Slice.ShouldResetSectorMarkerProgress(
            markerExists: true,
            sectorCount: 5,
            polyobjectCount: 0,
            sideCount: 0));
        Assert.Equal(2, VmGLevelMarkTickMusic1017Slice.CountLocalEventHandlerMarkTargets(hasLocalEventManager: true));
        Assert.True(VmGLevelMarkTickMusic1017Slice.IsInvalidScrollerSector(-1));
        Assert.True(VmGLevelMarkTickMusic1017Slice.ShouldResizeScrollCarryArray(currentScrollCount: 0));
        Assert.Equal(128, VmGLevelMarkTickMusic1017Slice.ResolveScrollCarryCapacity(sectorCount: 128));
    }

    [Fact]
    public void InterMusicAndVolume()
    {
        var map = VmGLevelMarkTickMusic1017Slice.ResolveInterMusic(
            hasMapInterMusicEntry: true,
            mapInterMusic: "map.mid",
            mapInterOrder: 2,
            levelInterMusicNonEmpty: true,
            levelInterMusic: "level.mid",
            levelInterOrder: 1,
            gameInterMusic: "game.mid",
            gameInterOrder: 0);
        Assert.Equal(VmGLevelMarkTickMusic1017Slice.InterMusicSelection.MapInterMusic, map.Source);
        Assert.Equal("map.mid", map.Music);

        var game = VmGLevelMarkTickMusic1017Slice.ResolveInterMusic(
            hasMapInterMusicEntry: false,
            mapInterMusic: null,
            mapInterOrder: 0,
            levelInterMusicNonEmpty: false,
            levelInterMusic: "",
            levelInterOrder: 0,
            gameInterMusic: "game.mid",
            gameInterOrder: 4);
        Assert.Equal(VmGLevelMarkTickMusic1017Slice.InterMusicSelection.GameInterMusic, game.Source);
        Assert.True(VmGLevelMarkTickMusic1017Slice.HasSetInterMusicActionTarget(1));
        var volume = VmGLevelMarkTickMusic1017Slice.AssignMusicVolume(0.75f);
        Assert.Equal(0.75f, volume.LevelVolume);
        Assert.True(volume.InvokeHostVolume);
    }
}
