namespace HCDE.MapLoader;

/// <summary>
/// <c>SetLineID</c>, <c>SaveLineSpecial</c>, and <c>FinishLoadingLineDef(s)</c> from <c>maploader.cpp</c> (1472–1645).
/// </summary>
public static class VmMapLoaderLineDefFinish
{
    public const int MapTypeHexen = 1;

    public enum HexenLineSpecial
    {
        SetIdentification = 1,
        TranslucentLine = 2,
        TeleportLine = 3,
        ScrollTextureModel = 4,
        PolyobjStartLine = 5,
        PolyobjExplicitLine = 6,
        PlaneAlign = 7,
        StaticInit = 8,
        SetPortal = 9,
    }

    public static int? ResolveHexenLineId(
        int mapType,
        int special,
        int arg0,
        int arg1,
        int arg2,
        int arg3,
        int arg4,
        bool levelHexenHack,
        bool noSlopeIdCompat)
    {
        if (mapType != MapTypeHexen)
            return null;

        var setId = -1;
        switch ((HexenLineSpecial)special)
        {
            case HexenLineSpecial.SetIdentification:
                setId = levelHexenHack ? arg0 : arg0 + 256 * arg4;
                break;
            case HexenLineSpecial.TranslucentLine:
                setId = arg0;
                break;
            case HexenLineSpecial.TeleportLine:
            case HexenLineSpecial.ScrollTextureModel:
                setId = arg0;
                break;
            case HexenLineSpecial.PolyobjStartLine:
                setId = arg3;
                break;
            case HexenLineSpecial.PolyobjExplicitLine:
                setId = arg4;
                break;
            case HexenLineSpecial.PlaneAlign:
                if (!noSlopeIdCompat)
                    setId = arg2;
                break;
            case HexenLineSpecial.StaticInit:
                if (arg1 == 1)
                    setId = arg0;
                break;
            case HexenLineSpecial.SetPortal:
                setId = arg1;
                break;
        }

        return setId == -1 ? null : setId;
    }

    public static bool ShouldSaveLineSpecial(bool hasFrontSide, int special, int staticInitKind)
    {
        if (!hasFrontSide)
            return false;
        return special != (int)HexenLineSpecial.StaticInit || staticInitKind == 2;
    }

    public static (int special, int tag) PlanSavedSideSpecial(int special, int tag, int staticInitKind)
    {
        if (special != (int)HexenLineSpecial.StaticInit || staticInitKind == 2)
            return (special, tag);
        return (0, tag);
    }

    public static int ComputeTexelLength(double dx, double dy) =>
        (int)(Math.Sqrt(dx * dx + dy * dy) + 0.5);

    public readonly struct TranslucentApply
    {
        public TranslucentApply(double alpha, bool additive, bool applyToLineGroup)
        {
            Alpha = alpha;
            Additive = additive;
            ApplyToLineGroup = applyToLineGroup;
        }

        public double Alpha { get; }
        public bool Additive { get; }
        public bool ApplyToLineGroup { get; }
    }

    public static TranslucentApply? ResolveTranslucentLine(
        int special,
        int alphaArg,
        int groupId,
        int passedAlpha)
    {
        if (special != (int)HexenLineSpecial.TranslucentLine)
            return null;

        var alpha = passedAlpha;
        var additive = false;
        if (passedAlpha == short.MinValue)
        {
            alpha = alphaArg;
            additive = groupId != 0;
        }
        else if (passedAlpha < 0)
        {
            alpha = -passedAlpha;
            additive = true;
        }

        return new TranslucentApply(alpha / 255.0, additive, groupId != 0);
    }
}
