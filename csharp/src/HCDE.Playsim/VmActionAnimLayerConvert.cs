namespace HCDE.Playsim;

/// <summary><c>AnimLayerToAnimInfo</c> from <c>p_actionfunctions.cpp</c> (7043–7078).</summary>
public static class VmActionAnimLayerConvert
{
    public static bool UseModelAnimNoneWhenLayerMissing(bool layerPresent, bool curAnimPresent) =>
        !layerPresent || !curAnimPresent;

    public static bool CopyCurAnimFields(bool layerPresent, bool curAnimPresent) =>
        layerPresent && curAnimPresent;

    public static bool TryBuildPrecalculatedPrev(bool layerPresent, bool prevPresent, bool prevIsPrecalculated) =>
        layerPresent && prevPresent && prevIsPrecalculated;

    public static bool TryBuildInterpolatedPrev(bool layerPresent, bool prevPresent, bool prevIsInterpolated) =>
        layerPresent && prevPresent && prevIsInterpolated;

    public static bool ClearPrevAnimVariant(bool layerPresent, bool prevPresent, bool precalc, bool interpolated) =>
        layerPresent && prevPresent && !precalc && !interpolated;

    public static (float Inter, int Frame1, int Frame2) InterpolatedPrevFields(float inter, int frame1, int frame2) =>
        (inter, frame1, frame2);
}
