namespace HCDE.Playsim.Tests;

public class VmPMapPitCheckPortalTests
{
    [Fact]
    public void PitCheckPortal_IgnoresVerticalPortalFlags()
    {
        var tm = new VmPMapPitCheckPortal.CheckPositionTm();
        Assert.False(VmPMapPitCheckPortal.PitCheckPortal(
            new VmPMapPitCheckPortal.PortalCheckContext { PortalFlags = VmPMapPitCheckPortal.FfcfNofloor },
            tm,
            new VmPMapPitCheckPortal.ThingState()));
    }

    [Fact]
    public void PitCheckPortal_AdjustsFloorAndCeiling()
    {
        var tm = new VmPMapPitCheckPortal.CheckPositionTm { CeilingZ = 128, FloorZ = 0, DropoffZ = 0 };
        var zdelta = new List<double>();
        var ret = VmPMapPitCheckPortal.PitCheckPortal(
            new VmPMapPitCheckPortal.PortalCheckContext
            {
                PortalFlags = 0,
                LineIndex = 1,
                PosX = 8,
                PosY = 8,
                InRange = _ => true,
                BoxOnLineSide = (_, _, _, _) => -1,
                PortalDestinationLine = _ => 9,
                TranslatePortalXy = (_, c) => { c.X += 4; c.Y += 4; },
                TranslatePortalZ = (_, c) => c.Z += 8,
                LineHasBacksector = _ => true,
                LinesInBox = _ => new[] { 10 },
                FindRefPoint = (_, x, y) => (x, y),
                LineOpening = (_, _, _, _) => new VmPMapPitCheckPortal.LineOpening(64, 32, 16, 7, 8, 9),
            },
            tm,
            new VmPMapPitCheckPortal.ThingState { AddZ = d => zdelta.Add(d) });
        Assert.True(ret);
        Assert.Equal(56, tm.CeilingZ);
        Assert.Equal(24, tm.FloorZ);
        Assert.Equal(2, zdelta.Count);
        Assert.Equal(0, zdelta[1]);
    }
}
