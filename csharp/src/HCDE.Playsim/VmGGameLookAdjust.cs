namespace HCDE.Playsim;

/// <summary><c>LookAdjust</c>, <c>G_AddViewPitch</c>, and <c>G_AddViewAngle</c> from <c>g_game.cpp</c> (1130–1204).</summary>
public static class VmGGameLookAdjust
{
    public const int PitchClamp = 0x78000000;
    public const int LookFixedShift = 16;

    public static int PrepareLookInput(int look) => look << LookFixedShift;

    public static int ScaleLook(int look, double fovScale, double lookScale, bool hasWeapon)
    {
        if (!hasWeapon)
            return look;
        if (fovScale > 0)
            look = (int)(look * fovScale);
        if (lookScale > 0)
            look = (int)(look * lookScale);
        return look;
    }

    public static bool ShouldSkipTitleLevel(int gameState, int gsTitleLevel) => gameState == gsTitleLevel;

    public static bool ShouldClearPitchWhenFreelookBlocked(bool freelookAllowed) => !freelookAllowed;

    public static int ApplyPitchUp(int current, int look, bool freelookAllowed)
    {
        if (!freelookAllowed)
            return 0;
        if (look <= 0)
            return current;
        return ClampPitchUp(current, look);
    }

    public static int ApplyPitchDown(int current, int look, bool freelookAllowed)
    {
        if (!freelookAllowed)
            return 0;
        if (look >= 0)
            return current;
        return ClampPitchDown(current, look);
    }

    public static int ClampPitchUp(int current, int look)
    {
        if (current > int.MaxValue - look)
            return PitchClamp;
        return Math.Min(current + look, PitchClamp);
    }

    public static int ClampPitchDown(int current, int look)
    {
        if (current < int.MinValue - look)
            return -PitchClamp;
        return Math.Max(current + look, -PitchClamp);
    }

    public static bool ShouldMarkKeyboardTurner(int delta) => delta != 0;

    public static int ApplyViewAngle(int localAngle, int yaw) => localAngle - yaw;

    public static bool IsPlayerDead(int playerState, int pstDead) => playerState == pstDead;
}
