namespace HCDE.Client.Tests;

public class VmAutomapBoundaryHelpersTests
{
    [Fact]
    public void Check3DFloors_ReturnsFalse_WhenBothEmpty()
    {
        Assert.False(VmAutomapBoundaryHelpers.Check3DFloors(Array.Empty<VmAutomapBoundaryHelpers.FFloorRecord>(),
            Array.Empty<VmAutomapBoundaryHelpers.FFloorRecord>()));
    }

    [Fact]
    public void Check3DFloors_ReturnsTrue_WhenCountsMismatch()
    {
        var front = new[]
        {
            new VmAutomapBoundaryHelpers.FFloorRecord("m", VmAutomapBoundaryHelpers.FFloorFlags.Exists, 255),
        };
        Assert.True(VmAutomapBoundaryHelpers.Check3DFloors(front, Array.Empty<VmAutomapBoundaryHelpers.FFloorRecord>()));
    }

    [Fact]
    public void Check3DFloors_ReturnsTrue_WhenFrontFloorUnmatched()
    {
        var front = new[]
        {
            new VmAutomapBoundaryHelpers.FFloorRecord("a", VmAutomapBoundaryHelpers.FFloorFlags.Exists, 200),
        };
        var back = new[]
        {
            new VmAutomapBoundaryHelpers.FFloorRecord("b", VmAutomapBoundaryHelpers.FFloorFlags.Exists, 200),
        };
        Assert.True(VmAutomapBoundaryHelpers.Check3DFloors(front, back));
    }

    [Fact]
    public void CheckSpecialBoundary_LineActivation_Wins()
    {
        var ok = VmAutomapBoundaryHelpers.CheckSpecialBoundary(
            VmAutomapBoundaryHelpers.SpacPlayerActivate,
            VmAutomapBoundaryHelpers.SpecialExitNormal,
            Array.Empty<int>(),
            false,
            Array.Empty<VmAutomapBoundaryHelpers.SectorAction>(),
            null,
            (special, _) => VmAutomapBoundaryHelpers.IsExitSpecial(special),
            out var special,
            out _);
        Assert.True(ok);
        Assert.Equal(VmAutomapBoundaryHelpers.SpecialExitNormal, special);
    }

    [Fact]
    public void IsTeleportSpecial_MatchesNativeSet()
    {
        Assert.True(VmAutomapBoundaryHelpers.IsTeleportSpecial(VmAutomapBoundaryHelpers.SpecialTeleport));
        Assert.False(VmAutomapBoundaryHelpers.IsTeleportSpecial(VmAutomapBoundaryHelpers.SpecialDoorOpen));
    }

    [Fact]
    public void TryResolveLockBoundary_UsesLineLockNumber()
    {
        Assert.True(VmAutomapBoundaryHelpers.TryResolveLockBoundary(
            7, 0, 0, Array.Empty<int>(), false,
            Array.Empty<VmAutomapBoundaryHelpers.SectorAction>(), null, out var lockNum));
        Assert.Equal(7, lockNum);
    }
}

public class VmAutomapDrawSegTests
{
    [Fact]
    public void ShouldApplyMapRotation_MatchesNativeModes()
    {
        Assert.True(VmAutomapDrawSeg.ShouldApplyMapRotation(1, false));
        Assert.True(VmAutomapDrawSeg.ShouldApplyMapRotation(2, true));
        Assert.False(VmAutomapDrawSeg.ShouldApplyMapRotation(2, false));
    }

    [Fact]
    public void RotateEndpointsIfNeeded_CallsDelegateWhenEnabled()
    {
        var line = VmAutomapDrawSeg.BuildSegLine(0, 0, 10, 0);
        var rotated = VmAutomapDrawSeg.RotateEndpointsIfNeeded(line, true, (x, y) => (-y, x));
        Assert.Equal(0, rotated.A.X);
        Assert.Equal(0, rotated.A.Y);
        Assert.Equal(0, rotated.B.X);
        Assert.Equal(10, rotated.B.Y);
    }
}
