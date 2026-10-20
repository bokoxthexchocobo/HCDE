namespace HCDE.Client;

/// <summary><c>AMColorset</c> indices from <c>am_map.cpp</c> (357–424).</summary>
public static class VmAutomapColorsetIndex
{
    public const int Background = 0;
    public const int YourColor = 1;
    public const int WallColor = 2;
    public const int ThingColor = 7;
    public const int PortalColor = 24;

    public static int ColorNameCount => ColorNames.Length;

    public static readonly string[] ColorNames =
    [
        "Background", "YourColor", "WallColor", "TwoSidedWallColor",
        "FloorDiffWallColor", "CeilingDiffWallColor", "ExtraFloorWallColor",
        "ThingColor", "ThingColor_Item", "ThingColor_CountItem",
        "ThingColor_Monster", "ThingColor_NocountMonster", "ThingColor_Friend",
        "SpecialWallColor", "SecretWallColor", "GridColor", "XHairColor",
        "NotSeenColor", "LockedColor", "IntraTeleportColor", "InterTeleportColor",
        "SecretSectorColor", "UnexploredSecretColor", "SectorFillColor", "PortalColor",
        "AlmostBackgroundColor"
    ];
}
