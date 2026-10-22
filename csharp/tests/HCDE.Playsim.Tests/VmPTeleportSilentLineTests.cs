namespace HCDE.Playsim.Tests;

public class VmPTeleportSilentLineTests
{
    [Fact]
    public void EvSilentLineTeleport_RejectsWhenNoBackSidedef()
    {
        var s = new VmPTeleportSilentLine.SilentLineTeleportState { BackSidedefNull = true };
        Assert.False(VmPTeleportSilentLine.EvSilentLineTeleport(s));
    }

    [Fact]
    public void EvSilentLineTeleport_TeleportsWhenCandidateMatches()
    {
        var moved = false;
        var s = new VmPTeleportSilentLine.SilentLineTeleportState
        {
            LineIdIterator = _ => new[] { 2 },
            CurrentLineIndex = 1,
            GetCandidate = _ => new VmPTeleportSilentLine.SilentLineCandidate
            {
                DeltaAngleDegrees = 0,
                V1X = 0,
                V1Y = 0,
                DeltaX = 100,
                DeltaY = 0,
                SrcDeltaX = 100,
                SrcDeltaY = 0,
                FrontFloorZ = 0,
                BackFloorZ = 0,
            },
            PointOnLineSide = (_, _, _) => true,
            TeleportMove = (_, _, _) => { moved = true; return true; },
        };
        Assert.True(VmPTeleportSilentLine.EvSilentLineTeleport(s));
        Assert.True(moved);
    }
}
