namespace HCDE.Playsim.Tests;

public class VmPMobjAdjustFloorClipTests
{
    [Fact]
    public void Skip_SpecialFloorclipFlag() =>
        Assert.True(VmPMobjAdjustFloorClip.SkipWhenSpecialFloorclip(VmPMobjAdjustFloorClip.Mf3Specialfloorclip));

    [Fact]
    public void TerrainClip_PicksShallowest() =>
        Assert.Equal(4, VmPMobjAdjustFloorClip.PickShallowestClip(8, 4));

    [Fact]
    public void Resolve_NoTouchingReturnsZero() =>
        Assert.Equal(0, VmPMobjAdjustFloorClip.ResolveFloorClip(10, false));

    [Fact]
    public void ThreeDFloor_RequiresSolidExists() =>
        Assert.True(VmPMobjAdjustFloorClip.Valid3dFloor(
            VmPMobjAdjustFloorClip.FfSolid | VmPMobjAdjustFloorClip.FfExists));

    [Fact]
    public void PlayerView_AdjustsWhenClipChanges() =>
        Assert.True(VmPMobjAdjustFloorClip.PlayerViewAdjust(true, true, 8, 4));

    [Fact]
    public void PlayerSub_RejectsInvalidPair() =>
        Assert.False(VmPMobjAdjustFloorClip.PlayerSubValid(true, false, false, true, true));

    [Fact]
    public void MorphSub_BlocksUnmorphedDummy() =>
        Assert.True(VmPMobjAdjustFloorClip.MorphSubstitutionBlocked(
            new VmPMobjAdjustFloorClip.MorphSubContext(false, false, false, false, VmPMobjAdjustFloorClip.MfUnmorphed, true, false, false, false, false, false)));

    [Fact]
    public void MorphSub_PlayerRequiresRealMo() =>
        Assert.True(VmPMobjAdjustFloorClip.PlayerMorphRequiresValidMo(
            new VmPMobjAdjustFloorClip.MorphSubContext(false, false, false, false, 0, true, false, true, true, true, false)));

    [Fact]
    public void MorphSub_BlocksOwnedInventory() =>
        Assert.True(VmPMobjAdjustFloorClip.MorphInventoryOwnedBlocked(false, false, true, false));

    [Fact]
    public void ComputeShallowestClip_PrefersTerrainTouch()
    {
        var touches = new[]
        {
            new VmPMobjAdjustFloorClip.FloorTouchSector(
                false, 0, 0, 6, Array.Empty<VmPMobjAdjustFloorClip.ThreeDFloorTouchContext>()),
            new VmPMobjAdjustFloorClip.FloorTouchSector(
                false, 0, 0, 2, Array.Empty<VmPMobjAdjustFloorClip.ThreeDFloorTouchContext>()),
        };
        Assert.Equal(2, VmPMobjAdjustFloorClip.ComputeShallowestClip(touches));
    }

    [Fact]
    public void RunAdjustFloorClip_AdjustsViewHeightWhenPlayerView()
    {
        var result = VmPMobjAdjustFloorClip.RunAdjustFloorClip(
            0,
            8,
            new[]
            {
                new VmPMobjAdjustFloorClip.FloorTouchSector(
                    false, 1, 1, 3, Array.Empty<VmPMobjAdjustFloorClip.ThreeDFloorTouchContext>()),
            },
            true,
            true,
            41,
            () => 0.25);
        Assert.Equal(3, result.FloorClip);
        Assert.Equal(36, result.ViewHeight);
        Assert.Equal(0.25, result.DeltaViewHeight);
    }

    [Fact]
    public void PlayerSub_RewiresMoPointer()
    {
        var oldP = new object();
        var newP = new object();
        var fields = new VmPMobjAdjustFloorClip.PlayerPointerFields { Mo = oldP };
        VmPMobjAdjustFloorClip.SubstitutePlayerPointers(fields, oldP, newP);
        Assert.Same(newP, fields.Mo);
    }

    [Fact]
    public void EvaluateMorph_AllowsValidMorph()
    {
        var ctx = new VmPMobjAdjustFloorClip.MorphSubContext(
            false, false, false, false, 0, true, false, false, false, false, false);
        Assert.True(VmPMobjAdjustFloorClip.EvaluateMorphSubstitution(ctx, false, false, out var preMorph));
        Assert.True(preMorph);
    }

}
