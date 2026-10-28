namespace HCDE.Client;

/// <summary>
/// <c>DSBarInfo::DrawGraphic</c> coordinate, clip, and draw-path policy from <c>sbarinfo.cpp</c> (**1201–1350**).
/// Chunk **1070**; follows **1069** null-texture guard (**1198–1199**). Relative center uses <see cref="VmSbarinfoPopupDraw1068Slice"/> adjust helpers.
/// </summary>
public static class VmSbarinfoDrawGraphic1070Slice
{
    public const int ForceUseTextureDimension = -1;
    public const int OffsetCenter = 0x2 | 0x40;
    public const int OffsetCenterBottom = 0x4 | 0x40;
    public const int DimOverlayArgb = unchecked((int)0xAA000000);

    public static bool UsesCenterOffset(int offsetFlags) =>
        (offsetFlags & OffsetCenter) == OffsetCenter;

    public static bool UsesCenterBottomOffset(int offsetFlags) =>
        (offsetFlags & OffsetCenterBottom) == OffsetCenterBottom;

    public static double ApplyCenteredDrawX(
        double dx,
        double textureWidth,
        double leftOffset,
        int forceWidth)
    {
        if (forceWidth < 0)
            return dx - (textureWidth / 2.0) + leftOffset;
        var textureWidthSafe = textureWidth <= 0 ? 1.0 : textureWidth;
        return dx - forceWidth * (0.5 - leftOffset / textureWidthSafe);
    }

    public static double ApplyCenteredDrawY(
        double dy,
        double textureHeight,
        double topOffset,
        int forceHeight)
    {
        if (forceHeight < 0)
            return dy - (textureHeight / 2.0) + topOffset;
        var textureHeightSafe = textureHeight <= 0 ? 1.0 : textureHeight;
        return dy - forceHeight * (0.5 - topOffset / textureHeightSafe);
    }

    public static (double Dx, double Dy) ApplyPixelOffsets(double dx, double dy, int xOffset, int yOffset) =>
        (dx + xOffset, dy + yOffset);

    public static (double Width, double Height) ResolveDestSize(
        int forceWidth,
        int forceHeight,
        double textureWidth,
        double textureHeight) =>
        (forceWidth < 0 ? textureWidth : forceWidth,
            forceHeight < 0 ? textureHeight : forceHeight);

    public static double ComputeStatusbarClipLeft(double dx, double clipLeft, double leftOffset, bool clipDisabled) =>
        clipDisabled ? 0 : dx + clipLeft - leftOffset;

    public static double ComputeStatusbarClipTop(double dy, double clipTop, double topOffset, bool clipDisabled) =>
        clipDisabled ? 0 : dy + clipTop - topOffset;

    public static double ComputeStatusbarClipRight(
        double dx,
        double width,
        double clipRight,
        double leftOffset,
        bool clipDisabled) =>
        clipDisabled ? int.MaxValue : dx + width - clipRight - leftOffset;

    public static double ComputeStatusbarClipBottom(
        double dy,
        double height,
        double clipBottom,
        double topOffset,
        bool clipDisabled) =>
        clipDisabled ? int.MaxValue : dy + height - clipBottom - topOffset;

    public static int ResolveColorOverlay(bool dim) => dim ? DimOverlayArgb : 0;

    public static (double Rx, double Ry) ApplyFullscreenRelativeCenter(
        bool relativeX,
        bool relativeY,
        double dx,
        double dy,
        double viewWidth,
        double viewHeight,
        double scaleX,
        double scaleY)
    {
        var rx = VmSbarinfoPopupDraw1068Slice.AdjustRelativeCenterX(relativeX, dx, viewWidth, scaleX);
        var ry = VmSbarinfoPopupDraw1068Slice.AdjustRelativeCenterY(relativeY, dy, viewHeight, scaleY);
        return (rx, ry);
    }

    public static double ResolveFullscreenAnchor(
        double coordinate,
        double scaledPosition,
        bool coordinateNegative,
        bool relativeCenter,
        double viewSize) =>
        coordinateNegative && !relativeCenter ? viewSize + scaledPosition : scaledPosition;

    public static (double Width, double Height, double Rx, double Ry) ScaleFullscreenDrawMetrics(
        double rx,
        double ry,
        double width,
        double height,
        double scaleX,
        double scaleY) =>
        (width * scaleX, height * scaleY, rx * scaleX, ry * scaleY);

    public static bool HasAnyFullscreenClipComponent(
        bool clipLeftUnset,
        bool clipTopUnset,
        bool clipRightUnset,
        bool clipBottomUnset) =>
        !clipLeftUnset || !clipTopUnset || !clipRightUnset || !clipBottomUnset;
}
