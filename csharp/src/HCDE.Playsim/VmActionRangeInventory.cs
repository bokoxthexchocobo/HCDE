namespace HCDE.Playsim;

/// <summary>Range checks, inventory drop, blend, countdown, burst from <c>p_actionfunctions.cpp</c> (1954–2123).</summary>
public static class VmActionRangeInventory
{
    public const int MaxCountdownArg = 5;
    public const int WhiteArgb = unchecked((int)0xFFFFFFFF);
    public const int MfMissile = 1 << 2;
    public const int MfShootable = 1 << 9;

    public static double SquaredRange(double range) => range * range;

    public static bool CheckRangeShouldReturnFalse(bool inRange) => inRange;

    public static bool ShouldCheckCamera(bool hasCamera, bool cameraIsPlayer) =>
        hasCamera && !cameraIsPlayer;

    public static bool ShouldDropInventory(object? dropClass, object? inventory) =>
        dropClass is not null && inventory is not null;

    public static int NormalizeBlendColor(int color) =>
        color == WhiteArgb ? 0 : color;

    public static double BlendAlphaSeconds(int tics, double ticRate) => tics / ticRate;

    public static bool CountdownArgInRange(int index) => index >= 0 && index < MaxCountdownArg;

    public static int DecrementCountdownArg(int value) => value - 1;

    public static bool ShouldJumpWhenCountdownZero(int value) => value <= 0;

    public static bool CountdownTriggersExplode(int flags) => (flags & MfMissile) != 0;

    public static bool CountdownTriggersDamage(int flags) => (flags & MfShootable) != 0;

    public static int BurstChunkCount(double radius, double height, int randomModulo) =>
        Math.Max(24, Math.Max(4, (int)(radius * height / 32)) + randomModulo);

    public static double BurstOffsetFromPr(int pr, double radius) =>
        (pr - 128) * radius / 128.0;

    public static double BurstZFromPr(int pr, double height) => pr * height / 255.0;

    public static double BurstShardVelocityZ(double moZ, double selfZ, double height) =>
        height == 0 ? 0 : 4 * (moZ - selfZ) / height;

    public static (double vx, double vy) BurstVelocityFromPr(int pr) => (pr / 128.0, pr / 128.0);

    public static bool BurstChunkNull(object? chunkClass) => chunkClass is null;

    public static bool ShouldBossDeathBurst(int flags4BossDeath) => flags4BossDeath != 0;

    public static int LoopBurstShards(int count) => Math.Max(0, count);

    public static double CopyBurstAlpha(double sourceAlpha) => sourceAlpha;

    public static bool CheckRangeLoop(
        IEnumerable<(bool inGame, bool playerInRange, bool cameraInRange)> players)
    {
        foreach (var (inGame, playerInRange, cameraInRange) in players)
        {
            if (!inGame)
                continue;
            if (CheckRangeShouldReturnFalse(playerInRange))
                return true;
            if (ShouldCheckCamera(true, true) && CheckRangeShouldReturnFalse(cameraInRange))
                return true;
        }

        return false;
    }

    public static (float r, float g, float b, float a) SplitBlendColor(int argb, float alpha) =>
        (((argb >> 16) & 255) / 255f, ((argb >> 8) & 255) / 255f, (argb & 255) / 255f, alpha);

    public static int CountdownPostDecrementAction(int flags, bool hasDeathState) =>
        CountdownTriggersExplode(flags) ? 1 : CountdownTriggersDamage(flags) ? 2 : hasDeathState ? 3 : 0;
}
