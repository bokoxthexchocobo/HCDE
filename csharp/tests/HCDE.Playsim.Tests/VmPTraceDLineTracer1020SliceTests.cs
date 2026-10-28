namespace HCDE.Playsim.Tests;

public class VmPTraceDLineTracer1020SliceTests
{
    [Fact]
    public void SanitizeZScriptTraceFlags_ClearsPcrossImpactAndSets3DCallback()
    {
        var raw = VmPTraceDLineTracer1020Slice.TracePcross |
                  VmPTraceDLineTracer1020Slice.TraceImpact |
                  0x100;
        var sanitized = VmPTraceDLineTracer1020Slice.SanitizeZScriptTraceFlags(raw);
        Assert.Equal(0x120, sanitized);
        Assert.Equal(0x100 | VmPTraceDLineTracer1020Slice.Trace3DCallback, sanitized);
    }

    [Fact]
    public void ActorMaskForIgnoreAll_MatchesNativeTernary()
    {
        Assert.Equal(0u, VmPTraceDLineTracer1020Slice.ActorMaskForIgnoreAll(true));
        Assert.Equal(0xFFFFFFFFu, VmPTraceDLineTracer1020Slice.ActorMaskForIgnoreAll(false));
    }

    [Fact]
    public void TraceResultsVmFieldCount_MatchesNativeDefineBlock()
    {
        Assert.Equal(19, VmPTraceDLineTracer1020Slice.TraceResultsVmFieldCount);
        Assert.True(VmPTraceDLineTracer1020Slice.TryGetTraceResultsVmFieldName(0, out var first));
        Assert.Equal("HitSector", first);
        Assert.True(VmPTraceDLineTracer1020Slice.TryGetTraceResultsVmFieldName(18, out var last));
        Assert.Equal("Crossed3DWaterPos", last);
    }

    [Fact]
    public void TryPatchWallHitTexture_MapsTiersLikeNativeCallback()
    {
        var req = new VmPTraceDLineTracer1020Slice.WallTexturePatchRequest(
            VmPTraceDLineTracer1020Slice.TraceHitWall,
            VmPTraceDLineTracer1020Slice.TierUpper,
            SideMiddleTexture: 1,
            SideUpperTexture: 2,
            SideLowerTexture: 3,
            FfloorFlags: 0,
            FfloorMasterMiddleTexture: 4,
            FfloorMasterUpperTexture: 5,
            FfloorMasterLowerTexture: 6);

        Assert.True(VmPTraceDLineTracer1020Slice.TryPatchWallHitTexture(req, out var tex));
        Assert.Equal(2, tex);
    }

    [Fact]
    public void SelectFfloorTexturePart_UsesUpperLowerOrMiddle()
    {
        var upper = new VmPTraceDLineTracer1020Slice.WallTexturePatchRequest(
            VmPTraceDLineTracer1020Slice.TraceHitWall,
            VmPTraceDLineTracer1020Slice.TierFfloor,
            0, 0, 0,
            VmPTraceDLineTracer1020Slice.FfUpperTexture,
            10, 11, 12);
        Assert.Equal(11, VmPTraceDLineTracer1020Slice.SelectFfloorTexturePart(upper));

        var lower = upper with { FfloorFlags = VmPTraceDLineTracer1020Slice.FfLowerTexture };
        Assert.Equal(12, VmPTraceDLineTracer1020Slice.SelectFfloorTexturePart(lower));

        var mid = upper with { FfloorFlags = 0 };
        Assert.Equal(10, VmPTraceDLineTracer1020Slice.SelectFfloorTexturePart(mid));
    }

    [Fact]
    public void ResolveTraceCallbackStatus_StopWhenNoVirtual()
    {
        Assert.Equal(
            VmPTraceDLineTracer1020Slice.TraceStop,
            VmPTraceDLineTracer1020Slice.ResolveTraceCallbackStatus(false, () => 99));
    }
}
