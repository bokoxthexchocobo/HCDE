namespace HCDE.Playsim.Tests;

public class VmGLevelSnapshotPersist1015SliceTests
{
    [Fact]
    public void AirControlFriction()
    {
        Assert.Equal(1.0, VmGLevelSnapshotPersist1015Slice.ComputeAirFriction(0));
        Assert.Equal(1.0, VmGLevelSnapshotPersist1015Slice.ComputeAirFriction(1.0 / 512));
        Assert.True(VmGLevelSnapshotPersist1015Slice.UsesLowAirFriction(1.0 / 512));
        var friction = VmGLevelSnapshotPersist1015Slice.ComputeAirFriction(0.5);
        Assert.Equal(0.5 * -0.0941 + 1.0004, friction, precision: 6);
    }

    [Fact]
    public void SnapshotWritePaths()
    {
        Assert.False(VmGLevelSnapshotPersist1015Slice.ShouldEmitSnapshot(0));
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldEmitSnapshot(1));
        Assert.Equal(
            "map01.map.json",
            VmGLevelSnapshotPersist1015Slice.PlanLevelSnapshotWrite("MAP01").Filename);
        Assert.Equal(
            "default.mapd.json",
            VmGLevelSnapshotPersist1015Slice.PlanDefaultSnapshotWrite("Default").Filename);
        Assert.True(VmGLevelSnapshotPersist1015Slice.TryParseSnapshotMapName(
            "MAP02.map.json",
            out var mapName,
            out var kind));
        Assert.Equal("MAP02", mapName);
        Assert.Equal(VmGLevelSnapshotPersist1015Slice.SnapshotResourceKind.LevelMap, kind);
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldAssignSnapshotToDefaultLevelInfo(
            VmGLevelSnapshotPersist1015Slice.SnapshotResourceKind.DefaultMap));
    }

    [Fact]
    public void VisitedAndPlayerClassPersistence()
    {
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldPersistRandomPlayerClasses(
            multiplayer: false,
            deathmatch: true));
        Assert.False(VmGLevelSnapshotPersist1015Slice.ShouldPersistRandomPlayerClasses(
            multiplayer: true,
            deathmatch: true));
        var visited = VmGLevelSnapshotPersist1015Slice.ApplyVisitedLevelFlag(0);
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldWriteVisitedMapName(visited));
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldWritePlayerClassEntry(
            playerInGame: true,
            playerIndex: 0));
        Assert.Equal("3", VmGLevelSnapshotPersist1015Slice.FormatPlayerClassKey(3));
        Assert.Equal(4, VmGLevelSnapshotPersist1015Slice.CountVisitedEntriesToRead(4, maxEntries: 64));
        Assert.True(VmGLevelSnapshotPersist1015Slice.ShouldApplyVisitedFlagToLevelInfo(
            levelInfoFound: true,
            mapName: "MAP01"));
    }
}
