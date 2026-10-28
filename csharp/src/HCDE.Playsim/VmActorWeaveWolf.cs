namespace HCDE.Playsim;

/// <summary><c>A_Weave</c>, <c>A_LineEffect</c>, <c>A_WolfAttack</c> from <c>p_actionfunctions.cpp</c> (3484–3680).</summary>
public static class VmActorWeaveWolf
{
    public const int Mf5NoInteraction = 0x8000000;
    public const int MfNoBlockmap = 0x200000;
    public const int Mf6LineDone = 0x4000000;
    public const int WafNoRandom = 1;
    public const int WafUsePuff = 2;

    public static double WeaveOffset(int weaveIndex, double dist) =>
        Math.Sin(weaveIndex * Math.PI / 32.0) * dist;

    public static int AdvanceWeaveIndex(int weaveIndex, int speed) => (weaveIndex + speed) & 63;

    public static bool ShouldTryMove(bool noInteraction) => !noInteraction;

    public static bool LineEffectBlocked(bool lineDone) => lineDone;

    public static bool LineEffectSetsDone(bool executed, bool repeatSpecial) =>
        executed && !repeatSpecial;

    public static bool WolfAttackNoTarget(bool hasTarget) => !hasTarget;

    public static bool WolfAttackNoSight(bool canSee) => !canSee;

    public static int WolfChanceIndex(int health, int spawnHealthEighth) =>
        Math.Min(health / Math.Max(spawnHealthEighth, 1), 8);

    public static int WolfHitChance(int baseChance, int distanceBlocks, bool dodge) =>
        baseChance - distanceBlocks * (dodge ? 16 : 8);

    public static int WolfDamage(int maxDamage, bool noRandom, int randomRoll) =>
        noRandom ? maxDamage : 1 + (randomRoll % maxDamage);

    public static int WolfDamageAtRange(int damage, double dist, double pointBlank, double longRange)
    {
        if (dist >= pointBlank) damage >>= 1;
        if (dist >= longRange) damage >>= 1;
        return damage;
    }

    public static bool GhostHalvesDamage(bool isGhost, bool usePuff) =>
        isGhost && !usePuff;

    public static readonly int[] WolfRandomDecideChance = [192, 120, 120, 120, 64, 64, 32, 16, 0];

    public static int WolfDecideChance(int index) =>
        WolfRandomDecideChance[Math.Clamp(index, 0, WolfRandomDecideChance.Length - 1)];

    public static bool WolfDecidePasses(int randomValue, int chance) => randomValue >= chance;

    public static double WolfDistanceChebyshev(double dx, double dy) => Math.Max(Math.Abs(dx), Math.Abs(dy));

    public static int WolfDistanceBlocks(double dist, int blockSize) => (int)(dist / blockSize);

    public static int WolfBaseHitChance(double targetSpeedSq, double runSpeed) =>
        targetSpeedSq < runSpeed ? 256 : 160;

    public static bool WolfDodge(bool targetSeesAttacker, double angleDeg) =>
        targetSeesAttacker && angleDeg < 21.0;

    public static int WolfShadowPenalty(int hitchance, bool shadow) => shadow ? hitchance >> 2 : hitchance;

    public static bool SpawnBloodAllowed(bool noBlood, bool invulnerable, bool dormant) =>
        !noBlood && !invulnerable && !dormant;

    public static bool UsePuffInsteadOfBlood(bool usePuff, bool hasPuffType, bool spawnBlood) =>
        usePuff && hasPuffType && !spawnBlood;

    public readonly struct WeaveStep
    {
        public WeaveStep(double x, double y, double z, int weaveXy, int weaveZ)
        {
            X = x;
            Y = y;
            Z = z;
            WeaveXy = weaveXy;
            WeaveZ = weaveZ;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public int WeaveXy { get; }
        public int WeaveZ { get; }
    }

    public static WeaveStep PlanWeaveStep(
        double posX,
        double posY,
        double posZ,
        double yawDeg,
        int weaveXy,
        int weaveZ,
        int xySpeed,
        int zSpeed,
        double xyDist,
        double zDist)
    {
        var nextXy = weaveXy;
        var nextZ = weaveZ;
        var x = posX;
        var y = posY;
        var z = posZ;
        if (xyDist != 0 && xySpeed != 0)
        {
            var angleRad = (yawDeg + 90) * Math.PI / 180.0;
            var dist = WeaveOffset(weaveXy, xyDist);
            x -= Math.Cos(angleRad) * dist;
            y -= Math.Sin(angleRad) * dist;
            nextXy = AdvanceWeaveIndex(weaveXy, xySpeed);
            dist = WeaveOffset(nextXy, xyDist);
            x += Math.Cos(angleRad) * dist;
            y += Math.Sin(angleRad) * dist;
        }

        if (zDist != 0 && zSpeed != 0)
        {
            z -= WeaveOffset(weaveZ, zDist);
            nextZ = AdvanceWeaveIndex(weaveZ, zSpeed);
            z += WeaveOffset(nextZ, zDist);
        }

        return new WeaveStep(x, y, z, nextXy, nextZ);
    }

    public static bool LineEffectExecutes(bool lineDone, int special) =>
        !lineDone && special != 0;

    public static bool WolfAttackRollHits(int randomRoll, int hitchance) => randomRoll < hitchance;
}
