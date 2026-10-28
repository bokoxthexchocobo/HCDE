namespace HCDE.Playsim;

/// <summary>
/// `G_BuildTiccmd` tail, look/view adjust, and `ChangeSpy` from <c>g_game.cpp</c> (**1101–1250**).
/// </summary>
public static class VmGGameLookSpy999Slice
{
    public const int MovementAxisShift = 8;
    public const int PitchClampLimit = 0x78000000;
    public const int LookInputShift = 16;
    public const uint Dmflag2DisallowSpying = 1u << 20;

    public static int ScaleMovementAxis(int axis) => axis << MovementAxisShift;

    public static (int forward, int side) ScaleMovementPair(int forward, int side) =>
        (ScaleMovementAxis(forward), ScaleMovementAxis(side));

    public static bool ShouldWriteWeaponSelectNet(int sendWeaponSlot, int weaponSlotNone) =>
        sendWeaponSlot != weaponSlotNone;

    public static bool ShouldEmitPeriodicInputTrace(int gametic, int lastTraceTic, int ticRate) =>
        gametic - lastTraceTic >= ticRate;

    public static bool HasNonZeroTiccmdInput(int forward, int side, int up, uint buttons) =>
        forward != 0 || side != 0 || up != 0 || buttons != 0;

    public static string FormatInputTraceLine(
        int gametic,
        int forward,
        int side,
        int up,
        uint buttons) =>
        $"tic={gametic} fwd={forward} side={side} up={up} buttons=0x{buttons:x8}";

    public static int AdjustLookWithWeaponScales(
        int look,
        bool isAliveWithReadyWeapon,
        double fovScale,
        double lookScale)
    {
        var adjusted = look << LookInputShift;
        if (!isAliveWithReadyWeapon)
            return adjusted;
        if (fovScale > 0)
            adjusted = (int)(adjusted * fovScale);
        if (lookScale > 0)
            adjusted = (int)(adjusted * lookScale);
        return adjusted;
    }

    public static int ApplyViewPitchDelta(
        int localPitch,
        int look,
        bool freelookAllowed,
        bool titleLevel)
    {
        if (titleLevel)
            return localPitch;
        if (!freelookAllowed)
            return 0;
        if (look > 0)
        {
            return localPitch > int.MaxValue - look
                ? PitchClampLimit
                : Math.Min(localPitch + look, PitchClampLimit);
        }

        if (look < 0)
        {
            return localPitch < int.MinValue - look
                ? -PitchClampLimit
                : Math.Max(localPitch + look, -PitchClampLimit);
        }

        return localPitch;
    }

    public static int ApplyViewAngleDelta(int localAngle, int yaw, bool titleLevel) =>
        titleLevel ? localAngle : localAngle - yaw;

    public static bool ShouldSkipChangeSpyOutsideLevel(int gameState, int gsLevel) =>
        gameState != gsLevel;

    public static bool ShouldRequestRevertCamera(bool cameraHasPlayer, bool demoPlayback) =>
        !cameraHasPlayer && !demoPlayback;

    public static bool IsSpyingDisallowedByDmflags(uint dmflags2) =>
        (dmflags2 & Dmflag2DisallowSpying) != 0;

    public static bool ShouldCheckTeamWhenSpying(bool demoPlayback, bool deathmatch) =>
        !demoPlayback && deathmatch;

    public static bool IsSpyTargetEligible(
        bool playerInGame,
        bool checkTeam,
        bool isTeammate,
        bool botAllowSpy,
        bool targetIsBot) =>
        playerInGame && (!checkTeam || isTeammate || (botAllowSpy && targetIsBot));

    public static int AdvanceSpyPlayerIndexChangeSpy(
        int consolePlayer,
        int startPlayerIndex,
        int spyCommand,
        int maxPlayers,
        Func<int, bool> isEligible)
    {
        if (spyCommand == VmGGameHead995Slice.SpyCancel)
            return consolePlayer;

        var step = VmGGameHead995Slice.ResolveSpyStep(spyCommand);
        if (step == 0)
            return startPlayerIndex;

        var mask = maxPlayers - 1;
        var pnum = startPlayerIndex;
        do
        {
            pnum = (pnum + step) & mask;
            if (isEligible(pnum))
                return pnum;
        } while (pnum != consolePlayer);

        return consolePlayer;
    }
}
