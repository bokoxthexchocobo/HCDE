namespace HCDE.Client;

/// <summary>
/// Protrusion tail seam, <c>ParseMugShotBlock</c>, <c>newImage</c>, <c>Init</c> defaults, and <c>Popup</c> init/tick head from <c>sbarinfo.cpp</c> (**751–900**).
/// Chunk **1067**; follows **1066** protrusion/popup parse (**601–750**). Popup transition enum reuses <see cref="VmSbarinfoParseTopLevel1066Slice"/>.
/// </summary>
public static class VmSbarinfoMugShotPopup1067Slice
{
    public const int HudSlotCount = 9;
    public const int MugShotGraphicMaxLength = 5;
    public const int DefaultInterpolationSpeed = 8;
    public const int DefaultResWidth = 320;
    public const int DefaultResHeight = 200;

    public enum ImagePatchKind
    {
        NullOrEmpty,
        Named
    }

    public readonly record struct SBarInfoInitDefaults(
        bool AutomapBar,
        bool InterpolateHealth,
        bool InterpolateArmor,
        bool CompleteBorder,
        bool LowerHealthCap,
        int InterpolationSpeed,
        int ArmorInterpolationSpeed,
        int Height,
        char SpacingCharacter,
        VmSbarinfoParseTopLevel1065Slice.SpacingAlignment SpacingAlignment,
        int ResW,
        int ResH);

    public static SBarInfoInitDefaults CreateNativeInitDefaults() =>
        new(
            AutomapBar: false,
            InterpolateHealth: false,
            InterpolateArmor: false,
            CompleteBorder: false,
            LowerHealthCap: true,
            InterpolationSpeed: DefaultInterpolationSpeed,
            ArmorInterpolationSpeed: DefaultInterpolationSpeed,
            Height: 0,
            SpacingCharacter: '\0',
            SpacingAlignment: VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Center,
            ResW: DefaultResWidth,
            ResH: DefaultResHeight);

    public static bool ShouldFailMugShotGraphicLength(int charCount) => charCount > MugShotGraphicMaxLength;

    public static int ResolveMugShotFrameDelay(bool hasNegativePrefix, int magnitude) =>
        (hasNegativePrefix ? -1 : 1) * magnitude;

    public static ImagePatchKind ClassifyImagePatch(string patchName)
    {
        if (string.IsNullOrEmpty(patchName))
            return ImagePatchKind.NullOrEmpty;
        if (patchName.Equals("nullimage", StringComparison.OrdinalIgnoreCase))
            return ImagePatchKind.NullOrEmpty;
        return ImagePatchKind.Named;
    }

    public static int MergeImageIndexAfterLookup(
        ImagePatchKind patchKind,
        bool foundExisting,
        int existingIndex,
        int indexAfterPush)
    {
        if (patchKind == ImagePatchKind.NullOrEmpty)
            return -1;
        return foundExisting ? existingIndex : indexAfterPush;
    }

    public static (int X, int Y, double Alpha) ResolvePopupInitState(
        VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind transition,
        int width,
        int height)
    {
        return transition switch
        {
            VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.SlideInBottom
                or VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.PushUp => (0, height, 1.0),
            VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.Fade => (0, 0, 0.0),
            _ => (width, height, 1.0)
        };
    }

    public static int ComputeSlidePopupYOffset(bool opened, int height, int currentY, int ispeed)
    {
        if (opened)
            return currentY - Math.Clamp(height + (currentY - height), 1, ispeed);
        return currentY + Math.Clamp(height - currentY, 1, ispeed);
    }

    public static int ComputePushUpDisplacementDelta(int oldY, int newY) => newY - oldY;

    public static bool ShouldKeepPopupMovingOnSlideAxis(int y, int height) => y != 0 && y != height;

    public static double AdvanceFadePopupAlpha(bool opened, double alpha, double speed, double speed2) =>
        opened ? Math.Clamp(alpha + speed, 0.0, 1.0) : Math.Clamp(alpha - speed2, 0.0, 1.0);

    public static bool ShouldIterateHudResetSlot(int index, int hudSlotCount) => index < hudSlotCount;
}
