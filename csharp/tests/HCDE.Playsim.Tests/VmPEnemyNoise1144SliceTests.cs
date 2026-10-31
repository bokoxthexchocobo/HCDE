namespace HCDE.Playsim.Tests;

public class VmPEnemyNoise1144SliceTests
{
    [Fact]
    public void ChaseTables_UseTheNativeDiagonalAndOppositeDirections()
    {
        Assert.Equal(VmPEnemyNoise1144Slice.SqrtHalf, VmPEnemyNoise1144Slice.XSpeed[VmPEnemyNoise1144Slice.NorthEast]);
        Assert.Equal((1d, 0d), VmPEnemyNoise1144Slice.MoveSpeed(VmPEnemyNoise1144Slice.East));
        Assert.Equal((0d, 1d), VmPEnemyNoise1144Slice.MoveSpeed(VmPEnemyNoise1144Slice.North));
        Assert.Equal((-1d, 0d), VmPEnemyNoise1144Slice.MoveSpeed(VmPEnemyNoise1144Slice.West));
        Assert.Equal((0d, -1d), VmPEnemyNoise1144Slice.MoveSpeed(VmPEnemyNoise1144Slice.South));
        Assert.Equal(VmPEnemyNoise1144Slice.West, VmPEnemyNoise1144Slice.Opposite[VmPEnemyNoise1144Slice.East]);
        Assert.Equal(VmPEnemyNoise1144Slice.NoDir, VmPEnemyNoise1144Slice.Opposite[VmPEnemyNoise1144Slice.NoDir]);
        Assert.Equal(VmPEnemyNoise1144Slice.NorthWest, VmPEnemyNoise1144Slice.Diagonals[0]);
        Assert.Equal(VmPEnemyNoise1144Slice.SouthEast, VmPEnemyNoise1144Slice.Diagonals[3]);
    }

    [Fact]
    public void RngStreams_LeaveChaseAndFaceTargetProcessWide()
    {
        Assert.False(VmPEnemyNoise1144Slice.NoMonsterInterpolationDefault());
        Assert.Equal(0, VmPEnemyNoise1144Slice.DropStyleDefault());
        Assert.Equal(19, VmPEnemyNoise1144Slice.RngStreams.Length);
        Assert.False(Find("Chase").FileLocal);
        Assert.False(Find("FaceTarget").FileLocal);
        Assert.False(Find("RailFace").FileLocal);
        Assert.True(Find("EnemyStrafe").FileLocal);
        Assert.Equal("CheckMissileRange", VmPEnemyNoise1144Slice.RngStreams[0].Name);
    }

    [Fact]
    public void MarkSector_SkipsASectorThatWasAlreadyFlooded()
    {
        var flooded = VmPEnemyNoise1144Slice.MarkSector(
            5, 5, 2, 1, false, 0,
            new[] { new VmPEnemyNoise1144Slice.SectorActor(false, 0, 1) });
        Assert.True(flooded.Flooded);
        Assert.Empty(flooded.Heard);
        Assert.Equal(2, flooded.SoundTraversed);

        var open = VmPEnemyNoise1144Slice.MarkSector(5, 5, 3, 1, false, 0, System.Array.Empty<VmPEnemyNoise1144Slice.SectorActor>());
        Assert.False(open.Flooded);
        Assert.Equal(5, open.ValidCount);
        Assert.Equal(2, open.SoundTraversed);
    }

    [Fact]
    public void MarkSector_HearsActorsInsideTheSplashAndDistanceRules()
    {
        var actors = new[]
        {
            new VmPEnemyNoise1144Slice.SectorActor(true, 0, 1),
            new VmPEnemyNoise1144Slice.SectorActor(false, VmPEnemyNoise1144Slice.NoSplashAlert, 1),
            new VmPEnemyNoise1144Slice.SectorActor(false, 0, 10),
            new VmPEnemyNoise1144Slice.SectorActor(false, 0, 10.5)
        };
        var splash = VmPEnemyNoise1144Slice.MarkSector(1, 2, 0, 0, true, 10, actors);
        Assert.Equal(new[] { 2 }, splash.Heard);
        Assert.Equal(1, splash.SoundTraversed);

        var quiet = VmPEnemyNoise1144Slice.MarkSector(1, 2, 9, 0, false, 0, actors);
        Assert.Equal(new[] { 1, 2, 3 }, quiet.Heard);
        Assert.Equal((false, true), VmPEnemyNoise1144Slice.PortalSoundChecks(true, false));
        Assert.Equal((true, true), VmPEnemyNoise1144Slice.PortalSoundChecks(false, false));
    }

    private static VmPEnemyNoise1144Slice.RngStream Find(string name)
    {
        for (var i = 0; i < VmPEnemyNoise1144Slice.RngStreams.Length; i++)
        {
            if (VmPEnemyNoise1144Slice.RngStreams[i].Name == name)
                return VmPEnemyNoise1144Slice.RngStreams[i];
        }
        throw new System.InvalidOperationException(name);
    }
}
