namespace HCDE.Playsim;

/// <summary>
/// Early actor networking, state tics, <c>P_CheckMissileSpawn</c>, and spawn sound
/// from <c>p_mobj.cpp</c> (122–200, 834–846, 7756–7908).
/// </summary>
public static class VmPMobjMissileSpawnCheck
{
    public const int Mf2Rip = 1;
    public const int Mf2Mcross = 1 << 1;
    public const int Mf2Pcross = 1 << 2;
    public const int MfMissile = 1;
    public const int BounceMbf = 1;
    public const int Mf4Randomize = 1;
    public const int LineHorizon = 1;
    public const int Mf5DonTrip = 1;
    public const int Mf7Handlenodelay = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int ChSpawnsoundsource = 1;

    public static bool ShouldAbortNetworkingDisable(bool enable, bool isClientSide) =>
        !enable && !isClientSide;

    public static double SanitizeSvGravity(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? 800 : value;

    public static int PropagateMarkBehaviors(IReadOnlyList<bool> behaviorPresent)
    {
        var marked = 0;
        for (var i = 0; i < behaviorPresent.Count; i++)
        {
            if (behaviorPresent[i])
                marked++;
        }

        return marked;
    }

    public static int GetTics(bool clientSide, int normalTics, int clientTics, bool isFast, bool stateFast, bool isSlow, bool stateSlow)
    {
        var tics = clientSide ? clientTics : normalTics;
        if (isFast && stateFast)
            return tics - (tics >> 1);
        if (isSlow && stateSlow)
            return tics << 1;
        return tics;
    }

    public static int RandomizeMissileTics(int tics, int randomRoll)
    {
        if (tics <= 0)
            return tics;
        tics -= randomRoll & 3;
        return tics < 1 ? 1 : tics;
    }

    public static (double X, double Y, double Z) AdvanceSpawnPosition(
        double velX,
        double velY,
        double velZ,
        double maxDist)
    {
        var ax = velX;
        var ay = velY;
        var az = velZ;
        if (maxDist > 0)
        {
            var maxSquared = maxDist * maxDist;
            while (ax * ax + ay * ay >= maxSquared)
            {
                ax *= 0.5;
                ay *= 0.5;
                az *= 0.5;
            }
        }

        return (ax, ay, az);
    }

    public static bool MbfGrenadeSpawnPolicy(int flags, int bounceFlags) =>
        (flags & MfMissile) == 0 || (bounceFlags & BounceMbf) != 0;

    public static int StripMissileCrossFlags(int flags2) => flags2 & ~(Mf2Mcross | Mf2Pcross);

    public readonly record struct MissileSpawnBlockContext(
        bool TryMoveFailed,
        bool IsRip,
        int BlockerFlags5,
        int BlockingLineSpecial,
        bool MbfGrenade,
        bool HasBlockingLine);

    public static bool ShouldExplodeOnBlock(MissileSpawnBlockContext ctx)
    {
        if (!ctx.TryMoveFailed)
            return false;
        if (ctx.IsRip && (ctx.BlockerFlags5 & Mf5DonTrip) == 0)
            return false;
        if (ctx.BlockingLineSpecial == LineHorizon)
            return false;
        return !ctx.MbfGrenade || !ctx.HasBlockingLine;
    }

    public static bool ShouldBounceWallOnBlock(MissileSpawnBlockContext ctx) =>
        ctx.TryMoveFailed && ctx.MbfGrenade && ctx.HasBlockingLine;

    public static bool ShouldDestroyOnHorizon(MissileSpawnBlockContext ctx) =>
        ctx.TryMoveFailed && ctx.BlockingLineSpecial == LineHorizon;

    public static bool PlaySpawnSoundOnMissile(bool spawnSoundFromSource) => !spawnSoundFromSource;

    public static bool PlaySpawnSoundOnSpawner(bool spawnSoundFromSource, bool spawnerNull) =>
        spawnSoundFromSource && !spawnerNull;

    public static bool PlaySpawnSoundAtPosition(bool spawnSoundFromSource, bool spawnerNull, bool silentSector) =>
        spawnSoundFromSource && spawnerNull && !silentSector;

    public static bool HasSeeSound(int seeSound) => seeSound != 0;
}
