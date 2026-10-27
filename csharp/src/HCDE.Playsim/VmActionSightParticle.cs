namespace HCDE.Playsim;

/// <summary><c>A_SpawnParticleEx</c> and sight/range checks from <c>p_actionfunctions.cpp</c> (1777–1970).</summary>
public static class VmActionSightParticle
{
    public const int StyleCount = 20;

    public static int NormalizeParticleStyle(int style) =>
        style < 0 || style >= StyleCount ? 0 : style;

    public static double ClampParticleStartAlpha(double startAlpha) => Math.Clamp(startAlpha, 0.0, 1.0);

    public static double ClampFadeStep(double fadestep) => Math.Clamp(fadestep, -1.0, 1.0);

    public static double AbsParticleSize(double size) => Math.Abs(size);

    public static bool ShouldSpawnParticleEx(int lifetime) => lifetime != 0;

    public static double SquaredRange(double range) => range * range;

    public static double VerticalSightDelta(double eyeZ, double actorTop, double actorFloorZ)
    {
        if (eyeZ > actorTop)
            return actorTop - eyeZ;
        if (eyeZ < actorFloorZ)
            return actorFloorZ - eyeZ;
        return 0;
    }

    public static double SquaredDistance2d(double dx, double dy, double dz, bool twoDi) =>
        dx * dx + dy * dy + (twoDi ? 0 : dz * dz);

    public static bool WithinRange(double distanceSquared, double rangeSquared) =>
        distanceSquared <= rangeSquared;

    public static (double x, double y) RotateParticleExRelPos(double xoff, double yoff, double sin, double cos) =>
        (xoff * cos + yoff * sin, xoff * sin - yoff * cos);

    public static bool DoCheckSightOrRange(
        bool cameraIsNull,
        double dx,
        double dy,
        double dz,
        bool twoDi,
        double rangeSquared,
        bool checkSight,
        bool inSight)
    {
        if (cameraIsNull)
            return false;

        var distance = SquaredDistance2d(dx, dy, dz, twoDi);
        if (WithinRange(distance, rangeSquared))
            return true;

        return checkSight && inSight;
    }

    public static bool RangeOnlyHit(
        bool cameraIsNull,
        double dx,
        double dy,
        double dz,
        bool twoDi,
        double rangeSquared) =>
        DoCheckSightOrRange(cameraIsNull, dx, dy, dz, twoDi, rangeSquared, false, false);

    public static bool PlayerSightOrRangeHit(
        bool playerInGame,
        bool primaryHit,
        bool hasCamera,
        bool cameraIsPlayer,
        bool cameraHit) =>
        playerInGame
        && (primaryHit
            || (hasCamera && !cameraIsPlayer && cameraHit));

    public static double SightOrRangeLengthSquared(double dx, double dy, double dz, bool twoDi) =>
        SquaredDistance2d(dx, dy, dz, twoDi);

    public static bool SkipSpawnWhenLifetimeZero(int lifetime) => lifetime == 0;
}
