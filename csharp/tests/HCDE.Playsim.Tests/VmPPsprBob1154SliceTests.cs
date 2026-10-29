namespace HCDE.Playsim.Tests;

public class VmPPsprBob1154SliceTests
{
    [Fact]
    public void Smooth_PassesRawFractionsUntilTheCurveIsEnabled()
    {
        Assert.Equal(1.5, VmPPsprBob1154Slice.SmoothFrac(false, 1.5));
        Assert.Equal(-0.25, VmPPsprBob1154Slice.SmoothFrac(false, -0.25));
        Assert.Equal(0, VmPPsprBob1154Slice.SmoothFrac(true, -0.25));
        Assert.Equal(0, VmPPsprBob1154Slice.SmoothFrac(true, 0));
        Assert.Equal(1, VmPPsprBob1154Slice.SmoothFrac(true, 2));
        Assert.Equal(0.5, VmPPsprBob1154Slice.SmoothFrac(true, 0.5));
        Assert.Equal(0.15625, VmPPsprBob1154Slice.SmoothFrac(true, 0.25));
        Assert.False(VmPPsprBob1154Slice.BringUp(false));
        Assert.True(VmPPsprBob1154Slice.BringUp(true));
        Assert.Equal(0, VmPPsprBob1154Slice.PspNone);
        Assert.Equal(1, VmPPsprBob1154Slice.PspFlat);
        Assert.Equal(2, VmPPsprBob1154Slice.PspSpatial);
        Assert.Equal(1, VmPPsprBob1154Slice.KeepX);
        Assert.Equal(2, VmPPsprBob1154Slice.KeepY);
        Assert.Equal(4, VmPPsprBob1154Slice.Add);
        Assert.Equal(8, VmPPsprBob1154Slice.Interpolate);
        Assert.Equal(16, VmPPsprBob1154Slice.Relative);
        Assert.Equal(32, VmPPsprBob1154Slice.ZeroY);
    }

    [Fact]
    public void Bob_StopsAtADeadItemAndResetsWithoutAVirtual()
    {
        var reset = VmPPsprBob1154Slice.BobWeapon(false, 9, 1, 2, Array.Empty<VmPPsprBob1154Slice.BobItem>());
        Assert.True(reset.Reset);
        Assert.False(reset.Set);
        Assert.Equal(0, reset.CallFrac);
        Assert.Equal(1, reset.X);

        var empty = VmPPsprBob1154Slice.BobWeapon(true, 9, 1, 2, Array.Empty<VmPPsprBob1154Slice.BobItem>());
        Assert.True(empty.Set);
        Assert.Equal(0, empty.Visited);
        Assert.Equal(2, empty.Y);

        var stopped = VmPPsprBob1154Slice.BobWeapon(true, 9, 1, 2, new[]
        {
            new VmPPsprBob1154Slice.BobItem(0, false, 0, 0),
            new VmPPsprBob1154Slice.BobItem(VmPPsprBob1154Slice.Euthanize, true, 8, 8),
            new VmPPsprBob1154Slice.BobItem(0, true, 7, 6),
        });
        Assert.False(stopped.Reset);
        Assert.True(stopped.Set);
        Assert.True(stopped.Interpolate2D);
        Assert.Equal(1, stopped.CallFrac);
        Assert.Equal(9, stopped.Timer);
        Assert.Equal(1, stopped.Visited);
        Assert.Equal(1, stopped.X);
        Assert.Equal(2, stopped.Y);

        var walked = VmPPsprBob1154Slice.BobWeapon(true, 4, 1, 2, new[]
        {
            new VmPPsprBob1154Slice.BobItem(0, true, 5, 6),
            new VmPPsprBob1154Slice.BobItem(0, false, 0, 0),
            new VmPPsprBob1154Slice.BobItem(0, true, 7, 8),
        });
        Assert.Equal(3, walked.Visited);
        Assert.Equal(7, walked.X);
        Assert.Equal(8, walked.Y);

        var reset3 = VmPPsprBob1154Slice.BobWeapon3D(false, 3, 1, 0, 0, 0, 1, 0, Array.Empty<VmPPsprBob1154Slice.BobItem3>());
        Assert.True(reset3.Reset);
        Assert.False(reset3.Interpolate2D);

        var bob3 = VmPPsprBob1154Slice.BobWeapon3D(true, 3, 1, 2, 3, 4, 5, 6, new[]
        {
            new VmPPsprBob1154Slice.BobItem3(0, true, 9, 8, 7, 6, 5, 4),
            new VmPPsprBob1154Slice.BobItem3(VmPPsprBob1154Slice.Euthanize, true, 1, 1, 1, 1, 1, 1),
        });
        Assert.True(bob3.Set);
        Assert.False(bob3.Interpolate2D);
        Assert.Equal(1, bob3.CallFrac);
        Assert.Equal(3, bob3.Timer);
        Assert.Equal(1, bob3.Visited);
        Assert.Equal(9, bob3.Tx);
        Assert.Equal(8, bob3.Ty);
        Assert.Equal(7, bob3.Tz);
        Assert.Equal(6, bob3.Rx);
        Assert.Equal(4, bob3.Rz);
    }

    [Fact]
    public void Buttons_PreferZoomAndSkipAMissingState()
    {
        var checks = new[]
        {
            new VmPPsprBob1154Slice.WeaponButton(VmPPsprBob1154Slice.WeaponZoomOk, VmPPsprBob1154Slice.ZoomButton, true, 11),
            new VmPPsprBob1154Slice.WeaponButton(VmPPsprBob1154Slice.WeaponReloadOk, VmPPsprBob1154Slice.ReloadButton, true, 22),
            new VmPPsprBob1154Slice.WeaponButton(VmPPsprBob1154Slice.User1Ok, VmPPsprBob1154Slice.User1Button, true, 33),
        };
        Assert.False(VmPPsprBob1154Slice.CheckButtons(false, VmPPsprBob1154Slice.WeaponZoomOk, VmPPsprBob1154Slice.ZoomButton, checks).Changed);

        var zoom = VmPPsprBob1154Slice.CheckButtons(
            true,
            VmPPsprBob1154Slice.WeaponZoomOk | VmPPsprBob1154Slice.WeaponReloadOk,
            VmPPsprBob1154Slice.ZoomButton | VmPPsprBob1154Slice.ReloadButton,
            checks);
        Assert.True(zoom.Changed);
        Assert.Equal(11, zoom.StateId);

        var missing = checks.ToArray();
        missing[0] = new VmPPsprBob1154Slice.WeaponButton(VmPPsprBob1154Slice.WeaponZoomOk, VmPPsprBob1154Slice.ZoomButton, false, 11);
        var reload = VmPPsprBob1154Slice.CheckButtons(
            true,
            VmPPsprBob1154Slice.WeaponZoomOk | VmPPsprBob1154Slice.WeaponReloadOk,
            VmPPsprBob1154Slice.ZoomButton | VmPPsprBob1154Slice.ReloadButton,
            missing);
        Assert.Equal(22, reload.StateId);

        var quiet = VmPPsprBob1154Slice.CheckButtons(true, VmPPsprBob1154Slice.User2Ok, VmPPsprBob1154Slice.User1Button, checks);
        Assert.False(quiet.Changed);
        Assert.Equal(1 << 9, VmPPsprBob1154Slice.User2Ok);
        Assert.Equal(1 << 22, VmPPsprBob1154Slice.User2Button);
        Assert.Equal(1 << 10, VmPPsprBob1154Slice.User3Ok);
        Assert.Equal(1 << 23, VmPPsprBob1154Slice.User3Button);
        Assert.Equal(1 << 11, VmPPsprBob1154Slice.User4Ok);
        Assert.Equal(1 << 24, VmPPsprBob1154Slice.User4Button);
    }
}
