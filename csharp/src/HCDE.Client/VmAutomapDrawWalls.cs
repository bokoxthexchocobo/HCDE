namespace HCDE.Client;

/// <summary>
/// <c>AM_CheckSecret</c> and <c>DAutomap::drawWalls</c> from <c>am_map.cpp</c> (2315–2337, 2631–2784).
/// </summary>
public static class VmAutomapDrawWalls
{
    public enum SecretLineStyle
    {
        None = 0,
        SecretSector = 1,
        UnexploredSecret = 2,
    }

    public enum WallDrawStyle
    {
        Skip,
        CustomAutomapStyle,
        Portal,
        SecretSector,
        UnexploredSecret,
        SecretDoor,
        SecretWall,
        NormalWall,
        IntraTeleport,
        InterTeleport,
        LockColored,
        Locked,
        SpecialTrigger,
        OneSided,
        FloorDiff,
        CeilingDiff,
        ExtraFloor,
        TweenSecret,
        NotSeen,
    }

    public static SecretLineStyle CheckSecret(
        bool secretColorValid,
        bool frontWasSecret,
        bool frontIsSecret,
        bool backWasSecret,
        bool backIsSecret,
        int amMapSecrets,
        ushort lineFlags,
        ushort mlSecret)
    {
        if (!secretColorValid)
            return SecretLineStyle.None;

        if (frontWasSecret)
        {
            if (amMapSecrets != 0 && !frontIsSecret)
                return SecretLineStyle.SecretSector;
            if (amMapSecrets == 2 && (lineFlags & mlSecret) == 0)
                return SecretLineStyle.UnexploredSecret;
        }

        if (backWasSecret)
        {
            if (amMapSecrets != 0 && !backIsSecret)
                return SecretLineStyle.SecretSector;
            if (amMapSecrets == 2 && (lineFlags & mlSecret) == 0)
                return SecretLineStyle.UnexploredSecret;
        }

        return SecretLineStyle.None;
    }

    public static bool ShouldSkipDontDraw(int lineFlags, int dontDrawFlag, int amCheat, bool showAllEnabled, bool cheatMode) =>
        (lineFlags & dontDrawFlag) != 0 && (amCheat == 0 || amCheat >= 4)
        && (!showAllEnabled || cheatMode);

    public static int ResolvePortalGroup(bool polyobjWall, int linePortalGroup, int sampledPortalGroup) =>
        polyobjWall ? sampledPortalGroup : linePortalGroup;

    public static bool ShouldDrawPortalPass(int portalGroup, int pass, int mapPortalGroup, bool portalOverlay)
    {
        if (pass == mapPortalGroup)
            return false;

        if (portalGroup == pass)
            return true;

        return pass == -1 && (portalGroup == mapPortalGroup || !portalOverlay);
    }

    public static WallDrawStyle ClassifyVisibleWall(
        int amCheat,
        ushort lineFlags,
        ushort mappedFlag,
        ushort dontDrawFlag,
        bool showAllEnabled,
        bool cheatMode,
        int automapStyle,
        int automapStyleMax,
        bool portalMode,
        SecretLineStyle secretStyle,
        ushort mlSecretFlag,
        bool hasBacksector,
        bool isTeleport,
        bool teleportColorValid,
        bool isExit,
        bool exitColorValid,
        bool isLock,
        bool displayLocks,
        bool showTriggerLines,
        bool triggerColorValid,
        bool isTrigger,
        bool floorPlanesDiffer,
        bool ceilingPlanesDiffer,
        bool has3dFloorBorder)
    {
        if (ShouldSkipDontDraw(lineFlags, dontDrawFlag, amCheat, showAllEnabled, cheatMode))
            return WallDrawStyle.Skip;

        if (automapStyle > 0 && automapStyle < automapStyleMax && (amCheat == 0 || amCheat >= 4))
            return WallDrawStyle.CustomAutomapStyle;

        if (portalMode)
            return WallDrawStyle.Portal;

        if (secretStyle == SecretLineStyle.SecretSector)
            return WallDrawStyle.SecretSector;
        if (secretStyle == SecretLineStyle.UnexploredSecret)
            return WallDrawStyle.UnexploredSecret;

        if ((lineFlags & mlSecretFlag) != 0)
            return amCheat != 0 && amCheat < 4 && hasBacksector
                ? WallDrawStyle.SecretWall
                : WallDrawStyle.SecretDoor;

        if (isTeleport && teleportColorValid)
            return WallDrawStyle.IntraTeleport;
        if (isExit && exitColorValid)
            return WallDrawStyle.InterTeleport;

        if (isLock)
            return displayLocks ? WallDrawStyle.LockColored : WallDrawStyle.Locked;

        if (showTriggerLines && triggerColorValid && isTrigger)
            return WallDrawStyle.SpecialTrigger;

        if (!hasBacksector)
            return WallDrawStyle.OneSided;
        if (floorPlanesDiffer)
            return WallDrawStyle.FloorDiff;
        if (ceilingPlanesDiffer)
            return WallDrawStyle.CeilingDiff;
        if (has3dFloorBorder)
            return WallDrawStyle.ExtraFloor;
        if (amCheat > 0 && amCheat < 4)
            return WallDrawStyle.TweenSecret;

        return WallDrawStyle.NormalWall;
    }

    public static WallDrawStyle ClassifyUnseenWall(
        bool allMap,
        ushort revealedFlag,
        ushort lineFlags,
        int amCheat,
        ushort dontDrawFlag,
        bool showAllEnabled,
        bool cheatMode)
    {
        if (!(allMap || (lineFlags & revealedFlag) != 0))
            return WallDrawStyle.Skip;

        if (ShouldSkipDontDraw(lineFlags, dontDrawFlag, amCheat, showAllEnabled, cheatMode))
            return WallDrawStyle.Skip;

        return WallDrawStyle.NotSeen;
    }
}
