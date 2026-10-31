namespace HCDE.Client;

/// <summary>
/// File head from <c>sbarinfo.cpp</c> (**1–149**): HUD art indices, coordinate packing, draw offsets, and
/// <c>GetCoordinates</c> loop prologue; line **150+** continues in chunk **1063**.
/// </summary>
public static class VmSbarinfoFileHead1098Slice
{
    public enum StatusBarArtIndex
    {
        ArtiBox = 0,
        SelectBox = 1,
        Cursor = 2,
        InvLfGem1 = 3,
        InvRtGem1 = 4
    }

    [Flags]
    public enum DrawOffsetAnchor
    {
        Top = 0x1,
        VMiddle = 0x2,
        Bottom = 0x4,
        Left = 0x10,
        Right = 0x20,
        HMiddle = 0x40,
        Center = VMiddle | HMiddle,
        CenterBottom = Bottom | HMiddle
    }

    public static int PackCoordinate(int value, bool relativeCenter) =>
        relativeCenter ? value | int.MinValue : value & int.MaxValue;

    public static (int Value, bool RelativeCenter) UnpackCoordinate(int packed) =>
        (packed & int.MaxValue, (packed & int.MinValue) != 0);

    public static int AddCoordinate(int packed, int delta) =>
        PackCoordinate(UnpackCoordinate(packed).Value + delta, UnpackCoordinate(packed).RelativeCenter);

    public static int CoordinateValue(int packed) => UnpackCoordinate(packed).Value;

    public static bool CoordinateRelativeCenter(int packed) => UnpackCoordinate(packed).RelativeCenter;

    public static int SubtractCoordinate(int packed, int delta) => AddCoordinate(packed, -delta);

    public static int AddPackedCoordinates(int leftPacked, int rightValue) =>
        AddCoordinate(leftPacked, rightValue);

    public static int SubtractPackedCoordinates(int leftPacked, int rightValue) =>
        SubtractCoordinate(leftPacked, rightValue);

    public static bool OffsetUsesVerticalAnchor(DrawOffsetAnchor anchor, DrawOffsetAnchor mask) =>
        (anchor & mask) != 0;

    public static bool OffsetUsesHorizontalAnchor(DrawOffsetAnchor anchor, DrawOffsetAnchor mask) =>
        (anchor & mask) != 0;

    public static int StatusBarArtFlashOffset(int invBarOffset) => invBarOffset + 6;

    public static int GetCoordinatesAxisCount() => 2;

    public static bool IsValidStatusBarArtIndex(int index) =>
        index >= (int)StatusBarArtIndex.ArtiBox && index <= (int)StatusBarArtIndex.InvRtGem1;

    public static DrawOffsetAnchor CombineOffsetAnchors(DrawOffsetAnchor first, DrawOffsetAnchor second) =>
        first | second;

    public static int NegateCoordinate(int packed)
    {
        var (value, rel) = UnpackCoordinate(packed);
        return PackCoordinate(-value, rel);
    }

    public static bool AnchorsSpecifyCenter(DrawOffsetAnchor anchor) =>
        OffsetUsesVerticalAnchor(anchor, DrawOffsetAnchor.VMiddle)
        && OffsetUsesHorizontalAnchor(anchor, DrawOffsetAnchor.HMiddle);

    public static DrawOffsetAnchor ParseOffsetAnchorToken(string token) =>
        token.ToLowerInvariant() switch
        {
            "top" => DrawOffsetAnchor.Top,
            "vmiddle" => DrawOffsetAnchor.VMiddle,
            "bottom" => DrawOffsetAnchor.Bottom,
            "left" => DrawOffsetAnchor.Left,
            "right" => DrawOffsetAnchor.Right,
            "hmiddle" => DrawOffsetAnchor.HMiddle,
            _ => DrawOffsetAnchor.Top
        };

    public static bool GetCoordinatesLoopRequiresComma(int axisIndex) => axisIndex > 0;

    public static (bool ResetNegative, bool ResetRelativeCenter) GetCoordinatesLoopIterationDefaults() =>
        (ResetNegative: true, ResetRelativeCenter: true);
}
